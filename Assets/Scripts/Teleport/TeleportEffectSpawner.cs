using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportEffectSpawner : MonoBehaviour
{
    private string targetScene; // Target scene to teleport to
    public GameObject teleportEffect; // Teleport effect prefab
    private GameObject newEffect;
    

    // Start is called before the first frame update
    void Start()
    {
        // Instantiate teleport effect prefab and set target scene
        newEffect = Instantiate(teleportEffect, transform.position, transform.rotation, transform);
        newEffect.GetComponent<TeleportEffect>().SetScene(targetScene);
    }

    // Set target scene with passed string
    public void SetScene(string scene)
    {
        targetScene = scene;
    }
}
