using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompleteObjective : MonoBehaviour
{

    public GameObject StartTrigger;
    public GameObject EndTrigger;
    public GameObject ObjectiveLocationUI;
    public GameObject ObjText;


    private PickupObjective pickupObjective;
    private bool MissionStarted;

    void Start()
    {

    }
    //ensure that the current state of MissionStarted bool is collected once collision occurs
    private void Awake()
    {
        //pickupObjective = GameObject.Find("EndObjective").GetComponent<PickupObjective>();//for finding the current state of the bool MissionStarted
        pickupObjective = StartTrigger.GetComponent<PickupObjective>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("touching something");
        //Debug.Log(Trigger.name);
        //Debug.Log(pickupObjective.MissionStarted);
        //check if player had already started the mission
        if (other.CompareTag("Player") && pickupObjective.MissionStarted)
        {
            //complete mission
            Debug.Log("Mission Complete");
            StartCoroutine(ObjectiveComplete());
        }
    }

    private IEnumerator ObjectiveComplete()
    {

        ObjText.GetComponent<Text>().text = "Objective Complete";
        ObjText.SetActive(true);

        //deactivate waypoint
        ObjectiveLocationUI.SetActive(false);

        yield return new WaitForSeconds(1f);

        //ensure that it cannot be activated again
        EndTrigger.SetActive(false);
    }
}
