using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinishLine : MonoBehaviour
{
    void OnTriggerEnter(Collider colInfo)
    {
        if (colInfo.CompareTag("Player"))
        {
            SceneManager.LoadScene("LevelSelection");
        }
    }
}

