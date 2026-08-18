using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Tooling for the mission scene template.
///
/// The template is a *scene*, not a prefab, on purpose: a prefab root can be
/// reverted (Prefab &gt; Revert All) and take an entire built level with it.
/// A scene can only be copied, so a level can never be blown away by an
/// accidental revert. These menu items make the copy safe and repeatable
/// instead of relying on everyone remembering to duplicate the right file.
/// </summary>
public static class MissionSceneTemplateTools
{
    public const string TemplatePath = "Assets/Scenes/Templates/_MissionSceneTemplate.unity";
    private const string ScenesFolder = "Assets/Scenes";

    // ------------------------------------------------------------------ create

    [MenuItem("Tools/Missions/New Mission Scene from Template...", false, 10)]
    public static void NewMissionSceneFromTemplate()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TemplatePath) == null)
        {
            EditorUtility.DisplayDialog("Template missing",
                "Could not find the mission scene template at:\n\n" + TemplatePath +
                "\n\nMake sure you have pulled the latest main branch.", "OK");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        string destination = EditorUtility.SaveFilePanelInProject(
            "New mission scene", "Mission9Scene", "unity",
            "Choose a name and location for the new mission scene.", ScenesFolder);

        if (string.IsNullOrEmpty(destination))
            return;

        if (destination == TemplatePath)
        {
            EditorUtility.DisplayDialog("Nope",
                "That would overwrite the template itself. Pick a different name.", "OK");
            return;
        }

        if (!AssetDatabase.CopyAsset(TemplatePath, destination))
        {
            EditorUtility.DisplayDialog("Copy failed",
                "Unity could not copy the template to:\n\n" + destination, "OK");
            return;
        }

        AssetDatabase.Refresh();

        Scene scene = EditorSceneManager.OpenScene(destination, OpenSceneMode.Single);
        string sceneName = Path.GetFileNameWithoutExtension(destination);

        // NetworkManagement.ActiveScene must match the scene's own name, or
        // SceneManager.SetActiveScene() at startup silently targets nothing and
        // networked objects spawn into the wrong scene.
        var network = Object.FindObjectOfType<NetworkManagement>();
        if (network != null && network.ActiveScene != sceneName)
        {
            Undo.RecordObject(network, "Set ActiveScene");
            network.ActiveScene = sceneName;
            EditorUtility.SetDirty(network);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        if (EditorUtility.DisplayDialog("Add to Build Settings?",
                sceneName + " was created from the template.\n\n" +
                "Add it to the build settings scene list now? " +
                "(Required before MapLoader can load it at runtime.)",
                "Add it", "Not yet"))
        {
            AddSceneToBuildSettings(destination);
        }

        ValidateActiveMissionScene();
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.Any(s => s.path == scenePath))
            return;

        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("[MissionTemplate] Added " + scenePath + " to Build Settings.");
    }

    // ---------------------------------------------------------------- validate

    [MenuItem("Tools/Missions/Validate Active Mission Scene", false, 11)]
    public static void ValidateActiveMissionScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        var problems = new System.Collections.Generic.List<string>();
        var notes = new System.Collections.Generic.List<string>();

        var network = Object.FindObjectOfType<NetworkManagement>();
        if (network == null)
        {
            problems.Add("No NetworkManagement in the scene - the player will never spawn.");
        }
        else
        {
            if (network.ActiveScene != scene.name)
                problems.Add("NetworkManagement.ActiveScene is \"" + network.ActiveScene +
                             "\" but the scene is named \"" + scene.name + "\".");
            if (network.NetworkPlayer == null)
                problems.Add("NetworkManagement.NetworkPlayer is empty (expects Player_New.prefab).");
            if (network.SpawnTarget == null)
                problems.Add("NetworkManagement.SpawnTarget is empty - set it to SpawnPoints/MainSpawn.");
        }

        var activator = Object.FindObjectOfType<Mission_Activator>();
        if (activator == null)
            problems.Add("No Mission_Activator - missions cannot be switched on for this scene.");
        else if (activator.Missions == null || activator.Missions.Length == 0)
            notes.Add("Mission_Activator.Missions is empty; it will fall back to FindObjectsOfType at runtime.");

        var spawns = Object.FindObjectsOfType<MissionSpawn>();
        if (spawns.Length == 0)
            notes.Add("No MissionSpawn points - every mission will start at MainSpawn.");
        var duplicateSpawns = spawns.GroupBy(s => s.Mission).Where(g => g.Count() > 1).ToList();
        foreach (var group in duplicateSpawns)
            problems.Add("More than one MissionSpawn is set to mission " + group.Key + ".");

        if (Object.FindObjectOfType<EventSystem>() == null)
            problems.Add("No EventSystem - UI and XR ray interaction will not receive input.");

        if (Object.FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.XRInteractionManager>().Length != 1)
            problems.Add("There should be exactly one XRInteractionManager in the scene.");

        int missingScripts = CountMissingScripts();
        if (missingScripts > 0)
            problems.Add(missingScripts + " GameObject(s) have missing (None) script components.");

        if (!EditorBuildSettings.scenes.Any(s => s.path == scene.path && s.enabled))
            notes.Add("This scene is not enabled in Build Settings, so MapLoader cannot load it.");

        string report = "[MissionTemplate] Validation of \"" + scene.name + "\":\n";
        report += problems.Count == 0
            ? "  No blocking problems found.\n"
            : string.Join("\n", problems.Select(p => "  PROBLEM: " + p).ToArray()) + "\n";
        if (notes.Count > 0)
            report += string.Join("\n", notes.Select(n => "  note: " + n).ToArray());

        if (problems.Count > 0) Debug.LogWarning(report);
        else Debug.Log(report);
    }

    private static int CountMissingScripts()
    {
        int count = 0;
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                var components = transform.GetComponents<Component>();
                if (components.Any(c => c == null))
                    count++;
            }
        }
        return count;
    }

    // ------------------------------------------------------------------ select

    [MenuItem("Tools/Missions/Select Template Scene Asset", false, 30)]
    public static void SelectTemplate()
    {
        var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(TemplatePath);
        if (asset == null)
        {
            Debug.LogError("[MissionTemplate] Template not found at " + TemplatePath);
            return;
        }
        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);
    }
}
