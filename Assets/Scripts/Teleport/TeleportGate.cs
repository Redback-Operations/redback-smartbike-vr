using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportGate : MonoBehaviour
{
    // The target of the scene to teleport to
    public string TargetSceneName;
    public GameObject teleportEffect;
    private GameObject newEffect;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //if (NetworkManagement.Instance != null)
            //    NetworkManagement.Instance.Disconnect();

            // teleport the player to the target scene
            //MapLoader.LoadScene(TargetSceneName);

            // Instantiate new teleport effect on player, and send target scene to effect
            Debug.Log("Player hit teleport");
            newEffect = Instantiate(teleportEffect, other.transform.position, other.transform.rotation, other.transform);
            newEffect.GetComponent<TeleportEffect>().SetScene(TargetSceneName);
        }

    }
}
