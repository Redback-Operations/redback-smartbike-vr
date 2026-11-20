using UnityEngine;

public static class DebugLog
{
    public static bool Enabled
    {
        get => Debug.unityLogger.filterLogType == LogType.Log;
        set => Debug.unityLogger.filterLogType = value ? LogType.Log : LogType.Warning;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init() => Enabled = Debug.isDebugBuild;
}
