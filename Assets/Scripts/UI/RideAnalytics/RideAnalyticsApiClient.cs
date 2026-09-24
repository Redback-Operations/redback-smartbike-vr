using System;
using System.Collections;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class RideAnalyticsApiClient : MonoBehaviour
{
    [Header("Backend")]
    [SerializeField] private string apiBaseUrl = "http://localhost:5000";
    [SerializeField] private string ridesEndpointPath = "/api/rides";
    [SerializeField] private string rideDetailsEndpointTemplate = "/api/rides/{ride_id}";
    [SerializeField] private string sensorDataEndpointTemplate = "/api/rides/{ride_id}/sensor-data";
    [SerializeField] private string userId;
    [SerializeField] private string currentRideId;

    [Header("Polling")]
    [SerializeField] private bool pollDashboardHud;
    [SerializeField] private float pollIntervalSeconds = 2f;
    [SerializeField] private float targetDistanceKm = 30f;
    [SerializeField] private int fallbackGear = 6;

    [Header("References")]
    [SerializeField] private RideAnalyticsManager analyticsManager;

    private Coroutine pollingRoutine;

    public string CurrentRideId => currentRideId;

    private void Awake()
    {
        if (analyticsManager == null)
            analyticsManager = RideAnalyticsManager.Instance;
    }

    private void OnEnable()
    {
        if (pollDashboardHud)
            StartHudPolling();
    }

    private void OnDisable()
    {
        StopHudPolling();
    }

    [ContextMenu("Start Backend Ride")]
    public void StartBackendRide()
    {
        StartCoroutine(StartRideRoutine());
    }

    [ContextMenu("End Backend Ride")]
    public void EndBackendRide()
    {
        StartCoroutine(EndRideRoutine());
    }

    [ContextMenu("Fetch Ride Data Once")]
    public void FetchHudOnce()
    {
        StartCoroutine(FetchHudRoutine());
    }

    public void StartHudPolling()
    {
        pollDashboardHud = true;

        if (pollingRoutine == null)
            pollingRoutine = StartCoroutine(PollHudRoutine());
    }

    public void StopHudPolling()
    {
        pollDashboardHud = false;

        if (pollingRoutine != null)
        {
            StopCoroutine(pollingRoutine);
            pollingRoutine = null;
        }
    }

    public void SetCurrentRideId(string rideId)
    {
        currentRideId = rideId;
    }

    private IEnumerator StartRideRoutine()
    {
        if (analyticsManager == null)
            analyticsManager = RideAnalyticsManager.Instance;

        if (string.IsNullOrWhiteSpace(userId))
        {
            Debug.LogWarning("RideAnalyticsApiClient needs a Supabase profile userId before starting a backend ride.");
            analyticsManager?.BeginRide();
            yield break;
        }

        string body = "{\"user_id\":\"" + EscapeJson(userId) + "\"}";
        using (UnityWebRequest request = CreateJsonRequest("POST", "/api/rides/start", body))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Failed to start backend ride: " + request.error + " " + request.downloadHandler.text);
                analyticsManager?.BeginRide();
                yield break;
            }

            StartRideResponse response = JsonUtility.FromJson<StartRideResponse>(request.downloadHandler.text);
            currentRideId = response != null ? response.ride_id : null;
            analyticsManager?.BeginRide();
            Debug.Log("Backend ride started: " + currentRideId);
        }
    }

    private IEnumerator EndRideRoutine()
    {
        if (analyticsManager == null)
            analyticsManager = RideAnalyticsManager.Instance;

        RideAnalyticsSnapshot snapshot = analyticsManager != null ? analyticsManager.EndRide() : null;

        if (string.IsNullOrWhiteSpace(currentRideId) || snapshot == null)
        {
            Debug.LogWarning("No backend ride_id available. Ended local ride only.");
            yield break;
        }

        string body = string.Format(
            CultureInfo.InvariantCulture,
            "{{\"duration\":{0},\"distance\":{1},\"avg_speed\":{2},\"calories\":{3}}}",
            Mathf.RoundToInt(snapshot.rideTimeSeconds),
            snapshot.distanceKm,
            snapshot.averageSpeedKmh,
            snapshot.caloriesKcal);

        using (UnityWebRequest request = CreateJsonRequest("POST", "/api/rides/" + UnityWebRequest.EscapeURL(currentRideId) + "/end", body))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Failed to end backend ride: " + request.error + " " + request.downloadHandler.text);
                yield break;
            }

            Debug.Log("Backend ride ended: " + request.downloadHandler.text);
        }
    }

    private IEnumerator PollHudRoutine()
    {
        while (pollDashboardHud)
        {
            yield return FetchHudRoutine();
            yield return new WaitForSeconds(Mathf.Max(0.5f, pollIntervalSeconds));
        }

        pollingRoutine = null;
    }

    private IEnumerator FetchHudRoutine()
    {
        if (analyticsManager == null)
            analyticsManager = RideAnalyticsManager.Instance;

        string ridePath = BuildRideFetchPath();
        if (string.IsNullOrWhiteSpace(ridePath))
        {
            Debug.LogWarning("RideAnalyticsApiClient needs Current Ride Id before it can fetch the backend ride detail endpoint.");
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequest.Get(BuildUrl(ridePath)))
        {
            request.timeout = 5;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Failed to fetch ride data: " + request.error + " " + request.downloadHandler.text);
                yield break;
            }

            RideApiRecord ride = ParseRideResponse(request.downloadHandler.text);
            if (ride == null)
            {
                Debug.LogWarning("Ride data response did not contain any rides.");
                yield break;
            }

            currentRideId = string.IsNullOrWhiteSpace(currentRideId) ? ride.ride_id : currentRideId;
            if (!ride.HasSensorData && !string.IsNullOrWhiteSpace(ride.ride_id))
                yield return FetchSensorDataRoutine(ride);

            SensorDataRecord latestSensor = ride.LatestSensor();
            RideApiMetrics metrics = RideApiMetrics.FromRide(ride, latestSensor, targetDistanceKm);

            analyticsManager?.SetBackendHudValues(
                metrics.currentSpeedKmh,
                metrics.cadenceRpm,
                metrics.heartRateBpm,
                metrics.powerWatts,
                fallbackGear,
                metrics.distanceKm,
                metrics.caloriesKcal,
                metrics.rideTimeSeconds,
                metrics.averageSpeedKmh,
                metrics.maxSpeedKmh,
                metrics.progressPercent);
        }
    }

    private string BuildRideFetchPath()
    {
        if (!string.IsNullOrWhiteSpace(currentRideId))
            return rideDetailsEndpointTemplate.Replace("{ride_id}", UnityWebRequest.EscapeURL(currentRideId));

        return ridesEndpointPath;
    }

    private IEnumerator FetchSensorDataRoutine(RideApiRecord ride)
    {
        if (ride == null || string.IsNullOrWhiteSpace(ride.ride_id) || string.IsNullOrWhiteSpace(sensorDataEndpointTemplate))
            yield break;

        string path = sensorDataEndpointTemplate.Replace("{ride_id}", UnityWebRequest.EscapeURL(ride.ride_id));
        using (UnityWebRequest request = UnityWebRequest.Get(BuildUrl(path)))
        {
            request.timeout = 5;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Ride data loaded, but sensor data could not be fetched: " + request.error + " " + request.downloadHandler.text);
                yield break;
            }

            SensorDataRecord[] records = ParseSensorDataResponse(request.downloadHandler.text);
            if (records != null && records.Length > 0)
                ride.sensor_data = records;
        }
    }

    private RideApiRecord ParseRideResponse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        RideApiRecord directRide = TryParseRide(json);
        if (directRide != null && !string.IsNullOrWhiteSpace(directRide.ride_id))
            return directRide;

        RideApiSingleResponse singleResponse = TryParseSingleRideResponse(json);
        if (singleResponse != null && singleResponse.Ride != null && !string.IsNullOrWhiteSpace(singleResponse.Ride.ride_id))
            return singleResponse.Ride;

        RideApiResponse wrapped = TryParseRideResponse(json);
        if (wrapped == null || wrapped.Rides == null || wrapped.Rides.Length == 0)
            return null;

        RideApiRecord selectedRide = SelectRide(wrapped.Rides);
        if (selectedRide != null)
            return selectedRide;

        return wrapped.Rides[wrapped.Rides.Length - 1];
    }

    private RideApiSingleResponse TryParseSingleRideResponse(string json)
    {
        try
        {
            return JsonUtility.FromJson<RideApiSingleResponse>(json);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private RideApiResponse TryParseRideResponse(string json)
    {
        try
        {
            string trimmed = json.TrimStart();
            if (trimmed.StartsWith("[", StringComparison.Ordinal))
                return JsonUtility.FromJson<RideApiResponse>("{\"rides\":" + json + "}");

            RideApiResponse response = JsonUtility.FromJson<RideApiResponse>(json);
            if (response != null && response.Rides != null && response.Rides.Length > 0)
                return response;

            ApiDataResponse dataResponse = JsonUtility.FromJson<ApiDataResponse>(json);
            if (dataResponse != null && dataResponse.data != null && dataResponse.data.Length > 0)
                return new RideApiResponse { rides = dataResponse.data };

            return null;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Could not parse ride list response: " + ex.Message);
            return null;
        }
    }

    private RideApiRecord TryParseRide(string json)
    {
        try
        {
            return JsonUtility.FromJson<RideApiRecord>(json);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private RideApiRecord SelectRide(RideApiRecord[] rides)
    {
        if (rides == null || rides.Length == 0)
            return null;

        if (!string.IsNullOrWhiteSpace(currentRideId))
        {
            for (int i = 0; i < rides.Length; i++)
            {
                if (rides[i] != null && string.Equals(rides[i].ride_id, currentRideId, StringComparison.OrdinalIgnoreCase))
                    return rides[i];
            }
        }

        RideApiRecord newestRide = null;
        DateTime newestStart = DateTime.MinValue;

        for (int i = 0; i < rides.Length; i++)
        {
            RideApiRecord ride = rides[i];
            if (ride == null)
                continue;

            DateTime rideStart;
            if (DateTime.TryParse(ride.start_time, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out rideStart)
                && (newestRide == null || rideStart > newestStart))
            {
                newestRide = ride;
                newestStart = rideStart;
            }
        }

        return newestRide ?? rides[rides.Length - 1];
    }

    private SensorDataRecord[] ParseSensorDataResponse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            string trimmed = json.TrimStart();
            if (trimmed.StartsWith("[", StringComparison.Ordinal))
                return JsonUtility.FromJson<SensorDataApiResponse>("{\"sensor_data\":" + json + "}").SensorRecords;

            SensorDataApiResponse response = JsonUtility.FromJson<SensorDataApiResponse>(json);
            if (response != null && response.SensorRecords != null && response.SensorRecords.Length > 0)
                return response.SensorRecords;

            SensorDataDataResponse dataResponse = JsonUtility.FromJson<SensorDataDataResponse>(json);
            return dataResponse != null ? dataResponse.data : null;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Could not parse sensor data response: " + ex.Message);
            return null;
        }
    }

    private UnityWebRequest CreateJsonRequest(string method, string path, string body)
    {
        UnityWebRequest request = new UnityWebRequest(BuildUrl(path), method);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        return request;
    }

    private string BuildUrl(string path)
    {
        return apiBaseUrl.TrimEnd('/') + "/" + path.TrimStart('/');
    }

    private static string EscapeJson(string value)
    {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    [Serializable]
    private class StartRideResponse
    {
        public string ride_id;
    }

    [Serializable]
    private class RideApiResponse
    {
        public RideApiRecord[] rides;

        public RideApiRecord[] Rides
        {
            get { return rides; }
        }
    }

    [Serializable]
    private class ApiDataResponse
    {
        public RideApiRecord[] data;
    }

    [Serializable]
    private class RideApiSingleResponse
    {
        public RideApiRecord ride;
        public RideApiRecord data;

        public RideApiRecord Ride
        {
            get
            {
                return ride != null ? ride : data;
            }
        }
    }

    [Serializable]
    private class RideApiRecord
    {
        public string ride_id;
        public string start_time;
        public string end_time;
        public float distance;
        public float avg_speed;
        public float calories;
        public string duration;
        public SensorDataRecord[] sensor_data;
        public SensorDataRecord[] sensorData;

        public bool HasSensorData
        {
            get
            {
                SensorDataRecord[] records = SensorRecords;
                return records != null && records.Length > 0;
            }
        }

        public SensorDataRecord LatestSensor()
        {
            SensorDataRecord[] records = SensorRecords;
            if (records == null || records.Length == 0)
                return null;

            SensorDataRecord latest = records[0];
            DateTime latestTime = ParseDateTime(latest.timestamp);

            for (int i = 1; i < records.Length; i++)
            {
                SensorDataRecord record = records[i];
                if (record == null)
                    continue;

                DateTime recordTime = ParseDateTime(record.timestamp);
                if (recordTime >= latestTime)
                {
                    latest = record;
                    latestTime = recordTime;
                }
            }

            return latest;
        }

        public float MaxSensorSpeed()
        {
            SensorDataRecord[] records = SensorRecords;
            if (records == null || records.Length == 0)
                return 0f;

            float maxSpeed = 0f;
            for (int i = 0; i < records.Length; i++)
            {
                if (records[i] != null)
                    maxSpeed = Mathf.Max(maxSpeed, records[i].speed);
            }

            return maxSpeed;
        }

        public SensorDataRecord[] SensorRecords
        {
            get
            {
                if (sensor_data != null && sensor_data.Length > 0)
                    return sensor_data;

                return sensorData;
            }
        }
    }

    [Serializable]
    private class SensorDataRecord
    {
        public string timestamp;
        public float speed;
        public float cadence;
        public float heart_rate;
        public float heartRate;
        public float power;

        public float HeartRateBpm
        {
            get { return heart_rate > 0f ? heart_rate : heartRate; }
        }
    }

    [Serializable]
    private class SensorDataApiResponse
    {
        public SensorDataRecord[] sensor_data;
        public SensorDataRecord[] sensorData;

        public SensorDataRecord[] SensorRecords
        {
            get
            {
                if (sensor_data != null && sensor_data.Length > 0)
                    return sensor_data;

                return sensorData;
            }
        }
    }

    [Serializable]
    private class SensorDataDataResponse
    {
        public SensorDataRecord[] data;
    }

    private struct RideApiMetrics
    {
        public float currentSpeedKmh;
        public float cadenceRpm;
        public float heartRateBpm;
        public float powerWatts;
        public float distanceKm;
        public float caloriesKcal;
        public float rideTimeSeconds;
        public float averageSpeedKmh;
        public float maxSpeedKmh;
        public float progressPercent;

        public static RideApiMetrics FromRide(RideApiRecord ride, SensorDataRecord latestSensor, float targetDistanceKm)
        {
            float distanceKm = Mathf.Max(0f, ride.distance);
            float maxSpeedKmh = Mathf.Max(ride.MaxSensorSpeed(), latestSensor != null ? latestSensor.speed : 0f);
            float durationSeconds = ParseDurationSeconds(ride.duration);

            if (durationSeconds <= 0f)
                durationSeconds = ParseElapsedSeconds(ride.start_time, ride.end_time);

            float averageSpeed = ride.avg_speed > 0f
                ? ride.avg_speed
                : durationSeconds > 0f ? distanceKm / (durationSeconds / 3600f) : 0f;

            return new RideApiMetrics
            {
                currentSpeedKmh = latestSensor != null ? Mathf.Max(0f, latestSensor.speed) : 0f,
                cadenceRpm = latestSensor != null ? Mathf.Max(0f, latestSensor.cadence) : 0f,
                heartRateBpm = latestSensor != null ? Mathf.Max(0f, latestSensor.HeartRateBpm) : 0f,
                powerWatts = latestSensor != null ? Mathf.Max(0f, latestSensor.power) : 0f,
                distanceKm = distanceKm,
                caloriesKcal = Mathf.Max(0f, ride.calories),
                rideTimeSeconds = durationSeconds,
                averageSpeedKmh = Mathf.Max(0f, averageSpeed),
                maxSpeedKmh = Mathf.Max(0f, maxSpeedKmh),
                progressPercent = targetDistanceKm <= 0f ? 0f : Mathf.Clamp01(distanceKm / targetDistanceKm) * 100f
            };
        }
    }

    private static DateTime ParseDateTime(string value)
    {
        DateTime parsed;
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out parsed)
            ? parsed
            : DateTime.MinValue;
    }

    private static float ParseDurationSeconds(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0f;

        TimeSpan duration;
        return TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out duration)
            ? (float)duration.TotalSeconds
            : 0f;
    }

    private static float ParseElapsedSeconds(string startTime, string endTime)
    {
        DateTime start = ParseDateTime(startTime);
        DateTime end = ParseDateTime(endTime);

        if (start == DateTime.MinValue || end == DateTime.MinValue || end <= start)
            return 0f;

        return (float)(end - start).TotalSeconds;
    }

}
