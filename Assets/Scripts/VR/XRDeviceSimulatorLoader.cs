using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Spawns the XR Device Simulator when no real headset is rendering,
/// so the project can be tested without VR hardware.
/// </summary>
public class XRDeviceSimulatorLoader : MonoBehaviour
{
    [SerializeField] private GameObject simulatorPrefab;

    [Tooltip("Also spawn the simulator in standalone builds without a headset, not just in the editor.")]
    [SerializeField] private bool allowInBuilds = false;

    private static GameObject _instance;

    private void Awake()
    {
        if (_instance != null || simulatorPrefab == null)
            return;

        if (!Application.isEditor && !allowInBuilds)
            return;

        if (XRSettings.isDeviceActive)
            return;

        _instance = Instantiate(simulatorPrefab);
        _instance.name = simulatorPrefab.name;
        DontDestroyOnLoad(_instance);
        Debug.Log("XRDeviceSimulatorLoader: no headset detected, XR Device Simulator spawned.");
    }
}
