using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class RideSettingsManager : MonoBehaviour
{
    [Header("Duration")]
    public TMP_Text durationText;

    private int rideDuration = 45;

    [Header("Assistance Buttons")]
    public Button ecoButton;
    public Button sportButton;
    public Button normalButton;

    [Header("Ride Mode Buttons")]
    public Button fitnessButton;
    public Button leisureButton;
    public Button commuteButton;

    [Header("Terrain Buttons")]
    public Button flatButton;
    public Button mixedButton;
    public Button mountainButton;

    [Header("Time of Day Buttons")]
    public Button dayButton;
    public Button nightButton;
    public Button eveningButton;

    [Header("Weather Buttons")]
    public Button clearButton;
    public Button rainyButton;
    public Button windyButton;

    private Color normalColor = Color.white;
    private Color selectedColor = new Color(0.2f, 0.5f, 1f);

    void Start()
    {
        // Restore saved ride duration
        rideDuration = PlayerPrefs.GetInt("RideDuration", 45);
        UpdateDurationText();

        // Restore Assistance Level
        if (PlayerPrefs.HasKey("AssistanceLevel"))
        {
            SelectAssistance(PlayerPrefs.GetString("AssistanceLevel"));
        }

        // Restore Ride Mode
        if (PlayerPrefs.HasKey("RideMode"))
        {
            SelectRideMode(PlayerPrefs.GetString("RideMode"));
        }

        // Restore Terrain
        if (PlayerPrefs.HasKey("Terrain"))
        {
            SelectTerrain(PlayerPrefs.GetString("Terrain"));
        }

        // Restore Time of Day
        if (PlayerPrefs.HasKey("TimeOfDay"))
        {
            SelectTimeOfDay(PlayerPrefs.GetString("TimeOfDay"));
        }

        // Restore Weather
        if (PlayerPrefs.HasKey("Weather"))
        {
            SelectWeather(PlayerPrefs.GetString("Weather"));
        }
    }

    public void IncreaseDuration()
    {
        if (rideDuration < 120)
        {
            rideDuration += 5;

            PlayerPrefs.SetInt("RideDuration", rideDuration);
            PlayerPrefs.Save();

            UpdateDurationText();
        }
    }

    public void DecreaseDuration()
    {
        if (rideDuration > 5)
        {
            rideDuration -= 5;

            PlayerPrefs.SetInt("RideDuration", rideDuration);
            PlayerPrefs.Save();

            UpdateDurationText();
        }
    }

    void UpdateDurationText()
    {
        durationText.text = rideDuration + " min";
    }

    // ASSISTANCE LEVEL
    public void SelectAssistance(string option)
    {
        PlayerPrefs.SetString("AssistanceLevel", option);
        Debug.Log("Assistance Level: " + option);

        ecoButton.image.color = normalColor;
        sportButton.image.color = normalColor;
        normalButton.image.color = normalColor;

        if (option == "Eco")
            ecoButton.image.color = selectedColor;
        else if (option == "Sport")
            sportButton.image.color = selectedColor;
        else if (option == "Normal")
            normalButton.image.color = selectedColor;
    }

    // RIDE MODE
    public void SelectRideMode(string option)
    {
        PlayerPrefs.SetString("RideMode", option);
        Debug.Log("Ride Mode: " + option);

        fitnessButton.image.color = normalColor;
        leisureButton.image.color = normalColor;
        commuteButton.image.color = normalColor;

        if (option == "Fitness")
            fitnessButton.image.color = selectedColor;
        else if (option == "Leisure")
            leisureButton.image.color = selectedColor;
        else if (option == "Commute")
            commuteButton.image.color = selectedColor;
    }

    // TERRAIN
    public void SelectTerrain(string option)
    {
        PlayerPrefs.SetString("Terrain", option);
        Debug.Log("Terrain: " + option);

        flatButton.image.color = normalColor;
        mixedButton.image.color = normalColor;
        mountainButton.image.color = normalColor;

        if (option == "Flat")
            flatButton.image.color = selectedColor;
        else if (option == "Mixed")
            mixedButton.image.color = selectedColor;
        else if (option == "Mountain")
            mountainButton.image.color = selectedColor;
    }

    // TIME OF DAY
    public void SelectTimeOfDay(string option)
    {
        PlayerPrefs.SetString("TimeOfDay", option);
        Debug.Log("Time of Day: " + option);

        dayButton.image.color = normalColor;
        nightButton.image.color = normalColor;
        eveningButton.image.color = normalColor;

        if (option == "Day")
            dayButton.image.color = selectedColor;
        else if (option == "Night")
            nightButton.image.color = selectedColor;
        else if (option == "Evening")
            eveningButton.image.color = selectedColor;
    }

    // WEATHER
    public void SelectWeather(string option)
    {
        PlayerPrefs.SetString("Weather", option);
        Debug.Log("Weather: " + option);

        clearButton.image.color = normalColor;
        rainyButton.image.color = normalColor;
        windyButton.image.color = normalColor;

        if (option == "Clear")
            clearButton.image.color = selectedColor;
        else if (option == "Rainy")
            rainyButton.image.color = selectedColor;
        else if (option == "Windy")
            windyButton.image.color = selectedColor;
    }

    // BACK BUTTON
    public void Back()
    {
        SceneManager.LoadScene("BikeSelection");
    }

    // READY TO RIDE BUTTON
    public void ReadyToRide()
    {
        PlayerPrefs.SetInt("RideDuration", rideDuration);
        PlayerPrefs.Save();

        Debug.Log("Ready to Ride clicked");

        SceneManager.LoadScene("ReadyToRide");
    }
    // MENU BUTTON
    public void OpenMenu()
    {
        SceneManager.LoadScene("MenuPage");
    }
}