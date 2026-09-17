using System;

/// <summary>
/// One point-in-time reading of the bike's live telemetry, taken at a fixed
/// sampling interval while a session is being recorded.
/// </summary>
[Serializable]
public class BikeTelemetrySample
{
    public float sessionTimeSeconds;
    public float speed;
    public float cadence;
    public float heartRate;
    public float power;
}
