using System.Collections;
using UnityEngine;
using TMPro;

//Mission is for bike to collect star before robot (capsule) crosses road. Use capsule for robot
public class Mission1 : Mission
{
    public override int MissionNumber => 1;
    public override string MissionName => "Collect the Star";

    public GameObject b;
    public GameObject robot;

    public float speed = 1.0f;
    private float delay = 2.0f;

    //For time delay to reset.
    private bool isDelayed = false;
    public bool missionComplete = false; //Tracking whether mission completed, either bike take star or robot reach point b.  
    public TextMeshProUGUI missionStatus; //Ref to TextMeshPro

    IEnumerator StartResetting()
    {
        // Set resetting flag to true to prevent multiple coroutines running simultaneously
        isDelayed = true;
        
        // Wait for the specified delay before resetting the text
        yield return new WaitForSeconds(delay);

        if (missionStatus != null)
            missionStatus.text = null;
        
        // Reset the text
        isDelayed = false;
    }

    //From point A to point B for robot...
    //void OnTriggerEnter(Collider other)
    //{
        //Make a UI, to show fail or success. Looking for textsmashpro(TMPro)

        //if player collects star before robot reaches point b, success
        //if (!missionComplete){
            //if (other.tag == "5")
                //{
                    //missionComplete = true;
                    //missionStatus.text = "Success!";
                    //StartCoroutine(StartResetting());
                //}
        //}

    //}

    // Start is called before the first frame update
    void Start()
    {
        //No UI status yet...
        if (missionStatus != null)
            missionStatus.text = null;

    }

    // Update is called once per frame
    void Update()
    {
        // if robot not assigned don't update
        if (robot == null)
            return;

        GameObject[] starFind = GameObject.FindGameObjectsWithTag("5");//jai

        var step = speed * Time.deltaTime;
        //Alteration of speed for the robot to reach point b, use step
        if (missionComplete)
            return;

        //Move robot to b
        robot.transform.position = Vector3.MoveTowards(robot.transform.position, b.transform.position, step);
        if(robot.transform.position == b.transform.position)
        {
            missionStatus.text = "Failed!";
            missionComplete = true;
            StartCoroutine(StartResetting());
        }
             
        //alterating by jai for mission combining
        if(starFind.Length == 0)
        {
            missionComplete = true;
            missionStatus.text = "Success!";
            StartCoroutine(StartResetting());   
        }
    }
}
