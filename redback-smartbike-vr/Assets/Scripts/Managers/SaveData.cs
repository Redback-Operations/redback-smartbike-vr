using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

[System.Serializable]
public class SaveData
{
    public string profileName = string.Empty;  // Ensure a default value to prevent null
    public float playerTime = 0f;
    public int highScore = 0;
    public int coins = 0;
}

