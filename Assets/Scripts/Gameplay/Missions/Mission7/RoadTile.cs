using ExitGames.Client.Photon.StructWrapping;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Rendering;

public class RoadTile : MonoBehaviour
{
    RoadSpawner roadSpawner;
    public bool bHasBeenVisited;
    public int TileIndex;

    // Start is called before the first frame update
    private void Start()
    {
        bHasBeenVisited = false;
        roadSpawner = GameObject.FindObjectOfType<RoadSpawner>();
        if (boostRampPrefab == null)
        {
            Debug.LogError("boostRampPrefab is not assigned in RoadTile!");
        }
    }


    //inform the spawner when we've been entered
    private void OnTriggerEnter(Collider other)
    {
        if (roadSpawner != null) roadSpawner.TileTriggerEnter(this, other);
    }

    public GameObject itemPrefab;
    public GameObject boostRampPrefab;

    public void SpawnItem()
    {
        int itemSpawnIndex = Random.Range(2, 5);
        Transform spawnPoint = transform.GetChild(itemSpawnIndex).transform;

        Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity, transform);
    }

    public void SpawnBoostRamp()
    {
        Debug.Log("SpawnBoostRamp method called.");
        if (boostRampPrefab == null)
        {
            Debug.LogWarning("BoostRamp Prefab is not assigned!");
            return;
        }

        int rampToSpawn = 2;
        for (int i = 0; i < rampToSpawn; i++)
        {
            GameObject temp = Instantiate(boostRampPrefab, transform);
            temp.transform.position = GetRandomPointInCollider(GetComponent<Collider>());
            Debug.Log($"BoostRamp spawned at: {temp.transform.position}");
        }
    }

    Vector3 GetRandomPointInCollider(Collider collider)
    {
        Vector3 point = new Vector3(
            Random.Range(collider.bounds.min.x, collider.bounds.max.x),
            Random.Range(collider.bounds.min.y, collider.bounds.max.y),
            Random.Range(collider.bounds.min.z, collider.bounds.max.z)
        );

        if (point != collider.ClosestPoint(point))
        {
            point = GetRandomPointInCollider(collider);
        }

        point.y = 0;
        Debug.Log($"Generated point: {point}");
        return point;
    }
}
