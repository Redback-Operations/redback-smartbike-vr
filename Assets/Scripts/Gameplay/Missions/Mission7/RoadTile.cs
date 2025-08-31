using ExitGames.Client.Photon.StructWrapping;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]

public class RoadTile : MonoBehaviour
{
    RoadSpawner roadSpawner;
    public bool bHasBeenVisited;
    public int TileIndex;
    public bool bHasBoosts;

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

    public List<GameObject> itemPrefab = new List<GameObject>();
    public GameObject boostRampPrefab;

    public void SpawnItem()
    {
        if (itemPrefab.Count == 0) return;

        int itemIndex = Random.Range(0, itemPrefab.Count);
        int itemSpawnIndex = Random.Range(2, 5); //this is assuming specific gameobject heirarchy in the prefab and is dangerous design
        Transform spawnPoint = transform.GetChild(itemSpawnIndex).transform;

        Instantiate(itemPrefab[itemIndex], spawnPoint.position, Quaternion.identity, transform);
    }

    public void SpawnBoostRamp()
    {
        Debug.Log("SpawnBoostRamp method called.");
        if (!bHasBoosts)
        {
            Debug.LogWarning("BoostRamp Prefab is not assigned!");
            return;
        }

        int rampToSpawn = 2;
        for (int i = 0; i < rampToSpawn; i++)
        {
            GameObject temp = Instantiate(boostRampPrefab, transform);
            try
            {

                //temp.transform.position = GetRandomPointInCollider(GetComponent<Collider>());

                //evil gameobject hierarchy assumption of tile collider should be changed
                var pivotPoint = transform.GetChild(0);
                BoxCollider collider = pivotPoint.transform.GetComponent<BoxCollider>();
                if (collider = null) throw new System.Exception("Invalid collider!");

                temp.transform.position = GetRandomPointInCollider(pivotPoint.transform.GetComponent<BoxCollider>());
                Debug.Log($"Height: {temp.transform.position}");
                temp.transform.rotation = pivotPoint.transform.rotation;

                Debug.Log($"BoostRamp spawned at: {temp.transform.position}");
            }
            catch (System.Exception e)
            {
                Debug.Log($"Error. Failed to find collision point! {e}");
                Destroy(temp); // remove unnused boosts
            }

        }
    }

    //evil recursive function blows up when no valid collider
    Vector3 GetRandomPointInCollider(BoxCollider collider)
    {
        if (collider == null) throw new System.Exception("Invalid collider!");

        Vector3 point = new Vector3(
            Random.Range(collider.bounds.min.x, collider.bounds.max.x),
            collider.bounds.max.y,
            Random.Range(collider.bounds.min.z, collider.bounds.max.z)
        );

        Debug.Log($"Point: {point}");
        if (point != collider.ClosestPoint(point))
        {
            point = GetRandomPointInCollider(collider); //evil recursion
        }

        //point.y = 0;
        Debug.Log($"Generated point: {point}");
        return point;
    }
}
