using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPoint : MonoBehaviour
{
    public GameManager gameManager;
    public CountdownTimer countdownTimer;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (countdownTimer != null)
                countdownTimer.StopTimer();

            gameManager.CompleteLevel();
        }
    }
}
