using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hover : MonoBehaviour
{
    // Movable, levitating object.

    // This works by measuring the distance to ground with a
    // raycast a force is applied if the object is below the
	// minimumHeight, or below the hoverHeight from the ground

    // Desired height.
    public float minimumHeight = 182.0f;
    public float hoverHeight = 1.0f;

    // The force applied per unit of distance below the desired height.
    public float hoverForce = 20000.0f;

    // Damping reduces bouncing
    public float hoverDamp = 0.5f;

    // Rigidbody component.
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        //Fairly high drag makes the object easier to control.
        rb.drag = 0.5f;
        rb.angularDrag = 2.0f;
    }

    void FixedUpdate()
    {

        //Make a downwards raycast
        RaycastHit hit;
        Vector3 down = -Vector3.up * 1000;
        Ray downRay = new Ray(transform.position, down);
        Debug.DrawRay(transform.position, down, Color.red);

        float upwardSpeed = rb.velocity.y;

        // Apply a lifting force if the object is too low
        if(transform.position.y < minimumHeight){
            //Debug.Log("Below minimum");
            float heightError = minimumHeight - transform.position.y;
            UpwardForce(heightError * hoverForce - upwardSpeed * hoverDamp);
            //Debug.Log("heightError: " + heightError);
        }

        if (Physics.Raycast(downRay, out hit)){
            float hoverError = hoverHeight - hit.distance;
            //Debug.Log(hit.distance);
            //Debug.Log("hoverError: " + hoverError);
            if (hoverError > 0){
                UpwardForce(hoverError * hoverForce - upwardSpeed * hoverDamp);
            }
        }else{
            UpwardForce(hoverForce - upwardSpeed * hoverDamp);
            //Debug.Log("No raycast hit!");
        }
    }

    private void UpwardForce(float force){
            float lift = force;
            rb.AddForce(lift * Vector3.up);
    }
}