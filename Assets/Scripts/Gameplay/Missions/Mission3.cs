using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Mission3 : Mission
{
    public override int MissionNumber => 3;
    public override string MissionName => "Speed Trap";

    //Need to refer to speed boost ramp for info - maybe change up max speed for ramps via Unity? - Done.
    //Split the game into at least five zones ending with a finish line/win state - Maybe.
    //Or continuous and fail later to tally score and restart? 

    //Check SpeedBoostArea and PlayerController for tips?

    //Mission can be tested so far in the race track out at the city with speedboost ramp to run and see how it works

    private float delay = 1.0f;
    //For time delay to reset.
    private bool isDelayed = false;
    //Referencing the bike to get their speed.
    public PlayerController bike;
    public float speed;
    public float objective;
    public bool missionComplete = false; //Tracking finish line /win state at the end (?)
    public TextMeshProUGUI missionObjective; //Show objective speed for current situation
    public TextMeshProUGUI missionStatus; //Show player current speed

    IEnumerator StartResetting()
    {
        // Set resetting flag to true to prevent multiple coroutines running simultaneously
        isDelayed = true;

        // Wait for the specified delay before resetting the text
        yield return new WaitForSeconds(delay);

        if (missionObjective != null)
            missionObjective.text = null;
        if (missionStatus != null)
            missionStatus.text = null;

        // Generate a new objective after delay
        GenerateNewObjective();

        // Reset the text
        isDelayed = false;
    }

    void GenerateNewObjective()
    {
        var rand = new System.Random();
        objective = rand.Next(5, 30);
        //Show objective speed for UI - should show for current section so how to split down zones and add
        //Less than 5 makes it reverse so but does that mean it slows down player?
        if (missionObjective != null)
            missionObjective.text = "Objective speed: " + objective.ToString();

    }
    void Start()
    {
        GenerateNewObjective();
    }

    void Update()
    {
        //Show current speed of the bike
        speed = bike.movementSpeed;

        if (missionStatus != null)
            missionStatus.text = "Speed: " + bike.movementSpeed;

        Debug.Log("Objective: " + objective + " Speed: " + bike.movementSpeed);

        if (bike.movementSpeed >= objective)
        {
            if (missionStatus != null)
                missionStatus.text = "Speed accomplished";
            //Will show Objective speed for a few seconds in varying numbers before stopping and restart.
            StartCoroutine(StartResetting());
        }

        //How to make a new scene / finish line? Tried at terrain but bike spawning there was a pain

        //For spawning purposes using an if statement for mission completion 
        //- to make infinity road but few problems
        // if (bike.movementSpeed == objective){
        //     //To make continuous road (pending after too much trial and error)
        //     GameObject road = GameObject.FindGameObjectWithTag("road");
        //     if (bike.transform.position.x == road.transform.position.x){
        //         //Road runs away too fast. Find a way to make it move once etc? 
        //         Debug.Log("Road position: " + road.transform.position.x);
        //         road.transform.position = new Vector3(road.transform.position.x - 9, road.transform.position.y, road.transform.position.z);
        //         Debug.Log("After Road position: " + road.transform.position.x);
        //     }
        // }
    }
}
