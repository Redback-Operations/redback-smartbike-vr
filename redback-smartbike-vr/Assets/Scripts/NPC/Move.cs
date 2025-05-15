using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public Transform[] waypoints; // Array of waypoints forming a square path
    public float speed = 5.0f; // Speed at which the sphere moves
    private int currentWaypointIndex = 0; // Index of the current waypoint
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Check if there are waypoints to follow
        if (waypoints.Length == 0)
        {
            Debug.LogWarning("No waypoints assigned!");
            return;
        }

        // Move towards the current waypoint
        transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex].position, speed * Time.deltaTime);

        // Check if the object has reached the current waypoint
        if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position) < 0.01f)
        {
            // Move to the next waypoint
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            Vector3 relativePos = waypoints[currentWaypointIndex].position - transform.position;
            transform.rotation = Quaternion.LookRotation(relativePos);
        }
    }
}
