using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportGate : MonoBehaviour
{
    // The target of the scene to teleport to
   public string TargetSceneName;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (NetworkManagement.Instance != null)
                NetworkManagement.Instance.Disconnect();

            // teleport the player to the target scene
            MapLoader.LoadScene(TargetSceneName);
        }

    }
}
