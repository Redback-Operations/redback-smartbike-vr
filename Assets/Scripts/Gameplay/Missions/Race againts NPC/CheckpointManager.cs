using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    public GameObject[] checkpoints;
    public NPCBikeManager npcBikeManager;
    public TextMeshProUGUI raceResultText;
    private int currentCheckpointIndex = 0;
    private float delay = 2.0f;
    private bool isDelayed = false;
    private bool raceFinished = false;
    private bool[] playerCheckpoints;
    private bool[] npcCheckpoints;
    private bool playerWon = false;

    IEnumerator StartResetting()
    {
        // Set resetting flag to true to prevent multiple coroutines running simultaneously
        isDelayed = true;

        // Wait for the specified delay before resetting the text
        yield return new WaitForSeconds(delay);
        raceResultText.text = null;

        // Reset the text
        isDelayed = false;
    }
    void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        playerCheckpoints = new bool[checkpoints.Length];
        npcCheckpoints = new bool[checkpoints.Length];
        raceResultText.text = null;
    }
    public void CheckpointReached(GameObject checkpoint, GameObject bike)
    {
        int checkpointIndex = System.Array.IndexOf(checkpoints, checkpoint);
        if (checkpointIndex == -1) return;

        if (bike.CompareTag("Player"))
        {
            playerCheckpoints[checkpointIndex] = true;
        }
        else if (bike.CompareTag("NPC"))
        {
            npcCheckpoints[checkpointIndex] = true;
        }

        if (checkpointIndex == currentCheckpointIndex)
        {
            currentCheckpointIndex++;
            if (currentCheckpointIndex == checkpoints.Length)
            {
                RaceFinished();
            }
            else if (currentCheckpointIndex == 1)
            {
                StartRace();
            }
        }
    }

    private void StartRace()
    {
        npcBikeManager.StartRace();
    }

    private void RaceFinished()
    {
        if (!raceFinished)
        {
            raceFinished = true;

            // Determine if the player won or lost
            bool playerWon = DeterminePlayerWin();

            // Display the result on the screen
            if (playerWon)
            {
                raceResultText.text = "You Win!";
                raceResultText.color = Color.green;
                StartCoroutine(StartResetting());
            }
            else
            {
                raceResultText.text = "You Lose!";
                raceResultText.color = Color.red;
                StartCoroutine(StartResetting());
            }
        }
    }

    private bool DeterminePlayerWin()
    {
        // Check if player passed all checkpoints before any NPC
        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (playerCheckpoints[i] && !npcCheckpoints[i])
            {
                continue; // Player is ahead at this checkpoint
            }
            if (!playerCheckpoints[i] && npcCheckpoints[i])
            {
                return false; // NPC is ahead at this checkpoint
            }
        }
        return true;
    }
}
