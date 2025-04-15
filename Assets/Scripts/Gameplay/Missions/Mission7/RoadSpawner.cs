using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using UnityEngine.Tilemaps;

public class RoadSpawner : MonoBehaviour
{
    public GameObject roadTilePrefab;
    public List<GameObject> allRoadTiles = new List<GameObject>();

    private Vector3 nextSpawnPoint;

    public int initialRoadTileCount = 10;
    public int nMaxActiveTiles = 20;
    public M7GameManager manager;

    public void Reset()
    {
        if (roadTilePrefab == null)
        {
            Debug.LogError("roadTilePrefab is not assigned in the Inspector!");
            return;
        }

        //clear tiles
        if (allRoadTiles.Count > 0)
        {
            foreach (GameObject tile in allRoadTiles)
            {
                Destroy(tile);
            }

            allRoadTiles = new List<GameObject>();
            nextSpawnPoint = transform.position;
        }

        //spawn roadtiles 
        for (int i = 0; i < initialRoadTileCount; i++)
        {
            SpawnTile(false);
        }
    }

    private void Update()
    {
        if (manager == null) return;
        if (manager.playerTransform != null && manager.playerTransform.position.z < allRoadTiles[0].transform.position.z - 10f)
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

        GameObject roadTile = Instantiate(roadTilePrefab, nextSpawnPoint, Quaternion.identity, transform); //updated to spawn road tile as child of spawner
        nextSpawnPoint = roadTile.transform.GetChild(1).transform.position;

        //set the road tile index
        RoadTile tileScript = roadTile.GetComponent<RoadTile>();
        tileScript.TileIndex = allRoadTiles.Count;

        //add tile to list and increase score
        allRoadTiles.Add(roadTile);
        if (manager != null) manager.IncreaseScore();

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

    //spawn a new tile when a player enters a tile for the first time
    public void TileTriggerEnter(RoadTile tile, Collider other)
    {
        if (other.CompareTag("Player") == false) return;

        int ActiveTileIndex = tile.TileIndex;

        if (!tile.bHasBeenVisited)
        {
            tile.bHasBeenVisited = true;
            SpawnTile(true);
        }

        //sliding window of active tiles for performance
        //half the active tiles should be behind player, half in front (floor/ceil one side to account for odd numbers)
        int backThreshold = ActiveTileIndex - Mathf.FloorToInt(nMaxActiveTiles / 2);
        int fwdThreshold = (nMaxActiveTiles % 2 == 0 ? ActiveTileIndex + nMaxActiveTiles / 2 - 1 : ActiveTileIndex + Mathf.FloorToInt(nMaxActiveTiles / 2));

        //adjust for window cutoff at back end
        if (fwdThreshold < nMaxActiveTiles - 1) fwdThreshold = nMaxActiveTiles - 1;
        if (backThreshold < 0) backThreshold = 0;

        ////disable tiles outside of window, enable those within
        foreach (GameObject tileObject in allRoadTiles)
        {
            RoadTile _tile = tileObject.GetComponent<RoadTile>();

            bool inBehindRange = _tile.TileIndex >= backThreshold;
            bool inForwardRange = _tile.TileIndex <= fwdThreshold;
            bool inRange = inBehindRange && inForwardRange;

            //flip the active switch if required
            if (tileObject.activeInHierarchy != inRange) tileObject.SetActive(inRange);
        }
    }

    //depreciated
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
