using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UI.FloodEscape;


public class RisingWater : Mission
{
    public override int MissionNumber => 8;
    public override string MissionName => "Rising Water";

    public GameObject waterObject; // Reference to the water object that will rise
    public float riseSpeed = 0.5f; // Speed at which the water rises
    public float targetYHeight = 50f; // The Y height the player needs to reach to escape the flood

    public int objective;

    [SerializeField] private ElevationIndicatorUI elevationIndicatorUIPrefab;
    [SerializeField] private GameObject[] objectsToDisable;
    [SerializeField] private GameObject[] objectsToEnable;
    private float height;
    private bool missionComplete = false;

    public struct RisingWaterEvent : IEvent
    {
        public float CurrentWaterHeight;
        public float TargetHeight;
    }

    public override void StartMission()
    {
        objective = UIManager.Instance.AddObjective("Escape the Rising Water");
        DisplayMissionStatus("Flee to the hills!");

        foreach (var go in objectsToDisable)
        {
            go.SetActive(false);
        }

        foreach (var go in objectsToEnable)
        {
            go.SetActive(true);
        }
        
        UIManager.Instance.InsertAddon(elevationIndicatorUIPrefab.gameObject);
        base.StartMission();
    }


    IEnumerator StartResetting()
    {
        yield return new WaitForSeconds(5);
        MapLoader.LoadScene("GarageScene");
    }

    void Update()
    {
        if (!MissionStarted)
            return;

        if (!missionComplete && waterObject != null)
        {
            if (height < targetYHeight)
                waterObject.transform.position += new Vector3(0, riseSpeed * Time.deltaTime, 0);
            else
                EndMission(true);

            height = waterObject.transform.position.y;
            EventBus<RisingWaterEvent>.Raise(
                new RisingWaterEvent { CurrentWaterHeight = height, TargetHeight = targetYHeight });
            transform.position = waterObject.transform.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        
        // Check if the water collides with the player
        if (other.CompareTag("Player"))
            EndMission(false);
    }
    private void EndMission(bool success)
    {
        missionComplete = true;

        Debug.Log(success
            ? "Mission Complete! You reached the safe height."
            : "Mission Failed! The water touched you.");

        DisplayMissionStatus(success ? "Mission Complete" : "Mission Failed");
        UIManager.Instance.SetObjectiveState(objective,
            success ? UIManager.ObjectiveState.Success : UIManager.ObjectiveState.Failed);

        if (!success)
            StartCoroutine(StartResetting());
    }

    private void DisplayMissionStatus(string status)
    {
        // Display mission status 
        StartCoroutine(UIManager.Instance.ShowNotification(status, 5));
    }
}