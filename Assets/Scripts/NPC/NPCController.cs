using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay.BikeMovement;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    public Transform[] waypoints; // Array of waypoints forming a square path
    public float speed = 5.0f; // Speed at which the sphere moves
    private int currentWaypointIndex = 0; // Index of the current waypoint

    public BikeSelector bikeSelector;

    public GameObject bikeMoverGameObject;

    private IBikeMover _bikeMover;

    // Start is
    // called before the first frame update
    void Start()
    {
        bikeSelector.DisplayBike(0);
        _bikeMover = bikeMoverGameObject.GetComponent<IBikeMover>();
        _bikeMover.Speed = speed;
        _bikeMover.Init(gameObject);
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
        // transform.position = Vector3.MoveTowards(transform.position, n, speed * Time.deltaTime);

        // Check if the object has reached the current waypoint
        if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position) < 0.1f)
        {
            // Move to the next waypoint
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            Vector3 relativePos = waypoints[currentWaypointIndex].position - transform.position;
            transform.rotation = Quaternion.LookRotation(relativePos);
        }
    }

    private void FixedUpdate()
    {
        MoveTo(waypoints[currentWaypointIndex].position);
    }


    void MoveTo(Vector3 targetPos)
    {
        Vector3 moveDir = targetPos - transform.position;

        Vector3 localDir = transform.InverseTransformDirection(moveDir.normalized);

        float steerAngle = Mathf.Clamp(localDir.x, -1f, 1f);
        float throttle = Mathf.Clamp(localDir.z, -1f, 1f);
        _bikeMover.HanldeInput(new Vector2(steerAngle, throttle));
    }
}