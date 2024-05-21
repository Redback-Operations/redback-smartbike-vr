using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceBikeMove : MonoBehaviour
{
    private bool racing = false;
    public CheckpointManager checkpointManager;
    public Transform[] waypoints; // Array of waypoints forming a path
    public float speed = 5.0f; // Speed at which the bike moves
    public float rotationSpeed = 5.0f;
    private int currentWaypointIndex = 0; // Index of the current waypoint
    private float t = 0; // Parameter for interpolation

    public void StartRacing()
    {
        racing = true;
    }

    void Start()
    {
        if (waypoints.Length < 4)
        {
            Debug.LogError("Catmull-Rom splines require at least 4 waypoints");
        }
    }

    void Update()
    {
        if (racing)
        {
            // NPC bike racing logic

            if (waypoints.Length < 4)
            {
                return;
            }

            // Calculate the next position on the spline
            Vector3 newPos = CatmullRomSpline(t);

            // Move towards the new position
            transform.position = Vector3.MoveTowards(transform.position, newPos, speed * Time.deltaTime);

            // Calculate the direction and rotate towards the new position
            Vector3 direction = (newPos - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                transform.rotation = Quaternion.LookRotation(direction);
            }

            // Update the interpolation parameter
            t += speed * Time.deltaTime / Vector3.Distance(waypoints[currentWaypointIndex].position, waypoints[(currentWaypointIndex + 1) % waypoints.Length].position);

            // Move to the next waypoint if the current segment is complete
            if (t >= 1)
            {
                t = 0;
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
        }

        Vector3 CatmullRomSpline(float t)
        {
            // Clamp the current waypoint index to valid ranges
            int p0 = Mathf.Clamp(currentWaypointIndex - 1, 0, waypoints.Length - 1);
            int p1 = Mathf.Clamp(currentWaypointIndex, 0, waypoints.Length - 1);
            int p2 = Mathf.Clamp(currentWaypointIndex + 1, 0, waypoints.Length - 1);
            int p3 = Mathf.Clamp(currentWaypointIndex + 2, 0, waypoints.Length - 1);

            // Get the positions of the four waypoints
            Vector3 P0 = waypoints[p0].position;
            Vector3 P1 = waypoints[p1].position;
            Vector3 P2 = waypoints[p2].position;
            Vector3 P3 = waypoints[p3].position;

            // Calculate the Catmull-Rom spline
            return 0.5f * ((2 * P1) + (-P0 + P2) * t + (2 * P0 - 5 * P1 + 4 * P2 - P3) * t * t + (-P0 + 3 * P1 - 3 * P2 + P3) * t * t * t);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            checkpointManager.CheckpointReached(other.gameObject, this.gameObject);
        }
    }
}
