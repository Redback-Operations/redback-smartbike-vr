using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickupObjective : MonoBehaviour
{

    public bool DeliverObjActive = false;

    //public variables
    public GameObject Trigger;
    public GameObject ObjectiveLocationUI;
    public GameObject ObjText;
    public GameObject EndTrigger;
    //bool to determine whether the mission has started
    public bool MissionStarted { get; private set; } = false;

    
    

    // Start is called before the first frame update
    void Start()
    {
       // hide text and location
       ObjText.SetActive(false);
       EndTrigger.SetActive(false);
       ObjectiveLocationUI.SetActive(false);
       
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("touching something");
        //if player collides with this item
        if(other.CompareTag("Player"))
        {
            Debug.Log("touching player");
            //begin mission objective
            StartCoroutine(missionObjective());
        }
    }

    private IEnumerator missionObjective()
    {
        Debug.Log("Starting Mission");
        //start the mission objective
        MissionStarted = true;

        ObjText.GetComponent<Text>().text = "New Objective, Deliver item to X";
        ObjText.SetActive(true);
        DeliverObjActive = true;

        //activate waypoint
        ObjectiveLocationUI.SetActive(true);
        EndTrigger.SetActive(true);

        yield return new WaitForSeconds(1f);

        //ensure that this quest cannot be activated again
        Trigger.SetActive(false);



    }
}
