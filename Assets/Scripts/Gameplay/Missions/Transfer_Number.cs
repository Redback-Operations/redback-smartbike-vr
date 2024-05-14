using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transfer_Number : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        PlayerPrefs.GetInt("MissionNumber");
    }
}
