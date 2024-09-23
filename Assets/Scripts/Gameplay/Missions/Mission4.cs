using System.Collections;
using TMPro;
using UnityEngine;

public class Mission4 : Mission
{
    public override int MissionNumber => 4;
    public override string MissionName => "Collection Mission";

    //public GameObject collectableType; // Declaration of what item needs collecting for the mission
    public TextMeshProUGUI missionNameText; // References the UI text that displays the mission goal
    public TextMeshProUGUI collectableCountText; // References the UI text that updates the collectable count
    public int requiredCollectableCount; 
    public bool missionCompletion;

    private int collectableCount;

    void Awake()
    {
        // Declares initial state of the mission
        missionCompletion = false;
        collectableCount = 0;

        // Sets UI to default
        UpdateUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the tag on the triggered collision is the same as the either of the coin tags
        if (other.CompareTag("1") || other.CompareTag("2"))
        {
            CollectItem(other.gameObject);
        }
    }

    // Handles what to do with the item once collision is triggered with it
    private void CollectItem(GameObject itemCollectable)
    {
        Debug.Log(itemCollectable.name + " collected");

        // Destroy collected object from the scene and add to counter
        Destroy(itemCollectable);
        collectableCount++;
        
        // Checks to see if mission complete condition is met
        if(collectableCount >= requiredCollectableCount)
        {
            Debug.Log("Mission Complete");
            missionCompletion = true;
            StartCoroutine(CompleteMission());
        }

        // Update new collection count
        UpdateUI();
    }


    // Changes the UI for the player based on the defined mission 
    private void UpdateUI() 
    {
        if (missionNameText != null)
            missionNameText.text = "Mission: " + MissionName; // Tells player the goal

        if (collectableCountText != null)
        {
            // Checks to see if goal has been acheived
            if (!missionCompletion)
                collectableCountText.text = " Collected: " + collectableCount + " / " + requiredCollectableCount; // Displays mission progress
            else
                // If mission goal is completed
                collectableCountText.text = " Collected: " + requiredCollectableCount + " / " + requiredCollectableCount + " Completed!";
        }
    }

    private IEnumerator CompleteMission()
    {
        // Wait for 3 seconds
        yield return new WaitForSeconds(3f);

        // Hide UI elements
        if (!missionNameText)
            missionNameText.gameObject.SetActive(false);

        if (!collectableCountText)
            collectableCountText.gameObject.SetActive(false);
    }
}


