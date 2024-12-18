using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMoves : MonoBehaviour
{
    public Transform[] waypoints; // Array of waypoints forming a path
    public float speed = 5.0f; // Speed of movement
    public float rotationSpeed = 5.0f; // Smooth rotation speed

    private int currentWaypointIndex = 0; // Index of the current waypoint
    private bool isRacing = false; // Control if NPC starts moving

    // Function to start NPC movement
    public void StartRacing()
    {
        isRacing = true;
    }

    void Start()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("No waypoints assigned to NPC.");
            return;
        }
    }

    void Update()
    {
        if (!isRacing) return; // Do nothing until the race starts

        MoveTowardsWaypoint();
    }

    void MoveTowardsWaypoint()
    {
        // Get current waypoint
        Transform targetWaypoint = waypoints[currentWaypointIndex];

        // Move NPC towards the waypoint
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, speed * Time.deltaTime);

        // Calculate direction and apply smooth rotation
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Check if NPC has reached the waypoint
        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            // Move to the next waypoint
            currentWaypointIndex++;

            // Check if we reached the end
            if (currentWaypointIndex >= waypoints.Length)
            {
                Debug.Log("NPC finished the race!");
                isRacing = false; // Stop NPC movement
            }
        }
    }
}
