using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadTile : MonoBehaviour
{
    RoadSpawner roadSpawner;

    // Start is called before the first frame update
    private void Start()
    {
        roadSpawner = GameObject.FindObjectOfType<RoadSpawner>();
    }

    private void OnTriggerExit(Collider other)
    {
        roadSpawner.SpawnTile(true);
        Destroy(gameObject, 2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject itemPrefab;

    public void SpawnItem()
    {
        int itemSpawnIndex = Random.Range(2, 5);
        Transform spawnPoint = transform.GetChild(itemSpawnIndex).transform;

        Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity, transform);
    }

    public GameObject coinPrefab;

    public void SpawnCoin()
    {
        int coinToSpawn = 2;
        for(int i = 0; i < coinToSpawn; i++)
        {
            GameObject temp = Instantiate(coinPrefab, transform);
            temp.transform.position = GetRandomPointInCollider(GetComponent<Collider>());
        }
    }

    Vector3 GetRandomPointInCollider (Collider collider)
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

        point.y = 1;
        return point;
    }

}
