using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadSpawner : MonoBehaviour
{
    public GameObject roadTilePrefab;
    private List<GameObject> allRoadTiles = new List<GameObject>();
    private Vector3 nextSpawnPoint;

    public int initialRoadTileCount = 10;
    public Transform playerTransform;

    private void Start()
    {
        if (roadTilePrefab == null)
        {
            Debug.LogError("roadTilePrefab is not assigned in the Inspector!");
            return;
        }

        for (int i = 0; i < initialRoadTileCount; i++)
        {
            SpawnTile(false);
        }
    }

    private void Update()
    {
        if (playerTransform != null && playerTransform.position.z < allRoadTiles[0].transform.position.z - 10f)
        {
            ResetRoadTiles();
        }
    }

    public void SpawnTile(bool spawnItems)
    {
        Debug.Log("SpawnTile method called.");

        if (roadTilePrefab == null)
        {
            Debug.LogError("roadTilePrefab is not assigned!");
            return;
        }

        GameObject roadTile = Instantiate(roadTilePrefab, nextSpawnPoint, Quaternion.identity);
        nextSpawnPoint = roadTile.transform.GetChild(1).transform.position;

        allRoadTiles.Add(roadTile);

        if (spawnItems)
        {
            var roadTileScript = roadTile.GetComponent<RoadTile>();
            if (roadTileScript != null)
            {
                roadTileScript.SpawnItem();
                roadTileScript.SpawnBoostRamp();
            }
        }
    }

    private void ResetRoadTiles()
    {
        Debug.Log("Player returned to start, resetting road tiles.");

        foreach (GameObject tile in allRoadTiles)
        {
            if (tile != null)
            {
                tile.SetActive(true);
                Debug.Log($"Road tile at {tile.transform.position} is now active.");
            }
            else
            {
                Debug.LogError("A road tile has been destroyed!");
            }
        }

        if (allRoadTiles.Count > 0)
        {
            nextSpawnPoint = allRoadTiles[0].transform.GetChild(1).position;
            Debug.Log($"Reset nextSpawnPoint to: {nextSpawnPoint}");
        }
    }
}
