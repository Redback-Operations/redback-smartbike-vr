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

    // Added for Race Mission best-time persistence.
    public List<RaceRecord> raceRecords = new List<RaceRecord>();
}

[System.Serializable]
public class RaceRecord
{
    public string trackKey = string.Empty;
    public float bestTimeSeconds = float.MaxValue;
}
