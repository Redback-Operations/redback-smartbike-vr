using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance {get; private set;}

    // Waypoint 0 should be starting line and Waypoint Len-1 should be finish line.
    [SerializeField] private Waypoint[] waypointsSorted;
    
    [SerializeField, Tooltip("The prefab used to visually draw the path in the world.")] private GameObject pathDisplayPrefab;
    [SerializeField] private Vector3 pathDisplayObjectOffset = new Vector3(0, 1, 0);

    [SerializeField, Range(0.25f, 1.0f)] private float displayDensity = 1.0f;

    private int targetWaypointId;

    private List<GameObject> pathDisplayObjects = new List<GameObject>();

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        // Set to 0 and call next waypoint to increase to 1 and draw the first path.
        targetWaypointId = 0;
        NextWaypoint();
    }

    /// <summary>
    /// Checks if the given transform is one of the waypoints.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool IsWaypoint(Transform point){
        foreach (var waypoint in waypointsSorted){
            if (waypoint.transform == point) return true;
        }

        return false;
    }

    private Waypoint GetWaypointFromTransform(Transform t){
        foreach (var waypoint in waypointsSorted){
            if (waypoint.transform == t) return waypoint;
        }

        return null;
    }

    public void CheckWaypoint(Transform waypointHit){
        var waypoint = GetWaypointFromTransform(waypointHit);

        if (waypoint == null){
            Debug.LogError($"{waypointHit} checked as a waypoint but is not one!");
            return;
        }

        var targetWaypoint = waypointsSorted[targetWaypointId];

        if (targetWaypoint != waypoint){
            Debug.Log($"Waypoint {waypoint.name} was hit but is not the target one. Target is: {targetWaypoint.name}");
            return;
        }

        NextWaypoint();
    }

    private void NextWaypoint(){
        if (targetWaypointId == waypointsSorted.Length - 1) {
            Debug.Log("Reached Finish waypoint, do what you want here!");
            return;
        }

        targetWaypointId++;
        UpdateWaypointPathDisplayed();
    }

    private void UpdateWaypointPathDisplayed(){
        for (var i = 0; i < pathDisplayObjects.Count; i++){
            Destroy(pathDisplayObjects[i]);
        }

        pathDisplayObjects.Clear();

        var previousWaypoint = waypointsSorted[targetWaypointId-1];
        var targetWaypoint = waypointsSorted[targetWaypointId];

        var tStep = Time.deltaTime + (1.0f - displayDensity);

        for (var t = 0.0f; t <= 1.0f; t += tStep){
            var pos = previousWaypoint.transform.position.BezierLerp(
                                    targetWaypoint.transform.position, 
                                    previousWaypoint.bezierControlPointOffsetA, 
                                    previousWaypoint.bezierControlPointOffsetB, 
                                    t);

            pos += pathDisplayObjectOffset;
            var displayObj = Instantiate(pathDisplayPrefab, pos, Quaternion.identity, null);
            pathDisplayObjects.Add(displayObj);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected() {
        if (waypointsSorted.Length <= 1) return;
        if (targetWaypointId < 1 || targetWaypointId >= waypointsSorted.Length) return;

        // Draw the waypoints
        for (var i = 0; i < waypointsSorted.Length; i++){
            Gizmos.color = targetWaypointId == i ? Color.green : Color.blue;
            Gizmos.DrawSphere(waypointsSorted[i].transform.position, 1.0f);
        }

        // Draw spheres from waypoint to waypoint and highlight previous to targetWaypoint
        for (var i = 1; i < waypointsSorted.Length; i++){
            var previousWaypointId = i - 1;
            var previousWaypoint = waypointsSorted[previousWaypointId];
            var targetWaypoint = waypointsSorted[i];

            const float DELTA_TIME = 1/90.0f; // fixed 90 fps delta time due to inspector usage.

            // Draw the bezier control points
            Gizmos.color = i == targetWaypointId ? Color.cyan : Color.gray;

            Gizmos.DrawSphere(previousWaypoint.transform.position + previousWaypoint.bezierControlPointOffsetA, 0.25f);
            Gizmos.DrawSphere(targetWaypoint.transform.position + previousWaypoint.bezierControlPointOffsetB, 0.25f);

            Gizmos.color = i == targetWaypointId ? Color.yellow : Color.white;

            for (var t = 0.0f; t <= 1.0f; t += DELTA_TIME){
                var bezierPos = previousWaypoint.transform.position.BezierLerp(
                                    targetWaypoint.transform.position, 
                                    previousWaypoint.bezierControlPointOffsetA, 
                                    previousWaypoint.bezierControlPointOffsetB, 
                                    t);

                Gizmos.DrawSphere(bezierPos, 0.15f);
            }
        }
        
    }
    #endif

    [System.Serializable]
    public class Waypoint{
        public string name;
        public Transform transform;
        public Vector3 bezierControlPointOffsetA;
        public Vector3 bezierControlPointOffsetB;
    }
}