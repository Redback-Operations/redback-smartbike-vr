using ExitGames.Client.Photon.StructWrapping;
using Nobi.UiRoundedCorners;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Timeline;

[System.Serializable]

public class RoadTile : MonoBehaviour
{
    RoadSpawner roadSpawner;
    public bool bHasBeenVisited;
    public int TileIndex;
    public bool bHasBoosts;
    public bool spawnsObstacles;
    public GameObject TileSurface;
    public BoxCollider RaycastSpawner;
    public int maxBoostPads;

    // Start is called before the first frame update
    private void Start()
    {
        bHasBeenVisited = false;
        roadSpawner = GameObject.FindObjectOfType<RoadSpawner>();
        if (boostRampPrefab == null && bHasBoosts)
        {
            Debug.LogError("boostRampPrefab is not assigned in RoadTile!");
        }
    }


    //inform the spawner when we've been entered
    private void OnTriggerEnter(Collider other)
    {
        if (roadSpawner != null) roadSpawner.TileTriggerEnter(this, other);
    }

    public List<GameObject> obstaclePrefabs = new List<GameObject>();
    public List<GameObject> obstacleSpawnPoints = new List<GameObject>();
    public GameObject boostRampPrefab;

    public void SpawnItem()
    {
        if (!spawnsObstacles) return;
        if (obstaclePrefabs.Count == 0 || obstaclePrefabs == null) return;
        if (obstacleSpawnPoints.Count == 0 || obstacleSpawnPoints == null) return;

        //updated to select obstacle spawn point from list of spawns
        int itemIndex = UnityEngine.Random.Range(0, obstaclePrefabs.Count);
        int itemSpawnIndex = UnityEngine.Random.Range(0, obstacleSpawnPoints.Count);
        Transform spawnPoint = obstacleSpawnPoints[itemSpawnIndex].transform;

        Instantiate(obstaclePrefabs[itemIndex], spawnPoint.position, Quaternion.identity, transform);
    }

    public void SpawnBoostRamp()
    {
        Debug.Log("SpawnBoostRamp method called.");
        if (!bHasBoosts)
        {
            Debug.LogWarning("BoostRamp Prefab is not assigned!");
            return;
        }

        //removed hardcoded 2 for pad variable (default to 2)
        for (int i = 0; i < maxBoostPads; i++)
        {
            GameObject temp = Instantiate(boostRampPrefab, transform);
            try
            {
                Collider collider = TileSurface.transform.GetComponent<Collider>();
                if (collider = null) throw new System.Exception("Invalid collider!");

                //find spawn point on the tile for the instantiated boostpad
                Vector3 spawnPos;
                Quaternion spawnRot;

                if (SpawnBoost(RaycastSpawner, TileSurface, temp.transform.GetComponent<BoxCollider>(), out spawnPos, out spawnRot)) {
                    temp.transform.SetPositionAndRotation(spawnPos, spawnRot);
                }
                else
                {
                    Destroy(temp); // remove unnused boosts
                }

                    Debug.Log($"BoostRamp spawned at: {temp.transform.position}");
            }
            catch (System.Exception e)
            {
                Debug.Log($"Error. Failed to find collision point! {e}");
                Destroy(temp); // remove unnused boosts
            }

        }
    }

    //evil recursive function blows up when no valid collider - DEPRECIATED
    Vector3 GetRandomPointInCollider(Collider tileCollider, Collider toSpawnCollider)
    {
        if (tileCollider == null) throw new System.Exception("Invalid tile collider!");
        if (toSpawnCollider == null) throw new System.Exception("Invalid boost collider!");

        //if we have a collider to spawn with, offset its spawnpoint by dimensions, otherwise no offset
        Vector3 toSpawnSize = toSpawnCollider.bounds.size;
        Debug.Log($"Boost size: {(toSpawnSize.x, toSpawnSize.y, toSpawnSize.z)}");

        //chose random point between bounds of tile + half size of new object
        Vector3 point = new Vector3(
            UnityEngine.Random.Range(tileCollider.bounds.min.x + toSpawnSize.x, tileCollider.bounds.max.x - toSpawnSize.x),
            tileCollider.bounds.max.y,
            UnityEngine.Random.Range(tileCollider.bounds.min.z + toSpawnSize.z, tileCollider.bounds.max.z - toSpawnSize.z)
        );

        Debug.Log($"Point: {point}");
        if (point != tileCollider.ClosestPoint(point))
        {
            point = GetRandomPointInCollider(tileCollider, toSpawnCollider); //evil recursion
        }

        //point.y = 0;
        Debug.Log($"Generated point: {point}");
        return point;
    }


