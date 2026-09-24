using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Redback.UI;

public class RideAnalyticsDashboard : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private RideAnalyticsManager analyticsManager;
    [SerializeField] private RideAnalyticsApiClient apiClient;

    [Header("Metric Labels")]
    [SerializeField] private TMP_Text currentSpeedText;
    [SerializeField] private TMP_Text averageSpeedText;
    [SerializeField] private TMP_Text maxSpeedText;
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private TMP_Text rideDurationText;
    [SerializeField] private TMP_Text caloriesText;
    [SerializeField] private TMP_Text heartRateText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text gearText;
    [SerializeField] private TMP_Text missionText;
    [SerializeField] private TMP_Text checkpointsText;
    [SerializeField] private TMP_Text personalBestText;

    [Header("Progress")]
    [SerializeField] private Image distanceProgressFill;

    [Header("HUD Pages")]
    [SerializeField] private GameObject pausePage;
    [SerializeField] private GameObject mapPage;
    [SerializeField] private GameObject analyticsPage;
    [SerializeField] private GameObject endRidePage;
    [SerializeField] private TMP_Text pauseButtonText;
    [SerializeField] private TMP_Text pauseSummaryText;
    [SerializeField] private TMP_Text analyticsSummaryText;
    [SerializeField] private TMP_Text endRideSummaryText;

    private bool pausedByHud;
    private bool rideEnded;

    private void Awake()
    {
        if (analyticsManager == null)
            analyticsManager = RideAnalyticsManager.Instance;
        if (apiClient == null)
            apiClient = GetComponentInParent<RideAnalyticsApiClient>();
    }

    private void Update()
    {
        if (analyticsManager == null)
            analyticsManager = RideAnalyticsManager.Instance;

        if (analyticsManager == null || analyticsManager.Snapshot == null)
            return;

        Render(analyticsManager.Snapshot);
    }

    public void Bind(
        RideAnalyticsManager manager,
        TMP_Text currentSpeed,
        TMP_Text averageSpeed,
        TMP_Text maxSpeed,
        TMP_Text distance,
        TMP_Text rideDuration,
        TMP_Text calories,
        TMP_Text heartRate,
        TMP_Text progress,
        TMP_Text gear,
        TMP_Text mission,
        TMP_Text checkpoints,
        TMP_Text personalBest,
        Image progressFill)
    {
        analyticsManager = manager;
        currentSpeedText = currentSpeed;
        averageSpeedText = averageSpeed;
        maxSpeedText = maxSpeed;
        distanceText = distance;
        rideDurationText = rideDuration;
        caloriesText = calories;
        heartRateText = heartRate;
        progressText = progress;
        gearText = gear;
        missionText = mission;
        checkpointsText = checkpoints;
        personalBestText = personalBest;
        distanceProgressFill = progressFill;
    }

    public void BindPages(
        GameObject pause,
        GameObject map,
        GameObject analytics,
        GameObject endRide,
        TMP_Text pauseLabel,
        TMP_Text pauseSummary,
        TMP_Text analyticsSummary,
        TMP_Text endRideSummary)
    {
        pausePage = pause;
        mapPage = map;
        analyticsPage = analytics;
        endRidePage = endRide;
        pauseButtonText = pauseLabel;
        pauseSummaryText = pauseSummary;
        analyticsSummaryText = analyticsSummary;
        endRideSummaryText = endRideSummary;
        HidePages();
    }

    private void Render(RideAnalyticsSnapshot snapshot)
    {
        SetText(currentSpeedText, string.Format("{0:0.0} km/h", snapshot.currentSpeedKmh));
        SetText(averageSpeedText, string.Format("{0:0.0} km/h", snapshot.averageSpeedKmh));
        SetText(maxSpeedText, string.Format("{0:0.0} km/h", snapshot.maxSpeedKmh));
        SetText(distanceText, string.Format("{0:0.00} km", snapshot.distanceKm));
        SetText(rideDurationText, FormatTime(snapshot.rideTimeSeconds));
        SetText(caloriesText, string.Format("{0:0} kcal", snapshot.caloriesKcal));
        SetText(heartRateText, string.Format("{0:0} bpm", snapshot.heartRateBpm));
        SetText(progressText, string.Format("{0:0}%", snapshot.DistanceProgress * 100f));
        SetText(gearText, snapshot.currentGear > 0 ? snapshot.currentGear.ToString() : "-");
        SetText(missionText, snapshot.currentMission);
        SetText(checkpointsText, string.Format("{0}/{1}", snapshot.checkpointsCompleted, snapshot.checkpointsTotal));
        SetText(personalBestText, string.Format("{0:0.0} km/h", snapshot.personalBestKmh));

        if (distanceProgressFill != null)
            distanceProgressFill.fillAmount = snapshot.DistanceProgress;

        SetText(pauseSummaryText, BuildPauseSummary(snapshot));
        SetText(analyticsSummaryText, BuildAnalyticsSummary(snapshot));
    }

    private void SetText(TMP_Text target, string value)
    {
        if (target != null)
            target.text = value;
    }

    private string FormatTime(float seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(Mathf.Max(0f, seconds));
        return string.Format("{0:00}:{1:00}:{2:00}", (int)time.TotalHours, time.Minutes, time.Seconds);
    }

    public void TogglePause()
    {
        if (rideEnded)
            return;

        if (pausedByHud)
        {
            ResumeRideFromHud();
            Debug.Log("Ride Analytics HUD resumed game.");
            return;
        }

        OpenOverlayPage(pausePage, "Pause");
        Debug.Log("Ride Analytics HUD paused game.");
    }

    public void ShowMap()
    {
        if (rideEnded)
            return;

        if (mapPage != null)
        {
            OpenOverlayPage(mapPage, "Map");
            return;
        }

        DashboardController controller = FindObjectOfType<DashboardController>();
        if (controller != null)
        {
            controller.ShowMapDashboardPanel();
            return;
        }

        Debug.Log("Ride Analytics HUD Map button clicked. No DashboardController map panel was found in this scene.");
    }

    public void ShowAnalytics()
    {
        if (rideEnded)
            return;

        if (analyticsPage != null)
        {
            OpenOverlayPage(analyticsPage, "Analytics");
            return;
        }

        DashboardController controller = FindObjectOfType<DashboardController>();
        if (controller != null)
        {
            controller.ShowTripDetailsPanel();
            return;
        }

        Debug.Log("Ride Analytics HUD Analytics button clicked. No DashboardController analytics panel was found in this scene.");
    }

    public void EndRide()
    {
        rideEnded = true;

        RideAnalyticsSnapshot snapshot = analyticsManager != null ? analyticsManager.EndRide() : null;

        if (apiClient == null)
            apiClient = GetComponentInParent<RideAnalyticsApiClient>();

        if (apiClient != null)
            apiClient.EndBackendRide();

        HidePages();
        if (endRidePage != null)
            endRidePage.SetActive(true);
        if (snapshot != null)
            SetText(endRideSummaryText, BuildEndRideSummary(snapshot));
        PauseGameOnly();
        SetText(pauseButtonText, "Ended");

        Debug.Log("Ride Analytics HUD ended ride.");
    }

    public void ReturnToGarage()
    {
        Time.timeScale = 1f;
        MapLoader.LoadScene("GarageScene");
    }

    public void ClosePages()
    {
        HidePages();
        if (!rideEnded)
            ResumeRideFromHud();
    }

    private void HidePages()
    {
        if (pausePage != null)
            pausePage.SetActive(false);
        if (mapPage != null)
            mapPage.SetActive(false);
        if (analyticsPage != null)
            analyticsPage.SetActive(false);
        if (endRidePage != null)
            endRidePage.SetActive(false);
    }

    private void OpenOverlayPage(GameObject page, string source)
    {
        HidePages();
        if (page != null)
            page.SetActive(true);

        PauseRideFromHud();
        Debug.Log("Ride Analytics HUD opened " + source + " overlay and paused gameplay.");
    }

    private void PauseRideFromHud()
    {
        if (analyticsManager == null)
            analyticsManager = RideAnalyticsManager.Instance;

        if (analyticsManager != null)
            analyticsManager.PauseRide();

        PauseGameOnly();
        pausedByHud = true;
        SetText(pauseButtonText, "Resume");
    }

    private void PauseGameOnly()
    {
        Time.timeScale = 0f;
    }

    private void ResumeRideFromHud()
    {
        if (analyticsManager == null)
            analyticsManager = RideAnalyticsManager.Instance;

        if (analyticsManager != null)
            analyticsManager.ResumeRide();

        Time.timeScale = 1f;
        pausedByHud = false;
        HidePages();
        SetText(pauseButtonText, "Pause");
    }

    private string BuildPauseSummary(RideAnalyticsSnapshot snapshot)
    {
        return "Ride paused\n\n"
            + "Current speed: " + snapshot.currentSpeedKmh.ToString("0.0") + " km/h\n"
            + "Distance: " + snapshot.distanceKm.ToString("0.00") + " km\n"
            + "Ride time: " + FormatTime(snapshot.rideTimeSeconds) + "\n\n"
            + "Choose Back to Ride to continue.";
    }

    private string BuildAnalyticsSummary(RideAnalyticsSnapshot snapshot)
    {
        return "Ride analytics\n\n"
            + "Speed: " + snapshot.currentSpeedKmh.ToString("0.0") + " km/h\n"
            + "Average: " + snapshot.averageSpeedKmh.ToString("0.0") + " km/h\n"
            + "Max: " + snapshot.maxSpeedKmh.ToString("0.0") + " km/h\n"
            + "Distance: " + snapshot.distanceKm.ToString("0.00") + " km\n"
            + "Calories: " + snapshot.caloriesKcal.ToString("0") + " kcal\n"
            + "Gear: " + (snapshot.currentGear > 0 ? snapshot.currentGear.ToString() : "0");
    }

    private string BuildEndRideSummary(RideAnalyticsSnapshot snapshot)
    {
        return "Ride complete\n\n"
            + "Time: " + FormatTime(snapshot.rideTimeSeconds) + "\n"
            + "Distance: " + snapshot.distanceKm.ToString("0.00") + " km\n"
            + "Average speed: " + snapshot.averageSpeedKmh.ToString("0.0") + " km/h\n"
            + "Max speed: " + snapshot.maxSpeedKmh.ToString("0.0") + " km/h\n"
            + "Calories: " + snapshot.caloriesKcal.ToString("0") + " kcal";
    }
}
