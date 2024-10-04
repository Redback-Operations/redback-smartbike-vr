using System.Collections;
using TMPro;
using UnityEngine;

public class Mission : MonoBehaviour
{
    public virtual int MissionNumber => 0;
    public virtual string MissionName => "Not Set";
    public bool MissionStarted = false;

    public virtual void StartMission()
    {
        MissionStarted = true;
        Debug.Log($"Mission {MissionNumber} Started");
    }
}
