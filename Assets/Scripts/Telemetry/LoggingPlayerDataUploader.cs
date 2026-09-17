using System;
using UnityEngine;

/// <summary>
/// Stand-in IPlayerDataUploader used until the real "Virtual Machine" backend
/// this data is meant to go to is confirmed (see the Player Data Persistence
/// task notes - the destination wasn't decided yet when this was written).
/// Logs what would have been sent and reports success, so sessions still get
/// marked uploaded and cleaned up locally instead of piling up on disk forever.
///
/// TODO: once the backend endpoint/schema is known, replace this with an
/// uploader that POSTs the session (JsonUtility.ToJson(session) is already
/// exactly what SaveManager uses elsewhere in this project) via
/// UnityWebRequest to that endpoint, and only calls onComplete(true) after a
/// successful response. Nothing else needs to change - see the note on
/// IPlayerDataUploader.
/// </summary>
public class LoggingPlayerDataUploader : IPlayerDataUploader
{
    public void Upload(BikeTelemetrySession session, Action<bool> onComplete)
    {
        Debug.Log($"[Telemetry] Would upload session {session.sessionId} for profile " +
                  $"'{session.profileName}': {session.samples.Count} samples, " +
                  $"{session.startedAtUtc} -> {session.endedAtUtc}. " +
                  "No backend configured yet, so this is a no-op.");

        onComplete?.Invoke(true);
    }
}
