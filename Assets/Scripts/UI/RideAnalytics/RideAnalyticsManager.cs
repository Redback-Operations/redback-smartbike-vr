using System;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;

public class RideAnalyticsManager : MonoBehaviour
{
    public static RideAnalyticsManager Instance { get; private set; }

    [Header("Data Source")]
    [SerializeField] private bool useMockDataWhenMqttUnavailable = false;
    [SerializeField] private bool subscribeToMqtt = true;
    [SerializeField] private bool deriveSpeedFromMovement = true;
    [SerializeField] private Transform movementSource;
    [SerializeField] private Rigidbody movementRigidbody;
    [SerializeField] private float movementSpeedScale = 3.6f;
    [SerializeField] private float movementNoiseFloorKmh = 0.2f;
    [SerializeField] private bool requireRideInputForMovement = true;
    [SerializeField] private float rideInputDeadZone = 0.05f;
    [SerializeField] private float coastDecelerationKmhPerSecond = 8f;
    [SerializeField] private float externalValueTimeoutSeconds = 3f;

    [Header("Ride Settings")]
    [SerializeField] private float riderWeightKg = 75f;
    [SerializeField] private float defaultPersonalBestKmh = 45.4f;
    [SerializeField] private int checkpointsTotal = 8;
    [SerializeField] private int currentGear = 6;
    [SerializeField] private bool deriveGearFromSpeed = true;
    [SerializeField] private int maxGear = 8;

    [Header("Fallback Values")]
    [SerializeField] private float mockBaseSpeedKmh = 39.4f;
    [SerializeField] private float mockCadenceRpm = 82f;
    [SerializeField] private float mockHeartRateBpm = 132f;

    private readonly object mqttLock = new object();
    private bool mqttSubscribed;
    private float mqttSpeedKmh;
    private float mqttCadenceRpm;
    private float mqttHeartRateBpm;
    private float mqttPowerWatts;
    private float rideStartTime;
    private float movingRideTimeSeconds;
    private float distanceKm;
    private float caloriesKcal;
    private float maxSpeedKmh;
    private bool rideActive;
    private bool ridePaused;
    private float ridePausedAt;
    private float pausedDurationSeconds;
    private float rideEndTime;
    private Vector3 previousMovementPosition;
    private bool hasPreviousMovementPosition;
    private float movementSpeedKmh;
    private float lastSpeedValueTime = -999f;
    private float lastCadenceValueTime = -999f;
    private float lastHeartRateValueTime = -999f;
    private float lastPowerValueTime = -999f;
    private float lastMqttSpeedValueTime = -999f;
    private float lastMqttCadenceValueTime = -999f;
    private float lastMqttHeartRateValueTime = -999f;
    private float lastMqttPowerValueTime = -999f;
    private float lastBackendHudValueTime = -999f;

