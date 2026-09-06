using System;
using System.Collections.Generic;

/// <summary>
/// One recorded play session's worth of bike telemetry: who was riding, when
/// it started/ended, and every sample taken in between. Serialized to disk
/// as-is (see BikeTelemetryRecorder) so a session survives an app crash or
/// device reboot before it has been uploaded.
/// </summary>
[Serializable]
public class BikeTelemetrySession
{
    public string sessionId;
    public string profileName;
    public string startedAtUtc;
    public string endedAtUtc;
    public bool uploaded;
    public List<BikeTelemetrySample> samples = new List<BikeTelemetrySample>();

    public static BikeTelemetrySession StartNew(string profileName)
    {
        return new BikeTelemetrySession
        {
            sessionId = Guid.NewGuid().ToString("N"),
            profileName = profileName,
            startedAtUtc = DateTime.UtcNow.ToString("o"),
            uploaded = false
        };
    }
}
