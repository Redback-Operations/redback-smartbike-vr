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
    private static Scene mapName;

    public static void Load(Scene sceneName)
    {
        //if loading desert scene
        if (sceneName == Scene.MainScene)
        {
            MapLoader.mapName = sceneName;
            SceneManager.LoadScene(Scene.LoadingScene.ToString());//load loadingScene
        } else
        {
            SceneManager.LoadScene(sceneName.ToString());
        }
    }
    public static void LoadAfter()//load map after 1 frame 
    {
        SceneManager.LoadScene(mapName.ToString());
    }

}