    public RideAnalyticsSnapshot Snapshot { get; private set; } = new RideAnalyticsSnapshot();
    public bool IsUsingMovementDrivenValues => deriveSpeedFromMovement;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        BeginRide();
        ResolveMovementSource();
        Snapshot.personalBestKmh = defaultPersonalBestKmh;
        Snapshot.currentMission = "Free Ride";
        Snapshot.checkpointsTotal = checkpointsTotal;
        Snapshot.currentGear = deriveGearFromSpeed ? 0 : currentGear;
    }

    private void OnDestroy()
    {
        if (mqttSubscribed && Mqtt.Instance != null)
            Mqtt.Instance.Unsubscribe(OnMqttMessage);

        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        TrySubscribeToMqtt();
        UpdateMovementSpeed();

        if (IsExternalValueFresh(lastBackendHudValueTime))
            return;

        if (!rideActive || ridePaused)
        {
            Snapshot.rideTimeSeconds = movingRideTimeSeconds;
            return;
        }

        float speed = GetCurrentSpeed();
        float cadence = GetCadence(speed);
        float heartRate = GetHeartRate(speed);
        float power = GetPowerWatts(speed);
        bool isMoving = speed > movementNoiseFloorKmh;

        if (isMoving)
            movingRideTimeSeconds += Time.deltaTime;

        distanceKm += speed * Time.deltaTime / 3600f;
        if (isMoving)
            caloriesKcal += EstimateCaloriesDelta(power, speed);

        maxSpeedKmh = Mathf.Max(maxSpeedKmh, speed);
        if (deriveGearFromSpeed)
            currentGear = EstimateGear(speed);

        Snapshot.currentSpeedKmh = speed;
        Snapshot.averageSpeedKmh = movingRideTimeSeconds <= 0f ? 0f : distanceKm / (movingRideTimeSeconds / 3600f);
        Snapshot.maxSpeedKmh = maxSpeedKmh;
        Snapshot.cadenceRpm = cadence;
        Snapshot.distanceKm = distanceKm;
        Snapshot.caloriesKcal = caloriesKcal;
        Snapshot.heartRateBpm = heartRate;
        Snapshot.rideTimeSeconds = movingRideTimeSeconds;
        Snapshot.currentMission = GetMissionName();
        Snapshot.checkpointsCompleted = EstimateCheckpointProgress();
        Snapshot.checkpointsTotal = checkpointsTotal;
        Snapshot.personalBestKmh = Mathf.Max(defaultPersonalBestKmh, maxSpeedKmh);
        Snapshot.currentGear = currentGear;
    }

    public void SetApiValues(float speedKmh, float cadenceRpm, float heartRateBpm, float powerWatts, int gear)
    {
        lock (mqttLock)
        {
            mqttSpeedKmh = Mathf.Max(0f, speedKmh);
            mqttCadenceRpm = Mathf.Max(0f, cadenceRpm);
            mqttHeartRateBpm = Mathf.Max(0f, heartRateBpm);
            mqttPowerWatts = Mathf.Max(0f, powerWatts);
            currentGear = Mathf.Max(0, gear);
            lastSpeedValueTime = Time.time;
            lastCadenceValueTime = Time.time;
            lastHeartRateValueTime = Time.time;
            lastPowerValueTime = Time.time;
        }
    }

    public void SetBackendHudValues(
        float speedKmh,
        float cadenceRpm,
        float heartRateBpm,
        float powerWatts,
        int gear,
        float backendDistanceKm,
        float backendCaloriesKcal,
        float backendRideTimeSeconds,
        float backendAverageSpeedKmh,
        float backendMaxSpeedKmh,
        float backendProgressPercent)
    {
        SetApiValues(speedKmh, cadenceRpm, heartRateBpm, powerWatts, gear);

        float progress = Mathf.Clamp01(backendProgressPercent / 100f);
        int completedCheckpoints = checkpointsTotal > 0
            ? Mathf.RoundToInt(progress * checkpointsTotal)
            : 0;

        distanceKm = Mathf.Max(0f, backendDistanceKm);
        caloriesKcal = Mathf.Max(0f, backendCaloriesKcal);
        movingRideTimeSeconds = Mathf.Max(0f, backendRideTimeSeconds);
        maxSpeedKmh = Mathf.Max(0f, backendMaxSpeedKmh);

        Snapshot.currentSpeedKmh = Mathf.Max(0f, speedKmh);
        Snapshot.averageSpeedKmh = Mathf.Max(0f, backendAverageSpeedKmh);
        Snapshot.maxSpeedKmh = maxSpeedKmh;
        Snapshot.cadenceRpm = Mathf.Max(0f, cadenceRpm);
        Snapshot.distanceKm = distanceKm;
        Snapshot.caloriesKcal = caloriesKcal;
        Snapshot.heartRateBpm = Mathf.Max(0f, heartRateBpm);
        Snapshot.rideTimeSeconds = movingRideTimeSeconds;
        Snapshot.currentMission = GetMissionName();
        Snapshot.checkpointsCompleted = Mathf.Clamp(completedCheckpoints, 0, checkpointsTotal);
        Snapshot.checkpointsTotal = checkpointsTotal;
        Snapshot.personalBestKmh = Mathf.Max(defaultPersonalBestKmh, maxSpeedKmh);
        Snapshot.currentGear = Mathf.Max(0, gear);
        lastBackendHudValueTime = Time.time;
    }

    public void BeginRide()
    {
        rideActive = true;
        ridePaused = false;
        rideStartTime = Time.time;
        movingRideTimeSeconds = 0f;
        ridePausedAt = 0f;
        pausedDurationSeconds = 0f;
        rideEndTime = 0f;
        distanceKm = 0f;
        caloriesKcal = 0f;
        maxSpeedKmh = 0f;
        hasPreviousMovementPosition = false;
        movementSpeedKmh = 0f;

        Snapshot = new RideAnalyticsSnapshot
        {
            personalBestKmh = defaultPersonalBestKmh,
            currentMission = "Free Ride",
            checkpointsTotal = checkpointsTotal,
            currentGear = deriveGearFromSpeed ? 0 : currentGear
        };
    }

    public void PauseRide()
    {
        if (!rideActive || ridePaused)
            return;

        ridePaused = true;
        ridePausedAt = Time.time;
    }

    public void ResumeRide()
    {
        if (!rideActive || !ridePaused)
            return;

        pausedDurationSeconds += Time.time - ridePausedAt;
        ridePaused = false;
        ridePausedAt = 0f;
    }

    public RideAnalyticsSnapshot EndRide()
    {
        if (!rideActive)
            return Snapshot;

        rideEndTime = Time.time;
        rideActive = false;
        ridePaused = false;
        Snapshot.rideTimeSeconds = movingRideTimeSeconds;
        return Snapshot;
    }

    private void TrySubscribeToMqtt()
    {
        if (!subscribeToMqtt || mqttSubscribed || Mqtt.Instance == null || !Mqtt.Instance.IsConnected)
            return;

        Mqtt.Instance.Subscribe(OnMqttMessage, Mqtt.SpeedTopic, Mqtt.CadenceTopic, Mqtt.HeartRateTopic, Mqtt.PowerTopic);
        mqttSubscribed = true;
    }

    private void OnMqttMessage(object sender, MqttMsgPublishEventArgs e)
    {
        string message = System.Text.Encoding.UTF8.GetString(e.Message);
        float value;

        lock (mqttLock)
        {
            if (e.Topic == Mqtt.SpeedTopic)
            {
                if (!MqttFieldParser.TryReadFloat(message, out value, "speed", "value"))
                    return;

                mqttSpeedKmh = Mathf.Max(0f, value);
                lastSpeedValueTime = Time.time;
                lastMqttSpeedValueTime = Time.time;
            }
            else if (e.Topic == Mqtt.CadenceTopic)
            {
                if (!MqttFieldParser.TryReadFloat(message, out value, "cadence", "rpm", "value"))
                    return;

                mqttCadenceRpm = Mathf.Max(0f, value);
                lastCadenceValueTime = Time.time;
                lastMqttCadenceValueTime = Time.time;
            }
            else if (e.Topic == Mqtt.HeartRateTopic)
            {
                if (!MqttFieldParser.TryReadFloat(message, out value, "heartrate", "heartRate", "heart_rate", "bpm", "value"))
                    return;

                mqttHeartRateBpm = Mathf.Max(0f, value);
                lastHeartRateValueTime = Time.time;
                lastMqttHeartRateValueTime = Time.time;
            }
            else if (e.Topic == Mqtt.PowerTopic)
            {
                if (!MqttFieldParser.TryReadFloat(message, out value, "power", "watts", "value"))
                    return;

                mqttPowerWatts = Mathf.Max(0f, value);
                lastPowerValueTime = Time.time;
                lastMqttPowerValueTime = Time.time;
            }
        }
    }

    private float GetCurrentSpeed()
    {
        lock (mqttLock)
        {
            if (IsExternalValueFresh(lastMqttSpeedValueTime))
                return mqttSpeedKmh;

            if (!deriveSpeedFromMovement && IsExternalValueFresh(lastSpeedValueTime))
                return mqttSpeedKmh;
        }

        if (deriveSpeedFromMovement)
            return movementSpeedKmh;

        return useMockDataWhenMqttUnavailable ? mockBaseSpeedKmh : 0f;
    }

    private float GetCadence(float speedKmh)
    {
        if (speedKmh <= movementNoiseFloorKmh)
            return 0f;

        lock (mqttLock)
        {
            if (IsExternalValueFresh(lastMqttCadenceValueTime))
                return mqttCadenceRpm;
        }

        if (!useMockDataWhenMqttUnavailable)
            return 0f;

        return Mathf.Clamp(mockCadenceRpm * Mathf.InverseLerp(0f, 35f, speedKmh), 0f, mockCadenceRpm);
    }

    private float GetHeartRate(float speedKmh)
    {
        if (speedKmh <= movementNoiseFloorKmh)
            return 0f;

        lock (mqttLock)
        {
            if (IsExternalValueFresh(lastMqttHeartRateValueTime))
                return mqttHeartRateBpm;
        }

        if (!useMockDataWhenMqttUnavailable)
            return 0f;

        return Mathf.Lerp(90f, mockHeartRateBpm, Mathf.InverseLerp(8f, 32f, speedKmh));
    }

    private float GetPowerWatts(float speedKmh)
    {
        if (speedKmh <= movementNoiseFloorKmh)
            return 0f;

        lock (mqttLock)
        {
            if (IsExternalValueFresh(lastMqttPowerValueTime))
                return mqttPowerWatts;
        }

        return Mathf.Max(80f, speedKmh * 5.2f);
    }

    private void ResolveMovementSource()
    {
        if (movementSource != null)
        {
            Transform playerRoot = GetPlayerRoot(movementSource);
            if (playerRoot != null && playerRoot != movementSource)
            {
                movementSource = playerRoot;
                movementRigidbody = null;
            }
            else if (IsLikelyBikeVisualTransform(movementSource))
            {
                movementSource = null;
                movementRigidbody = null;
            }
        }

        if (movementSource == null)
        {
            GameObject player = null;
            try
            {
                player = GameObject.FindWithTag("Player");
            }
            catch (UnityException)
            {
                player = null;
            }

            if (player == null)
                player = GameObject.Find("Player_New(Clone)");
            if (player == null)
                player = GameObject.Find("Player_2");
            if (player == null)
                player = GameObject.Find("Player_1");
            if (player != null)
                movementSource = player.transform;

            if (movementSource == null)
            {
                PlayerController playerController = FindObjectOfType<PlayerController>();
                if (playerController != null)
                    movementSource = playerController.transform;
            }
        }

        if (movementRigidbody == null && movementSource != null)
            movementRigidbody = movementSource.GetComponent<Rigidbody>() ?? movementSource.GetComponentInChildren<Rigidbody>();
    }

    private void UpdateMovementSpeed()
    {
        if (!deriveSpeedFromMovement)
            return;

        if (movementSource == null || IsLikelyBikeVisualTransform(movementSource))
            ResolveMovementSource();

        if (requireRideInputForMovement && !HasRideInput() && !HasFreshMqttSpeed())
        {
            movementSpeedKmh = Mathf.MoveTowards(
                movementSpeedKmh,
                0f,
                Mathf.Max(0.1f, coastDecelerationKmhPerSecond) * Time.deltaTime);

            if (movementSpeedKmh < movementNoiseFloorKmh)
                movementSpeedKmh = 0f;

            if (movementSource != null)
            {
                previousMovementPosition = movementSource.position;
                hasPreviousMovementPosition = true;
            }

            return;
        }

        if (movementRigidbody != null && !movementRigidbody.isKinematic)
        {
            Vector3 velocity = movementRigidbody.velocity;
            velocity.y = 0f;
            SetMovementSpeed(velocity.magnitude * movementSpeedScale);
            return;
        }

        if (movementSource == null)
        {
            SetMovementSpeed(0f);
            return;
        }

        if (!hasPreviousMovementPosition)
        {
            previousMovementPosition = movementSource.position;
            hasPreviousMovementPosition = true;
            SetMovementSpeed(0f);
            return;
        }

        float deltaSeconds = Mathf.Max(Time.deltaTime, 0.0001f);
        Vector3 delta = movementSource.position - previousMovementPosition;
        delta.y = 0f;
        previousMovementPosition = movementSource.position;
        SetMovementSpeed(delta.magnitude / deltaSeconds * movementSpeedScale);
    }

    private void SetMovementSpeed(float speedKmh)
    {
        float clampedSpeed = Mathf.Max(0f, speedKmh);
        if (clampedSpeed < movementNoiseFloorKmh)
            clampedSpeed = 0f;

        if (clampedSpeed <= 0f)
        {
            movementSpeedKmh = 0f;
            return;
        }

        movementSpeedKmh = Mathf.Lerp(movementSpeedKmh, clampedSpeed, 0.35f);
        if (movementSpeedKmh < movementNoiseFloorKmh)
            movementSpeedKmh = 0f;
    }

    private bool IsExternalValueFresh(float lastValueTime)
    {
        return Time.time - lastValueTime <= externalValueTimeoutSeconds;
    }

    private bool HasFreshMqttSpeed()
    {
        return IsExternalValueFresh(lastMqttSpeedValueTime) && mqttSpeedKmh > movementNoiseFloorKmh;
    }

    private bool HasRideInput()
    {
        return Mathf.Abs(Input.GetAxisRaw("Vertical")) > rideInputDeadZone
            || Input.GetKey(KeyCode.W)
            || Input.GetKey(KeyCode.UpArrow)
            || Input.GetKey(KeyCode.S)
            || Input.GetKey(KeyCode.DownArrow);
    }

    private Transform GetPlayerRoot(Transform source)
    {
        PlayerController playerController = source.GetComponentInParent<PlayerController>();
        if (playerController != null)
            return playerController.transform;

        Transform current = source;
        while (current.parent != null)
        {
            current = current.parent;
            if (current.name.StartsWith("Player_", StringComparison.OrdinalIgnoreCase))
                return current;
        }

        return null;
    }

    private bool IsLikelyBikeVisualTransform(Transform source)
    {
        if (source == null)
            return false;

        return string.Equals(source.name, "Bikes", StringComparison.OrdinalIgnoreCase)
            || source.GetComponentInParent<PlayerController>() != null && source.GetComponent<PlayerController>() == null;
    }

    private int EstimateGear(float speedKmh)
    {
        if (speedKmh <= movementNoiseFloorKmh)
            return 0;

        int gearCount = Mathf.Max(1, maxGear);
        float normalizedSpeed = Mathf.InverseLerp(0f, Mathf.Max(1f, mockBaseSpeedKmh), speedKmh);
        return Mathf.Clamp(Mathf.CeilToInt(normalizedSpeed * gearCount), 1, gearCount);
    }

    private float EstimateCaloriesDelta(float powerWatts, float speedKmh)
    {
        float elapsedHours = Time.deltaTime / 3600f;
        float met = Mathf.Lerp(6f, 12f, Mathf.InverseLerp(12f, 36f, speedKmh));
        float caloriesFromMet = met * riderWeightKg * elapsedHours;
        float caloriesFromPower = powerWatts * Time.deltaTime / 4184f;
        return Mathf.Max(caloriesFromMet, caloriesFromPower);
    }

    private float GetRideElapsedSeconds()
    {
        if (rideStartTime <= 0f)
            return 0f;

        float endTime = rideActive ? Time.time : rideEndTime;
        if (ridePaused)
            endTime = ridePausedAt;

        return Mathf.Max(0f, endTime - rideStartTime - pausedDurationSeconds);
    }

    private string GetMissionName()
    {
        if (Mission_Activator.ActiveMission != null)
            return Mission_Activator.ActiveMission.MissionName;

        return "Free Ride";
    }

    private int EstimateCheckpointProgress()
    {
        if (checkpointsTotal <= 0)
            return 0;

        return Mathf.Clamp(Mathf.FloorToInt(distanceKm * 1.6f), 0, checkpointsTotal);
    }

}
