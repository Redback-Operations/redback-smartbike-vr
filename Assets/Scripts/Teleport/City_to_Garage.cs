using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class City_to_Garage : MonoBehaviour
{
    // The target of the scene to teleport to
   public string TargetSceneName;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // teleport the player to the target scene
            SceneManager.LoadScene(TargetSceneName);
        }

    }
}
