using System.Collections;
using TMPro;
using UnityEngine;

public class Mission6 : Mission
{
    public override int MissionNumber => 6;
    public override string MissionName => "Star Rush";

    public string missionName; // Defines the mission goal to be displayed to the user
    public TextMeshProUGUI missionNameText; // References the UI text that displays the mission goal
    public TextMeshProUGUI timerText; // References the UI text that displays the remaining time
    public TextMeshProUGUI pointsText; // References the UI text that displays the current points
    public TextMeshProUGUI missionStatusText; // References the UI text that displays the mission status (e.g., "Mission Complete!")

    private int points;
    private float remainingTime = 30f;
    private bool missionCompletion;

    void Awake()
    {
        // Declares initial state of the mission
        missionCompletion = false;
        points = 0;

        // find children with collectable and subscribe
        var collectables = GetComponentsInChildren<Collectable>();
        foreach (var collectable in collectables)
        {
            collectable.Register(Collect);
        }

        // Sets UI to default
        UpdateUI();
        StartCoroutine(CountdownTimer());
    }

    private void Collect(Collectable item)
    {
        CollectItem(item.gameObject, item.Value, 3);
    }

    void Update()
    {
        if (remainingTime <= 0f && !missionCompletion)
        {
            // Time's up and mission ends
            missionCompletion = true;
            EndMission();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the tag on the triggered collision is Silver Star, Sliver coin or gold coin.
        if (other.CompareTag("1"))
        {
            CollectItem(other.gameObject, 1, 5f);
        }
        else if (other.CompareTag("2"))
        {
            CollectItem(other.gameObject, 2, 5f);
        }
        else if (other.CompareTag("5"))
        {
            CollectItem(other.gameObject, 3, 10f);
        }
    }

    // Handles what to do with the item once collision is triggered with it
    private void CollectItem(GameObject itemCollectable, int pointsEarned, float timeAdded)
    {
        Debug.Log(itemCollectable.name + " collected");

        // Destroy collected object from the scene
        Destroy(itemCollectable);
        points += pointsEarned;
        remainingTime += timeAdded;

        // Update new points and time
        UpdateUI();
    }

    // Changes the UI for the player based on the defined mission 
    private void UpdateUI()
    {
        if (missionNameText != null)
            missionNameText.text = "Mission: " + missionName; // Tells player the goal
        if (timerText != null)
            timerText.text = "Time: " + Mathf.Ceil(remainingTime) + "s"; // Display remaining time
        if (pointsText != null)
            pointsText.text = "Points: " + points; // Display current points
    }

    private IEnumerator CountdownTimer()
    {
        while (remainingTime > 0 && !missionCompletion)
        {
            yield return new WaitForSeconds(1f);
            remainingTime--;

            Debug.Log("Time Remaining " + remainingTime);

            UpdateUI();
        }
    }

    private void EndMission()
    {
        Debug.Log("Mission Ended! Points " + points + " Remaining Time " + remainingTime);

        // Display final points and mission completion message
        if (missionStatusText != null)
        {
            missionStatusText.text = "Mission Complete! Final Points: " + points;
            StartCoroutine(HideMissionStatusText());
        }

        //hide other UI elements
        if (missionStatusText != null)
            missionNameText.gameObject.SetActive(false);
        if (timerText != null)
            timerText.gameObject.SetActive(false);
        if (pointsText != null)
            pointsText.gameObject.SetActive(false);
    }

    private IEnumerator HideMissionStatusText()
    {
        // Wait for 3 seconds
        yield return new WaitForSeconds(3f);
        // Hide the mission status text
        missionStatusText.gameObject.SetActive(false);
    }



}
