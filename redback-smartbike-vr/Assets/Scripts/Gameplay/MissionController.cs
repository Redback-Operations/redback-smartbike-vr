using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionController : MonoBehaviour
{
    private GameObject skaterNet;
    // Start is called before the first frame update
    void Start()
    {
        skaterNet = GameObject.FindGameObjectWithTag("skaternet");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger: " + other.name);

        if(other.name == "StartMissionCollider") {
            skaterNet.transform.GetChild(0).gameObject.SetActive(true);
            PlayAudio(other);
        }else if(other.name == "FinishMissionCollider") {
            skaterNet.transform.GetChild(0).gameObject.SetActive(false);
            PlayAudio(other);
        }
    }

    private void PlayAudio(Collider other) {
        other.gameObject.GetComponent<AudioSource>().Play();
    }
}
