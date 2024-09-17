using System;
using UnityEngine;

public class Mission_Activator : MonoBehaviour
{
    /* TODO: Mission parent class
             Need to have a Mission behaviour that is parent of all that has an ID that is used to
             automatically find using FindObjectsOfType<Mission>(), removes need for specific layout
             of game objects and adding manually to the activator script */

    [Serializable]
    // mission settings to assign the ID and mission object
    public class MissionSetting
    {
        public int ID;
        public GameObject Mission;
    }

    public int MissionNumber;
    public MissionSetting[] Missions;

    void Awake()
    {
        // mission number retrieved from preferences unless testing
        if (MissionNumber == 0)
            MissionNumber = PlayerPrefs.GetInt("MissionNumber");

        // loop through each mission object
        foreach (var mission in Missions)
        {
            // disable the missions that are not matching the mission ID
            mission.Mission.SetActive(mission.ID == MissionNumber);
        }
    }
}