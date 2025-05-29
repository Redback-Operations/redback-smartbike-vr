using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceBikeMove : MonoBehaviour
{
    private bool racing = false;
    public Transform[] waypoints; // Array of waypoints forming a path
    public float maxSpeed = 8.0f; // Maximum speed in straight lines
    public float minSpeed = 3.0f; // Minimum speed when turning
    public float acceleration = 2.0f; // Speed change smoothing
    public float rotationSpeed = 5.0f;

    private float currentSpeed = 0f;
    private int currentWaypointIndex = 0; // Index of the current waypoint
    private float t = 0; // Parameter for interpolation
    private Vector3 lastPosition;

    public void StartRacing()
    {
        racing = true;
        currentSpeed = minSpeed;
        lastPosition = transform.position;
    }

    void Start()
    {
        if (waypoints.Length < 4)
        {
            Debug.LogError("Catmull-Rom splines require at least 4 waypoints");
        }

        lastPosition = transform.position;
    }

    void Update()
    {
        if (!racing || waypoints.Length < 4) return;

        // Calculate the next position on the spline
        Vector3 newPos = CatmullRomSpline(t);

        // Calculate direction vectors
        Vector3 currentForward = (newPos - transform.position).normalized;
        Vector3 lastForward = (transform.position - lastPosition).normalized;

        // Calculate angle difference to determine turning
        float angle = Vector3.Angle(currentForward, lastForward);
        bool isTurning = angle > 15f; // You can tweak this angle

        // Decide target speed
        float targetSpeed = isTurning ? minSpeed : maxSpeed;

        // Smooth transition to target speed
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * acceleration);

        // Move towards new position
        transform.position = Vector3.MoveTowards(transform.position, newPos, currentSpeed * Time.deltaTime);

        // Rotate towards movement direction
        if (currentForward != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(currentForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Update interpolation parameter t
        float segmentDistance = Vector3.Distance(waypoints[currentWaypointIndex].position, waypoints[(currentWaypointIndex + 1) % waypoints.Length].position);
        t += currentSpeed * Time.deltaTime / segmentDistance;

        if (t >= 1f)
        {
            t = 0f;
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }

        lastPosition = transform.position;
    }

    private Vector3 CatmullRomSpline(float t)
    {
        int p0 = Mathf.Clamp(currentWaypointIndex - 1, 0, waypoints.Length - 1);
        int p1 = Mathf.Clamp(currentWaypointIndex, 0, waypoints.Length - 1);
        int p2 = Mathf.Clamp(currentWaypointIndex + 1, 0, waypoints.Length - 1);
        int p3 = Mathf.Clamp(currentWaypointIndex + 2, 0, waypoints.Length - 1);

        Vector3 P0 = waypoints[p0].position;
        Vector3 P1 = waypoints[p1].position;
        Vector3 P2 = waypoints[p2].position;
        Vector3 P3 = waypoints[p3].position;

        return 0.5f * (
            (2 * P1) +
            (-P0 + P2) * t +
            (2 * P0 - 5 * P1 + 4 * P2 - P3) * t * t +
            (-P0 + 3 * P1 - 3 * P2 + P3) * t * t * t
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (CheckpointManager.Instance == null)
            return;

        if (other.CompareTag("Checkpoint"))
        {
            CheckpointManager.Instance.CheckpointReached(other.gameObject, this.gameObject);
        }
    }
}
