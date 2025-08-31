using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using UnityEngine.Tilemaps;

public class RoadSpawner : MonoBehaviour
{
    public bool TileCleanup;
    public List<GameObject> roadTilePrefab = new List<GameObject>();
    public List<GameObject> allRoadTiles = new List<GameObject>();

    private Vector3 nextSpawnPoint;

    public int initialRoadTileCount = 10;
    public int maxTiles = 20;
    public int nMaxActiveTiles = 20;
    public bool bSpawnNonFlatOnStart;
    private M7GameManager _manager;

    public void Reset()
    {
        if (roadTilePrefab.Count == 0)
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

        //spawn initial roadtiles (no objects on first batch, then spawn with objects if allowed)
        if (initialRoadTileCount > maxTiles) initialRoadTileCount = maxTiles;
        for (int i = 0; i < maxTiles; i++)
        {
            if (i < initialRoadTileCount) SpawnTile(false, false);
            else if (bSpawnNonFlatOnStart) SpawnTile(true, true);
        }
    }

    private void Start()
    {
        _manager = M7GameManager.inst;
        //    if (_manager == null) return;
    }

    ////handled by M7GameManager now
    //private void Update()
    //{
    //    _manager = M7GameManager.inst;
    //    if (_manager == null) return;

    //    if (allRoadTiles.Count == 0) return;

    //if (_manager.playerTransform != null && _manager.playerTransform.transform.position.z < allRoadTiles[0].transform.position.z - 10f)
    //{
    //    ResetRoadTiles();
    //}
    //}

    public void SpawnTile(bool spawnItems, bool randomTiles)
    {
        Debug.Log("SpawnTile method called.");

        if (roadTilePrefab == null || roadTilePrefab.Count == 0)
        {
            Debug.LogError("roadTilePrefab is not assigned!");
            return;
        }

        //chose a tile but make sure it does not drop below min height
        //go through available tiles, and add them to possible list
        RoadTileContainer selectedTile = roadTilePrefab[0].GetComponent<RoadTileContainer>();
        if (randomTiles)
        {
            List<GameObject> weightedTileSelection = new List<GameObject>();
            float totalWeight = 0f; //treat 1.0 as standard weight
            foreach (var tile in roadTilePrefab)
            {
                RoadTileContainer t = tile.GetComponent<RoadTileContainer>();
                if (nextSpawnPoint.y < t.MinHeight) continue;
                if (tile == null) continue;

                weightedTileSelection.Add(tile);
                totalWeight += Mathf.Max(0f, t.Weight); //don't let negatives mess with weights
            }

            //valid options selected, now chose - aim for 50% flat, remaining 2/3 inclines, remaining 1/3 novel
            float r = Random.value * totalWeight;
            foreach (var tile in weightedTileSelection)
            {
                //subtract percentage-based weights until we get a value
                RoadTileContainer t = tile.GetComponent<RoadTileContainer>();
                r -= Mathf.Max(0f, t.Weight);
                if (r <= 0f)
                {
                    selectedTile = t;
                    break;
                }
            }
        }


        GameObject roadTile = Instantiate(selectedTile.gameObject, nextSpawnPoint, Quaternion.identity, transform); //updated to spawn road tile as child of spawner
        nextSpawnPoint = roadTile.transform.GetChild(1).transform.position; //this is dangerous - assumes specific heirarchy order of NextSpawnPoint object

        //set the road tile index
        RoadTile tileScript = roadTile.GetComponent<RoadTile>();
        tileScript.TileIndex = allRoadTiles.Count;

        //add tile to list
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

    //spawn a new tile when a player enters a tile for the first time
    public void TileTriggerEnter(RoadTile tile, Collider other)
    {
        if (other.CompareTag("Player") == false) return;

        //half the active tiles should be behind player, half in front (floor/ceil one side to account for odd numbers)
        int ActiveTileIndex = tile.TileIndex;
        int backThreshold = ActiveTileIndex - Mathf.FloorToInt(nMaxActiveTiles / 2);
        int fwdThreshold = (nMaxActiveTiles % 2 == 0 ? ActiveTileIndex + nMaxActiveTiles / 2 - 1 : ActiveTileIndex + Mathf.FloorToInt(nMaxActiveTiles / 2));

        if (!tile.bHasBeenVisited)
        {
            //increase score
            if (_manager != null) _manager.IncreaseScore();

            tile.bHasBeenVisited = true;
            if (fwdThreshold >= allRoadTiles.Count) SpawnTile(true, true);
        }

        //sliding window of active tiles for performance
        if (!TileCleanup) return;

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
