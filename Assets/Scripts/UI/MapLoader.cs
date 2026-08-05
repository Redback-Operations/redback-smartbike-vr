using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MapLoader
{
    public static int targetSceneIndex;

    public enum Scene // Add the scene name here when adding more scenes on the Build Settings
    {
        MainMenu,
        MainScene,
        SceneSelect,
        CityScene,
        BikeSelectScene,
        SampleScene,
        LoadingScene,
        LevelSelection
    }

    private static string _scene;

    public static void LoadScene(Scene scene)
    {
        LoadScene(scene.ToString());
    }

    public static void LoadScene(string scene)
    {
        Debug.Log($"[MapLoader] LoadScene requested: '{scene}' (caller triggered a full LoadingScene reload)");
        _scene = scene;
        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }

    public static void LoadAfter()
    {
        if (string.IsNullOrEmpty(_scene))
        {
            Debug.LogWarning("[MapLoader] LoadAfter() called with no target scene set - falling back to 'AvatarSelection'. This means LoadScene(...) was never called with a valid name before LoadingScene ran.");
            _scene = "AvatarSelection";
        }
        Debug.Log($"[MapLoader] LoadAfter loading '{_scene}' additively");
        SceneManager.LoadSceneAsync(_scene, LoadSceneMode.Additive);
    }

    public static bool HasScene(Scene scene)
    {
        return HasScene(scene.ToString());
    }

    public static bool HasScene(string scene)
    {
        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var loadedScene = SceneManager.GetSceneAt(i);

            if (loadedScene.name == scene)
                return true;
        }

        return false;
    }

    public static void UnloadScene(Scene scene)
    {
        UnloadScene(scene.ToString());
    }

    public static void UnloadScene(string scene)
    {
        if (HasScene(scene))
            SceneManager.UnloadSceneAsync(scene);
    }
}
