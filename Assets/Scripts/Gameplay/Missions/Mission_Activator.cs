using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mission_Activator : MonoBehaviour
{
    public GameObject[] Missions;
    public int MissionNumber;
    private int num = 0;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        MissionNumber = PlayerPrefs.GetInt("MissionNumber");
    }
    
    void Update()
    {
        if(num != Missions.Length)
        {
            if(num != MissionNumber)
            {
                Missions[num].SetActive(false);
                num++;
            }
            else
            {
                Missions[MissionNumber].SetActive(true);
                num++;
            }
        }
    }
}
