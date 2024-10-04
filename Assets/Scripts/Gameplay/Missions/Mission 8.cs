using UnityEngine;
using TMPro;

public class RisingWater : MonoBehaviour
{
    public GameObject waterObject; // Reference to the water object that will rise
    public float riseSpeed = 0.5f; // Speed at which the water rises
    public float targetYHeight = 50f; // The Y height the player needs to reach to escape the flood
    public TextMeshProUGUI missionStatusText; // Reference to the UI text for mission status

    private bool missionComplete = false;

    void Update()
    {
        if (!missionComplete && waterObject != null)
        {
          
            waterObject.transform.position += new Vector3(0, riseSpeed * Time.deltaTime, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the water collides with the player
        if (other.CompareTag("Player"))
        {
            Debug.Log("Mission Failed! The water touched you.");
            missionComplete = true;
            DisplayMissionStatus("Mission Failed");
            EndMission(false);
        }
    }

    void OnTriggerStay(Collider other)
    {
        // Check if the player has reached the target Y height during the game
        if (other.CompareTag("Player") && other.transform.position.y >= targetYHeight && !missionComplete)
        {
            Debug.Log("Mission Complete! You reached the safe height.");
            missionComplete = true;
            DisplayMissionStatus("Mission Complete");
            EndMission(true);
        }
    }

   
    private void EndMission(bool success)
    {
       
        Time.timeScale = 0; 
    }

    private void DisplayMissionStatus(string status)
    {
        // Display mission status 
        if (missionStatusText != null)
        {
            missionStatusText.text = status;
            missionStatusText.gameObject.SetActive(true);
        }
    }
}