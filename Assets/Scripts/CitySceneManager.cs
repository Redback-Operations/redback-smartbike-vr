using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CitySceneManager : MonoBehaviour
{
    public void StopRide()
    {
        Debug.Log("Ride Stopped");

        SceneManager.LoadScene("ReadyToRide");
    }
}
