using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarageSpawnManager : MonoBehaviour
{
     // reference to the player prefab
    public GameObject playerPrefab;

    // reference to the spawn point
    public Transform spawnPoint;


    void Start()
    {
        // instantiate the player prefab at the spawn point
        Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
