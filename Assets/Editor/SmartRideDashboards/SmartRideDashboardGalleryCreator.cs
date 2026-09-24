#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SmartRideDashboardGalleryCreator
{
    private const string AssetFolder = "Assets/Art/FigmaDashboards";
    private const string PrefabFolder = "Assets/Prefabs/UI";
    private const string PrefabPath = PrefabFolder + "/SmartRideDashboardGallery.prefab";

    private static readonly string[] DashboardImagePaths =
    {
        AssetFolder + "/Smart Ride (select Bike).png",
        AssetFolder + "/Smart Ride (Ride Setting).png",
        AssetFolder + "/Smart Ride (Ready to Ride).png",
        AssetFolder + "/Overview Dashboard.png",
        AssetFolder + "/Rides Analytics Dashboard.png",
        AssetFolder + "/Map Dashboard.png",
        AssetFolder + "/User Statistics Dashboard.png",
        AssetFolder + "/Trip Details Dashboard.png",
        AssetFolder + "/Overview Dashboard Alt.png"
    };

    [MenuItem("SmartBike/Dashboards/Add Figma Dashboards To Current Scene")]
    public static void AddToCurrentScene()
    {
        ConfigureDashboardSprites();

        GameObject root = new GameObject("SmartRideDashboardGallery");
        SmartRideDashboardGallery gallery = root.AddComponent<SmartRideDashboardGallery>();
        gallery.SetScreens(LoadDashboardSprites());
        gallery.Rebuild();

        Selection.activeGameObject = root;
        Undo.RegisterCreatedObjectUndo(root, "Add Smart Ride Dashboard Gallery");
        Debug.Log("Smart Ride dashboard gallery added to the current scene.");
    }

    [MenuItem("SmartBike/Dashboards/Create Figma Dashboard Prefab")]
    public static void CreatePrefab()
    {
        ConfigureDashboardSprites();

        if (!Directory.Exists(PrefabFolder))
            Directory.CreateDirectory(PrefabFolder);

        GameObject root = new GameObject("SmartRideDashboardGallery");
        SmartRideDashboardGallery gallery = root.AddComponent<SmartRideDashboardGallery>();
        gallery.SetScreens(LoadDashboardSprites());
        gallery.Rebuild();

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Debug.Log("Smart Ride dashboard gallery prefab created at " + PrefabPath);
    }

    [MenuItem("SmartBike/Dashboards/Rebuild Selected Figma Dashboard")]
    public static void RebuildSelected()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("Select a SmartRideDashboardGallery object before rebuilding.");
            return;
        }

        SmartRideDashboardGallery gallery = selected.GetComponent<SmartRideDashboardGallery>();
        if (gallery == null)
        {
            Debug.LogWarning("Selected object does not have a SmartRideDashboardGallery component.");
            return;
        }

        ConfigureDashboardSprites();
        gallery.SetScreens(LoadDashboardSprites());
        gallery.Rebuild();
        EditorUtility.SetDirty(selected);
    }

    [MenuItem("SmartBike/Dashboards/Configure Imported Dashboard Sprites")]
    public static void ConfigureDashboardSprites()
    {
        foreach (string path in DashboardImagePaths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning("Dashboard image not found at " + path);
                continue;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = false;
            importer.SaveAndReimport();
        }

        AssetDatabase.Refresh();
    }

    private static Sprite[] LoadDashboardSprites()
    {
        Sprite[] sprites = new Sprite[DashboardImagePaths.Length];
        for (int i = 0; i < DashboardImagePaths.Length; i++)
            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(DashboardImagePaths[i]);
        return sprites;
    }
}
#endif
