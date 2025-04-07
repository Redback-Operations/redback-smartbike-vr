using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainAudioManager : MonoBehaviour
{
    // Start is called before the first frame update
    private float speed;
    private Vector3 prevPos; //position from previous update
    void Start()
    {
        prevPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        speed = Vector3.Distance(transform.position, prevPos); // get distance between current position and previous position, normalised for frametime
        prevPos = transform.position; // update previous position for next update
        Debug.Log("Bike speed: " + speed);
    }
}
