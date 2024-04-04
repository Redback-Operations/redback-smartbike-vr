using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreenCallback : MonoBehaviour
{
    bool Started = true;


    // call maploader to load map after 1 frame of this scene
    void Update()
    {
        if (Started)
        {
            Started = false;
            MapLoader.LoadAfter();
        }
    }
}
