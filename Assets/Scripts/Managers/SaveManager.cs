using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI;

public class SaveManager : MonoBehaviour
{
    public TMP_InputField profileInputField; // Input to assign profile name
    private string filePath; // Record file location
    private float startTime; // Record time since starting session
    private float playerTime; // Record total playtime
    private SaveData data; // Variable to store the save data for saving and loading
    public string profileName; // Name for currently accessed profile


    // Public method to set the profile from UI input
    public void OnProfileInputSubmit()
    {
        string enteredProfile = profileInputField.text; // Get the profile name from input field
        if (!string.IsNullOrEmpty(enteredProfile))
        {
            SetProfile(enteredProfile);
            startTime = Time.time; // Initialize startTime when profile is set
        }
        else
        {
            Debug.LogWarning("Profile name is empty!");
        }
    }

    // Public method to set the profile
    public void SetProfile(string profile)
    {
        profileName = profile;
        SetProfilePath(profile);
        LoadData();
        data.profileName = profile;
    }

    private void Update()
    {
        // Update the total playtime
        if (data != null)
        {
            playerTime = data.playerTime + (Time.time - startTime);
        }
    }

    // Save total playtime
    // Needs fixing, though doesn't effect functionality
    private void OnApplicationQuit()
    {
        SavePlayerTime(); // This also makes it so it calls SaveDataToFile which should be fixed.
    }

    // Saves highscore
    public void SaveHighScore(int score)
    {
        EnsureDataInitialized();

        data.highScore = score;
        SaveDataToFile();
    }

    public int LoadHighScore()
    {
        return data?.highScore ?? 0; // Return, if null then returns zero
    }

    public void SaveCoinCollection(int coins)
    {
        EnsureDataInitialized();

        data.coins = coins;
        SaveDataToFile();
    }

    public int LoadCoinCollection()
    {
        return data?.coins ?? 0; // Return, if null then returns zero
    }

    // --- Race Mission best-time persistence (new) -----------------------
    // Keyed by an arbitrary "trackKey" string (RaceHUD defaults this to the
    // scene name) so different race tracks/scenes keep separate records
    // within the same profile.

    /// <summary>Saves the time if it beats the existing record (or if there isn't one yet). Returns the best time on file after saving.</summary>
    public float SaveRaceBestTime(string trackKey, float timeSeconds)
    {
        EnsureDataInitialized();

        var record = data.raceRecords.Find(r => r.trackKey == trackKey);
        if (record == null)
        {
            record = new RaceRecord { trackKey = trackKey, bestTimeSeconds = timeSeconds };
            data.raceRecords.Add(record);
            SaveDataToFile();
            return timeSeconds;
        }

        if (timeSeconds < record.bestTimeSeconds)
        {
            record.bestTimeSeconds = timeSeconds;
            SaveDataToFile();
        }

        return record.bestTimeSeconds;
    }

    /// <summary>Returns the best time for a track, or -1 if none is recorded yet.</summary>
    public float LoadRaceBestTime(string trackKey)
    {
        EnsureDataInitialized();

        var record = data.raceRecords.Find(r => r.trackKey == trackKey);
        return record?.bestTimeSeconds ?? -1f;
    }

    // Creates/Accesses JSON file by specified profile name
    private void SetProfilePath(string profile)
    {
        filePath = Application.persistentDataPath + $"/{profile}_PlayerData.json";
        Debug.Log($"File Path for {profile}: {filePath}");
    }

    // Load the entite JSON file into the data variable
    private void LoadData()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<SaveData>(json);
            playerTime = data.playerTime;
        }
        else
        {
            data = new SaveData();
            playerTime = 0f;
        }
    }

    // Overwrites everything from data variable to JSON file
    private void SaveDataToFile()
    {
        EnsureDataInitialized();

         // Ensure filePath is not null or empty
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("File path is not set. Cannot save data.");
            return; // Early exit if filePath is not valid
        }

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, json);
    }

    // Saves the total playtime
    private void SavePlayerTime()
    {
        EnsureDataInitialized();

        data.playerTime = playerTime;
        SaveDataToFile();
    }

    // Ensure data is initialized to prevent null reference issues
    private void EnsureDataInitialized()
    {
        if (data == null)
        {
            Debug.LogWarning("SaveData is null, initializing new SaveData.");
            data = new SaveData();
        }
    }

    // Returns total play time
    public float GetPlayerTime()
    {
        return playerTime;
    }
}
