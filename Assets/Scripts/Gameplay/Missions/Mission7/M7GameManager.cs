using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class M7GameManager : MonoBehaviour
{
    public int score;
    public static M7GameManager inst;
    public RoadSpawner roadSpawner;
    public Transform playerTransform;

    public Text scoreText;
    public Text speedText;

    private PlayMove playMove;
    private PlayerReset playerReset;

    public void IncreaseScore()
    {
        score++;
        playMove.IncreaseMaxSpeed();
    }

    private void Awake()
    {
        inst = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        playMove = playerTransform.GetComponent<PlayMove>();
        playerReset = playerTransform.GetComponent<PlayerReset>();

        roadSpawner.manager = this;
        roadSpawner.Reset();
    }

    // Update is called once per frame
    void Update()
    {

        if (playerTransform.transform.position.y < -5f)
        {
            playerReset.ResetToStartPoint();
            score = 0;
            roadSpawner.Reset();
        }

        speedText.text = $"Speed: {playMove.currentSpeed:F1}";
        scoreText.text = $"Score: {score:F1}"; //temporarily removed
    }
}
