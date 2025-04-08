using System.Collections;
using Gameplay.Missions;
using TMPro;
using UnityEngine;

public class Mission : MonoBehaviour
{
    [SerializeField] protected MissionData missionData;
    public virtual int MissionNumber => 0;
    public virtual string MissionName => "Not Set";
    public bool MissionStarted = false;

    public virtual void StartMission()
    {
        MissionStarted = true;
        Debug.Log($"Mission {MissionNumber} Started");
    }
}
