using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    private string filePath; // Record file location
    private float startTime; // Record time since starting session
    private float playerTime; // Record total playtime
    private SaveData data; // Variable to store the save data for saving and loading
    public string profileName; // Name for currently accessed profile

    private void Awake()
    {
        startTime = Time.time;
        SetProfile(profileName); // This probably doesn't need to be called in the awake function
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
    private void OnApplicationQuit()
    {
        SavePlayerTime();
    }

    // Saves highscore
    public void SaveHighScore(int score)
    {
        data.highScore = score;
        SaveDataToFile();
    }

    public int LoadHighScore()
    {
        return data?.highScore ?? 0; // Return, if null then returns zero
    }

    public void SaveCoinCollection(int coins)
    {
        data.coins = coins;
        SaveDataToFile();
    }

    public int LoadCoinCollection()
    {
        return data?.coins ?? 0; // Return, if null then returns zero
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
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, json);
    }

    // Saves the total playtime
    private void SavePlayerTime()
    {
        if (data == null)
        {
            data = new SaveData();
        }
        data.playerTime = playerTime;
        SaveDataToFile();
    }

    // Returns total play time
    public float GetPlayerTime()
    {
        return playerTime;
    }
}
