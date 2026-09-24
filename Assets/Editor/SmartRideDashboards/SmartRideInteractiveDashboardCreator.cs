#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SmartRideInteractiveDashboardCreator
{
    private const string PrefabFolder = "Assets/Prefabs/UI";
    private const string PrefabPath = PrefabFolder + "/SmartRideInteractiveDashboard.prefab";

    [MenuItem("SmartBike/Dashboards/Add Interactive Dashboards To Current Scene")]
    public static void AddToCurrentScene()
    {
        GameObject root = new GameObject("SmartRideInteractiveDashboard");
        root.AddComponent<RideAnalyticsManager>();
        root.AddComponent<RideAnalyticsApiClient>();
        SmartRideInteractiveDashboard dashboard = root.AddComponent<SmartRideInteractiveDashboard>();
        dashboard.Rebuild();

        Selection.activeGameObject = root;
        Undo.RegisterCreatedObjectUndo(root, "Add Smart Ride Interactive Dashboard");
        Debug.Log("Smart Ride interactive dashboard added to the current scene.");
    }

    [MenuItem("SmartBike/Dashboards/Create Interactive Dashboard Prefab")]
    public static void CreatePrefab()
    {
        if (!Directory.Exists(PrefabFolder))
            Directory.CreateDirectory(PrefabFolder);

        GameObject root = new GameObject("SmartRideInteractiveDashboard");
        root.AddComponent<RideAnalyticsManager>();
        root.AddComponent<RideAnalyticsApiClient>();
        SmartRideInteractiveDashboard dashboard = root.AddComponent<SmartRideInteractiveDashboard>();
        dashboard.Rebuild();

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Debug.Log("Smart Ride interactive dashboard prefab created at " + PrefabPath);
    }

    [MenuItem("SmartBike/Dashboards/Rebuild Selected Interactive Dashboard")]
    public static void RebuildSelected()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("Select a SmartRideInteractiveDashboard object before rebuilding.");
            return;
        }

        SmartRideInteractiveDashboard dashboard = selected.GetComponent<SmartRideInteractiveDashboard>();
        if (dashboard == null)
        {
            Debug.LogWarning("Selected object does not have a SmartRideInteractiveDashboard component.");
            return;
        }

        dashboard.Rebuild();
        EditorUtility.SetDirty(selected);
    }
}
#endif
