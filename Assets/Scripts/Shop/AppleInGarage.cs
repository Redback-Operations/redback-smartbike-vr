using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleInGarage : MonoBehaviour
{
    public GameObject applePrefab;
    public Transform spawnPoint;
    void Start()
    {
        AppleCounter appleCounter = FindObjectOfType<AppleCounter>();
        if (appleCounter != null)
        {
            int applesToGenerate = appleCounter.ApplesGenerated;
            for (int i = 0; i < applesToGenerate; i++)
            {
                Instantiate(applePrefab, spawnPoint.position, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogWarning("AppleCounter not found!");
        }
    }

}
