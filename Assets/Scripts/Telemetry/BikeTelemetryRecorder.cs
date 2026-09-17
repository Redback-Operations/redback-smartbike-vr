using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;

/// <summary>
/// Continuously records live bike telemetry (speed, cadence, heart rate,
/// power) for the current play session, autosaves it to disk so a crash or
/// device reboot loses at most one autosave interval's worth of data, and
/// hands finished sessions to an IPlayerDataUploader - LoggingPlayerDataUploader
/// by default, until the project's real backend endpoint is known (see that
/// class's TODO).
///
/// On startup this also looks for any session files left over from a run
/// that didn't shut down cleanly (crash, force-quit, power loss) and tries to
/// upload those too, so a reboot doesn't silently drop data.
///
/// Place this on a root-level GameObject (e.g. alongside GameManager /
/// SaveManager) the same way Mqtt is - DontDestroyOnLoad only works on root
/// objects, and this needs to survive scene loads to keep recording across
/// a whole play session.
/// </summary>
public class BikeTelemetryRecorder : MonoBehaviour
{
    [Tooltip("How often (seconds) a telemetry sample is taken from the latest MQTT readings.")]
    public float sampleIntervalSeconds = 1f;

    [Tooltip("How often (seconds) the in-progress session is flushed to disk.")]
    public float autosaveIntervalSeconds = 15f;

    private static BikeTelemetryRecorder _instance;
    public static BikeTelemetryRecorder Instance => _instance;

    private IPlayerDataUploader _uploader = new LoggingPlayerDataUploader();

    private BikeTelemetrySession _currentSession;
    private float _sessionElapsedSeconds;
    private float _sampleTimer;
    private float _autosaveTimer;
    private bool _subscribed;

    private float _latestSpeed;
    private float _latestCadence;
    private float _latestHeartRate;
    private float _latestPower;

