using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ReadyToRideManager : MonoBehaviour
{
    [Header("Summary Text")]
    public TMP_Text bikeText;
    public TMP_Text assistanceText;
    public TMP_Text rideModeText;
    public TMP_Text terrainText;
    public TMP_Text durationText;
    public TMP_Text timeText;
    public TMP_Text weatherText;

    void Start()
    {
        LoadRideSummary();
    }

    void LoadRideSummary()
    {
        string bike = PlayerPrefs.GetString("SelectedBike", "Not Selected");
        string assistance = PlayerPrefs.GetString("AssistanceLevel", "Not Selected");
        string rideMode = PlayerPrefs.GetString("RideMode", "Not Selected");
        string terrain = PlayerPrefs.GetString("Terrain", "Not Selected");
        int duration = PlayerPrefs.GetInt("RideDuration", 45);
        string time = PlayerPrefs.GetString("TimeOfDay", "Not Selected");
        string weather = PlayerPrefs.GetString("Weather", "Not Selected");

        bikeText.text = "Bike: <color=#00BFFF>" + bike + "</color>";

        assistanceText.text = "Assistance: <color=#00BFFF>" + assistance + "</color>";

        rideModeText.text = "Ride Mode: <color=#00BFFF>" + rideMode + "</color>";

        terrainText.text = "Terrain: <color=#00BFFF>" + terrain + "</color>";

        durationText.text = "Duration: <color=#00BFFF>" + duration + " min</color>";

        timeText.text = "Time of Day: <color=#00BFFF>" + time + "</color>";

        weatherText.text = "Weather: <color=#00BFFF>" + weather + "</color>";
    }

    public void Back()
    {
        SceneManager.LoadScene("RideSetting");
    }
    public void StartRide()
    {
        Debug.Log("Starting Ride...");

        SceneManager.LoadScene("CityScene");
    }
}
