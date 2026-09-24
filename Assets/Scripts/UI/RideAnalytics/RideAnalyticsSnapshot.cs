using System;

[Serializable]
public class RideAnalyticsSnapshot
{
    public float currentSpeedKmh;
    public float averageSpeedKmh;
    public float maxSpeedKmh;
    public float cadenceRpm;
    public float distanceKm;
    public float caloriesKcal;
    public float heartRateBpm;
    public float rideTimeSeconds;
    public string currentMission;
    public int checkpointsCompleted;
    public int checkpointsTotal;
    public float personalBestKmh;
    public int currentGear;

    public float DistanceProgress
    {
        get
        {
            if (checkpointsTotal <= 0)
                return 0f;

            return Math.Max(0f, Math.Min(1f, checkpointsCompleted / (float)checkpointsTotal));
        }
    }
}
