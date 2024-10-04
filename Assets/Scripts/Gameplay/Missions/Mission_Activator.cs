using System;
using UnityEngine;

public class Mission_Activator : MonoBehaviour
{
    // used to store the active mission to access elsewhere
    private static Mission _active;
    public static Mission ActiveMission
    {
        get
        {
            //if (_active?.gameObject == null)
                //_active = null;

            return _active;
        }
    }

    public int MissionNumber;
    public Mission[] Missions;

    public void Activate(int id)
    {
        Debug.Log($"Mission {id} is Active");
        MissionNumber = id;

        // loop through each mission object
        foreach (var mission in Missions)
        {
            // disable the missions that are not matching the mission ID
            mission.gameObject.SetActive(mission.MissionNumber == MissionNumber);
            // store the active mission
            if (mission.MissionNumber == MissionNumber)
                _active = mission;
        }
    }

    void Awake()
    {
        // if not missions assigned manually, find all missions in the scene
        if (Missions == null || Missions.Length == 0)
            Missions = FindObjectsOfType<Mission>();

        // mission number retrieved from preferences unless testing
        if (MissionNumber == 0)
            MissionNumber = PlayerPrefs.GetInt("MissionNumber");

        Activate(MissionNumber);
    }
}