    //POLISH THIS
    public bool SpawnBoost(BoxCollider raycastPlane, GameObject tileCollider, BoxCollider toSpawnCollider, out Vector3 spawnPos, out Quaternion spawnRot)
    {
        if (raycastPlane == null || toSpawnCollider == null || tileCollider == null) throw new System.Exception("Missing collider!");
        int maxAttempts = 10;

        //make several attempts to find a valid spawn location
        for (int i = 0; i < maxAttempts; i++)
        {
            //find a point on the plane of the raycast spawner
            float localX = UnityEngine.Random.Range(-0.5f, 0.5f) * raycastPlane.size.x;
            float localZ = UnityEngine.Random.Range(-0.5f, 0.5f) * raycastPlane.size.z;
            float bufferDistance = 0.05f;

            Vector3 localPoint = new Vector3(localX, 0f, localZ) + raycastPlane.center;
            Vector3 worldPoint = raycastPlane.transform.TransformPoint(localPoint);
            Debug.Log($"Local Point: {localPoint}, World Point: {worldPoint}");

            //make the scan and try to find a valid point
            RaycastHit centreHit;
            if (Physics.Raycast(worldPoint, Vector3.down, out centreHit, 10) && centreHit.transform.IsChildOf(tileCollider.gameObject.transform))
            {
                var marker = new GameObject("CentreMarker");
                marker.transform.SetPositionAndRotation(centreHit.point, Quaternion.FromToRotation(Vector3.up, centreHit.normal));
                marker.transform.SetParent(transform, true);

                float toSpawnWidth = toSpawnCollider.size.x / 2 + bufferDistance;
                float toSpawnLength = toSpawnCollider.size.z / 2 + bufferDistance;

                //finding corners along axis of the potential spawn
                var normalRot = Quaternion.FromToRotation(Vector3.up, centreHit.normal);
                var normalRight = normalRot * Vector3.right;
                var normalFwd = normalRot * Vector3.forward;
                var temp = centreHit.point; temp.y += toSpawnCollider.size.y + bufferDistance;

                Vector3[] corners = new Vector3[4];
                corners[0] = temp; corners[0] = temp + (-normalRight * toSpawnWidth) + (-normalFwd * toSpawnLength);
                corners[1] = temp; corners[1] = temp + (-normalRight * toSpawnWidth) + (normalFwd * toSpawnLength);
                corners[2] = temp; corners[2] = temp + (normalRight * toSpawnWidth) + (-normalFwd * toSpawnLength);
                corners[3] = temp; corners[3] = temp + (normalRight * toSpawnWidth) + (normalFwd * toSpawnLength);

                //spawn a ray for each of the projected corners to see if we're in a valid spot
                bool validCorner = false;
                foreach (Vector3 corner in corners)
                {
                    bool impact = false;
                    bool validImpact = false;
                    RaycastHit cornerHit;

                    Debug.Log($"Projected Point: {corner}");


                    //if we go a little below the threshold and find a spot that is part of the road tile, then this is probably a valid point
                    impact = Physics.Raycast(corner, -centreHit.normal, out cornerHit, toSpawnCollider.size.y + bufferDistance * 2);
                    if (!impact) { validCorner = false; break; }

                    Debug.Log($"Spawning GameObject for : {corner}");
                    Quaternion cornerRot = Quaternion.FromToRotation(Vector3.up, cornerHit.normal);

                    //debug purposes if needed
                    marker = new GameObject("CornerMarker");
                    marker.transform.SetPositionAndRotation(cornerHit.point, cornerRot);
                    marker.transform.SetParent(transform, true);

                    //make sure we haven't impact an obstacle or impacted outside our buffer range
                    validImpact = (cornerHit.transform.IsChildOf(centreHit.transform));
                    if (!validImpact) { validCorner = false; break; }

                    validCorner = true;
                }

                if (!validCorner) break;

                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, centreHit.normal);
                //Instantiate(boostRampPrefab, centreHit.point, rotation, transform);

                Debug.Log($"BoostRamp spawned at: {centreHit.point}");
                spawnPos = centreHit.point;
                spawnRot = rotation;
                return true;
            }
        }
        spawnPos = Vector3.zero; spawnRot = Quaternion.identity;
        return false;
    }

}
