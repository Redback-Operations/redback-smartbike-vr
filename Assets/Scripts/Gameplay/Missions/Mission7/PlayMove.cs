using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayMove : MonoBehaviour
{
    public float speed = 3.0f;
    public float turnspeed = 20.0f;

    public float speedIncreasePerPoint = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        transform.position += speed * vertical * Time.deltaTime * transform.forward;
        transform.rotation *= Quaternion.AngleAxis(turnspeed * horizontal * Time.deltaTime, transform.up);
    }
}
