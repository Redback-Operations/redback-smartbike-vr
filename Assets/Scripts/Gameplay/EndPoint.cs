using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPoint : MonoBehaviour
{
    public GameManager gameManager;

    void onTriggerEnter()
    {
        Debug.Log("Hit something");
        gameManager.CompleteLevel();
    }
}
