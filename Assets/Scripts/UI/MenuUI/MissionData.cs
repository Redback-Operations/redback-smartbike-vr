using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionData : MonoBehaviour
{
    public string TargetScene;
    public int MissionID;
    [TextArea(5,25)]
    public String DescriptionText;
}
