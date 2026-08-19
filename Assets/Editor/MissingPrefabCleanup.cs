using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Finds and removes GameObjects that are instances of a prefab asset which no
/// longer exists in the project.
///
/// Unity keeps these around as "Missing Prefab" placeholders. They carry no
/// components and render nothing, but every time the scene is opened they log
/// "Prefab instance problem. Missing Prefab Asset: '...'", which buries real
/// errors in the console.
///
/// Report first, remove second - the report tells you what you are about to
/// lose, in case a placeholder is standing in for something that should be
/// restored from git rather than deleted.
/// </summary>
public static class MissingPrefabCleanup
{
    [MenuItem("Tools/Cleanup/Report Missing Prefab Instances in Open Scenes", false, 10)]
    public static void Report()
    {
        var found = Find();

        if (found.Count == 0)
        {
            Debug.Log("[MissingPrefab] No missing prefab instances in the open scenes.");
            return;
        }

        var lines = found.Select(go => "  " + PathOf(go) + "   (scene: " + go.scene.name + ")");
        Debug.LogWarning($"[MissingPrefab] {found.Count} missing prefab instance(s):\n" +
                         string.Join("\n", lines.ToArray()) +
                         "\n\nRun Tools > Cleanup > Remove Missing Prefab Instances to delete them.");
    }

    [MenuItem("Tools/Cleanup/Remove Missing Prefab Instances in Open Scenes", false, 11)]
    public static void Remove()
    {
        var found = Find();

        if (found.Count == 0)
        {
            Debug.Log("[MissingPrefab] Nothing to remove.");
            return;
        }

        string list = string.Join("\n", found.Select(go => "  " + PathOf(go)).Take(20).ToArray());
        if (found.Count > 20) list += $"\n  ...and {found.Count - 20} more";

        if (!EditorUtility.DisplayDialog("Remove missing prefab instances?",
                $"About to delete {found.Count} GameObject(s) whose prefab asset no longer exists:\n\n{list}\n\n" +
                "This cannot be undone once the scene is saved.",
                "Delete them", "Cancel"))
            return;

        var scenes = new HashSet<Scene>();
        foreach (var go in found)
        {
            scenes.Add(go.scene);
            Debug.Log("[MissingPrefab] Removed " + PathOf(go));
            Undo.DestroyObjectImmediate(go);
        }

        foreach (var scene in scenes)
            EditorSceneManager.MarkSceneDirty(scene);

        Debug.Log($"[MissingPrefab] Removed {found.Count} instance(s) from {scenes.Count} scene(s). " +
                  "Save the scene(s) to keep the change.");
    }

    private static List<GameObject> Find()
    {
        var found = new List<GameObject>();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            foreach (var root in scene.GetRootGameObjects())
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                    if (PrefabUtility.IsPrefabAssetMissing(transform.gameObject))
                        found.Add(transform.gameObject);
        }

        // Deepest first, so removing a parent never invalidates a queued child.
        found.Sort((a, b) => Depth(b).CompareTo(Depth(a)));
        return found;
    }

    private static int Depth(GameObject go)
    {
        int d = 0;
        for (Transform t = go.transform; t.parent != null; t = t.parent) d++;
        return d;
    }

    private static string PathOf(GameObject go)
    {
        string path = go.name;
        for (Transform t = go.transform.parent; t != null; t = t.parent)
            path = t.name + "/" + path;
        return path;
    }
}
