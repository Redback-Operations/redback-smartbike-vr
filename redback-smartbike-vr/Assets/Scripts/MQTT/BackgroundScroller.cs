using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    // This is not part of MQTT but only used to ensure the
    // moving object does not go out of camera range
    // and to scroll the background in this demo.

    public float maxDistance;
    public float moveDistance;
    public Transform target;

    // Update is called once per frame
    void Update()
    {
        if ((target.position.x - transform.position.x) > maxDistance)
            transform.position += new Vector3(moveDistance, 0.0f, 0.0f);
    }
}