    private string TelemetryDirectory => Path.Combine(Application.persistentDataPath, "Telemetry");

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        // See Mqtt.cs / the mission template README: DontDestroyOnLoad silently
        // does nothing on a non-root object, so this only works because this
        // component lives on a root GameObject.
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("BikeTelemetryRecorder is not on a root GameObject - it will not survive a scene load. Move it to scene root.");
        }
    }

    /// <summary>Swap in the real uploader once the backend endpoint exists. Defaults to LoggingPlayerDataUploader.</summary>
    public void SetUploader(IPlayerDataUploader uploader)
    {
        _uploader = uploader ?? new LoggingPlayerDataUploader();
    }

    private void Start()
    {
        Directory.CreateDirectory(TelemetryDirectory);
        UploadPendingSessions();
        BeginSession();
    }

    private void BeginSession()
    {
        string profileName = ResolveProfileName();
        _currentSession = BikeTelemetrySession.StartNew(profileName);
        _sessionElapsedSeconds = 0f;
        _sampleTimer = 0f;
        _autosaveTimer = 0f;
    }

    private string ResolveProfileName()
    {
        var saveManager = FindObjectOfType<SaveManager>();
        if (saveManager != null && !string.IsNullOrEmpty(saveManager.profileName))
        {
            return saveManager.profileName;
        }

        return "Default";
    }

    private void Update()
    {
        TrySubscribe();

        if (_currentSession == null)
            return;

        _sessionElapsedSeconds += Time.deltaTime;

        _sampleTimer += Time.deltaTime;
        if (_sampleTimer >= sampleIntervalSeconds)
        {
            _sampleTimer = 0f;
            RecordSample();
        }

        _autosaveTimer += Time.deltaTime;
        if (_autosaveTimer >= autosaveIntervalSeconds)
        {
            _autosaveTimer = 0f;
            SaveSessionToDisk(_currentSession);
        }
    }

    private void TrySubscribe()
    {
        if (_subscribed || Mqtt.Instance == null || !Mqtt.Instance.IsConnected)
            return;

        Mqtt.Instance.Subscribe(OnTelemetryMessage,
            Mqtt.SpeedTopic, Mqtt.CadenceTopic, Mqtt.HeartRateTopic, Mqtt.PowerTopic);

        _subscribed = true;
        Debug.Log("BikeTelemetryRecorder subscribed.");
    }

    private void OnTelemetryMessage(object sender, MqttMsgPublishEventArgs e)
    {
        string topic = e.Topic;
        string msg = System.Text.Encoding.UTF8.GetString(e.Message);

        // The exact payload shape for these single-metric topics isn't
        // confirmed anywhere in this codebase yet (only the combined control
        // topic's format is - see MqttFieldParser). ReadNumericOrField copes
        // with either a bare number or a "{'value': N}" style message.
        if (topic == Mqtt.SpeedTopic)
            _latestSpeed = MqttFieldParser.ReadNumericOrField(msg, "value");
        else if (topic == Mqtt.CadenceTopic)
            _latestCadence = MqttFieldParser.ReadNumericOrField(msg, "value");
        else if (topic == Mqtt.HeartRateTopic)
            _latestHeartRate = MqttFieldParser.ReadNumericOrField(msg, "value");
        else if (topic == Mqtt.PowerTopic)
            _latestPower = MqttFieldParser.ReadNumericOrField(msg, "value");
    }

    private void RecordSample()
    {
        if (!PlayerDataConsent.IsGranted)
            return;

        _currentSession.samples.Add(new BikeTelemetrySample
        {
            sessionTimeSeconds = _sessionElapsedSeconds,
            speed = _latestSpeed,
            cadence = _latestCadence,
            heartRate = _latestHeartRate,
            power = _latestPower
        });
    }

    private string SessionFilePath(BikeTelemetrySession session)
    {
        string safeProfile = string.Join("_", session.profileName.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(TelemetryDirectory, $"{safeProfile}_{session.sessionId}.json");
    }

    private void SaveSessionToDisk(BikeTelemetrySession session)
    {
        try
        {
            string json = JsonUtility.ToJson(session);
            File.WriteAllText(SessionFilePath(session), json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save telemetry session {session.sessionId}: {ex.Message}");
        }
    }

    private void FinalizeAndUploadCurrentSession()
    {
        if (_currentSession == null)
            return;

        _currentSession.endedAtUtc = DateTime.UtcNow.ToString("o");
        SaveSessionToDisk(_currentSession);
        UploadSession(_currentSession, SessionFilePath(_currentSession));
    }

    private void UploadSession(BikeTelemetrySession session, string filePath)
    {
        _uploader.Upload(session, success =>
        {
            if (!success)
            {
                Debug.LogWarning($"Telemetry session {session.sessionId} could not be uploaded; leaving it on disk to retry next launch.");
                return;
            }

            session.uploaded = true;
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Uploaded telemetry session {session.sessionId} but could not delete its local file: {ex.Message}");
            }
        });
    }

    /// <summary>
    /// Finds session files left on disk from a previous run that never got
    /// uploaded (most often because the app didn't shut down cleanly) and
    /// tries them again. This is what makes the "reboot" case in the task
    /// notes actually work: data survives even if OnApplicationQuit never ran.
    /// </summary>
    private void UploadPendingSessions()
    {
        string[] files;
        try
        {
            files = Directory.GetFiles(TelemetryDirectory, "*.json");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Could not scan {TelemetryDirectory} for pending telemetry sessions: {ex.Message}");
            return;
        }

        foreach (string file in files)
        {
            BikeTelemetrySession session;
            try
            {
                session = JsonUtility.FromJson<BikeTelemetrySession>(File.ReadAllText(file));
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Skipping unreadable telemetry file {file}: {ex.Message}");
                continue;
            }

            if (session == null || session.uploaded)
                continue;

            if (string.IsNullOrEmpty(session.endedAtUtc))
            {
                // Never got a clean end (crash/reboot mid-session) - close it
                // out now so it's not left open forever.
                session.endedAtUtc = DateTime.UtcNow.ToString("o");
            }

            UploadSession(session, file);
        }
    }

    private void OnApplicationPause(bool paused)
    {
        // A safety flush, not an end-of-session: VR headsets pause often
        // (headset removed, system overlay, etc.) and treating every pause as
        // the end of the session would fragment data unnecessarily.
        if (paused && _currentSession != null)
        {
            SaveSessionToDisk(_currentSession);
        }
    }

    private void OnApplicationQuit()
    {
        FinalizeAndUploadCurrentSession();
    }
}
