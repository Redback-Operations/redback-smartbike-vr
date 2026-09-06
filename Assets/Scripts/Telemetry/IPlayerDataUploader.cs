using System;

/// <summary>
/// Sends a recorded telemetry session somewhere off-device. The real
/// implementation (an HTTP call to the project's backend/VM) doesn't exist
/// yet - see LoggingPlayerDataUploader for the stand-in BikeTelemetryRecorder
/// uses until that endpoint is confirmed. Swap out the uploader instance and
/// nothing else about the recording/persistence side has to change.
/// </summary>
public interface IPlayerDataUploader
{
    /// <summary>
    /// Attempt to upload the session. Call onComplete(true) if it's safe to
    /// mark the session uploaded and delete it locally; call onComplete(false)
    /// to leave it on disk so it gets retried on the next launch.
    /// </summary>
    void Upload(BikeTelemetrySession session, Action<bool> onComplete);
}
