#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class RideAnalyticsHudPrefabCreator
{
    private const string PrefabFolder = "Assets/Prefabs/UI";
    private const string PrefabPath = PrefabFolder + "/RideAnalyticsHUD.prefab";

    [MenuItem("SmartBike/Ride Analytics/Create HUD Prefab")]
    public static void CreatePrefab()
    {
        if (!Directory.Exists(PrefabFolder))
            Directory.CreateDirectory(PrefabFolder);

        GameObject root = new GameObject("RideAnalyticsHUD");
        root.AddComponent<RideAnalyticsManager>();
        root.AddComponent<RideAnalyticsHudBuilder>();
        RideAnalyticsHudBuilder.Build(root, root.GetComponent<RideAnalyticsManager>());

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Debug.Log("Ride Analytics HUD prefab created at " + PrefabPath);
    }

    [MenuItem("SmartBike/Ride Analytics/Add HUD To Current Scene")]
    public static void AddToCurrentScene()
    {
        GameObject root = new GameObject("RideAnalyticsHUD");
        root.AddComponent<RideAnalyticsManager>();
        root.AddComponent<RideAnalyticsHudBuilder>();
        RideAnalyticsHudBuilder.Build(root, root.GetComponent<RideAnalyticsManager>());

        Selection.activeGameObject = root;
        Undo.RegisterCreatedObjectUndo(root, "Add Ride Analytics HUD");
    }

    [MenuItem("SmartBike/Ride Analytics/Rebuild Selected HUD")]
    public static void RebuildSelectedHud()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("Select a RideAnalyticsHUD object before rebuilding.");
            return;
        }

        RideAnalyticsHudBuilder builder = selected.GetComponent<RideAnalyticsHudBuilder>();
        if (builder == null)
        {
            Debug.LogWarning("Selected object does not have a RideAnalyticsHudBuilder component.");
            return;
        }

        builder.Rebuild();
        EditorUtility.SetDirty(selected);
    }
}
#endif
