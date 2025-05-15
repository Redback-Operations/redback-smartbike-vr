using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayMove : MonoBehaviour
{
    public float maxSpeed = 10.0f;
    public float maxReverseSpeed = -5.0f;
    public float acceleration = 5.0f;
    public float deceleration = 3.0f;
    public float turnspeed = 100.0f;

    public float currentSpeed = 0.0f;

    public float speedIncreasePerPoint = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Accelerate forward
        if (vertical > 0)
        {
            currentSpeed += acceleration * vertical * Time.deltaTime;
        }
        // Accelerate backward
        else if (vertical < 0)
        {
            currentSpeed += acceleration * vertical * Time.deltaTime;
        }
        // Decelerate when no input
        else
        {
            if (currentSpeed > 0)
            {
                currentSpeed -= deceleration * Time.deltaTime;
                currentSpeed = Mathf.Max(currentSpeed, 0); // Clamp to 0 when decelerating forward
            }
            else if (currentSpeed < 0)
            {
                currentSpeed += deceleration * Time.deltaTime;
                currentSpeed = Mathf.Min(currentSpeed, 0); // Clamp to 0 when decelerating backward
            }
        }

        // Clamp speed between max forward and max reverse speeds
        currentSpeed = Mathf.Clamp(currentSpeed, maxReverseSpeed, maxSpeed);

        transform.position += currentSpeed * Time.deltaTime * transform.forward;
        transform.rotation *= Quaternion.AngleAxis(turnspeed * horizontal * Time.deltaTime, transform.up);
    }
}
