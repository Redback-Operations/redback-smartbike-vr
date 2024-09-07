using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadedCallback : MonoBehaviour
{
    void Start()
    {
        MapLoader.UnloadScene("LoadingScene");
    }
}
