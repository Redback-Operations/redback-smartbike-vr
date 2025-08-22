using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class MissionData : MonoBehaviour
{
    [ScenePath] public string[] TargetScenes;
    public int MissionID;
    [TextArea(5,25)]
    public String DescriptionText;

}
