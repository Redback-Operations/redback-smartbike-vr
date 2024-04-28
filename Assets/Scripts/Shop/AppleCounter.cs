using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleCounter : MonoBehaviour
{
    private static AppleCounter instance;

    private int applesGenerated = 0;

    public int ApplesGenerated
    {
        get { return applesGenerated; }
        set { applesGenerated = value; }
    }


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }



    public static AppleCounter GetInstance()
    {
        return instance;
    }

    public void IncrementApplesGenerated()
    {
        applesGenerated++;
    }
}