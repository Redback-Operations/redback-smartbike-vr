using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Gameplay.BikeMovement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class M7GameManager : MonoBehaviour
{
    public int score;
    public static M7GameManager inst;
    public float speedGoal;


    public RoadSpawner roadSpawner;
    public PlayerController playerTransform;

    public Text speedTextTarget;

    public NetworkManagement NetworkManager;

    public float LifeRemainingStart;
    private float _lifeRemaining;
    private float _initialSpeed;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    public float ResetHeight;
    public float switchIntervalFast;
    public float switchIntervalSlow;
    private float _switchIntervalActive;

    public float speedGoalFast;
    public float speedGoalSlow;
    private float _speedGoalActive;

    private float _missionTimer;
    private bool speedGoalIsFastMode;

    public string TargetSceneName;
    public Button restartButton;
    public Button returnButton;
    private float _notificationDelay = 2.0f;
    private bool missionSuccess = false;
    private bool missionComplete = false;

    public void IncreaseScore()
    {
        if (playerTransform == null) return;

        //gain points by being above the required speed, lose points for not
        score++;
    }

    private void Awake()
    {
        inst = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(_RestartMission); // Adds the restart button as a listener for the onClick event
            returnButton.onClick.AddListener(_ReturnToGarage);

            restartButton.gameObject.SetActive(false); // Hides the restart button at the start
            returnButton.gameObject.SetActive(false);
        }
    }

    private void _ReturnToGarage()
    {
        if (NetworkManagement.Instance != null)
            NetworkManagement.Instance.Disconnect();

        // teleport the player to the target scene
        MapLoader.LoadScene(TargetSceneName);
    }

    private void _RestartMission()
    {
        //reset goals
        _lifeRemaining = LifeRemainingStart;
        speedGoalIsFastMode = false;
        _speedGoalActive = speedGoalSlow;
        _switchIntervalActive = switchIntervalSlow;
        _missionTimer = 0f;
        score = 0;

        //reset player and tiles
        _ResetPlayer();
        roadSpawner.Reset();

        restartButton.gameObject.SetActive(false);
        returnButton.gameObject.SetActive(false);
        missionComplete = false;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ClearObjectives();
            StartCoroutine(UIManager.Instance.ShowNotification("Survive HIIT", 5));
            UIManager.Instance.AddObjective($"Maintain {_speedGoalActive}m/s for {_switchIntervalActive}s");
        }
    }

    IEnumerator StartResetting()
    {
        Debug.Log("Start Resetting");
        yield return UIManager.Instance.ShowNotification(missionSuccess ? "Mission Complete!" : "Mission Failed!", _notificationDelay);
        yield return UIManager.Instance.ClearObjectives();

        if (!missionSuccess)
        {
            restartButton.gameObject.SetActive(true); // Show Restart Button on mission fail
            returnButton.gameObject.SetActive(true); // Show Restart Button on mission fail
        }
    }


    // Update is called once per frame
    void Update()
    {
        float playerSpeed;

        //get network manager and player before doing anything
        if (NetworkManager == null) return;
        if (playerTransform == null) return;
        if (missionComplete) return;

        if (_lifeRemaining <= 0f)
        {
            _lifeRemaining = 0f;
            missionComplete = true;
            StartCoroutine(StartResetting());
            UIManager.Instance.SetObjectiveState(-1, UIManager.ObjectiveState.Failed);
            return;
        }

        if (playerTransform.transform.position.y < ResetHeight)
        {
            //restart the mission
            missionComplete = true;
            StartCoroutine(StartResetting());
            UIManager.Instance.SetObjectiveState(-1, UIManager.ObjectiveState.Failed);
            return;
        }

        _missionTimer += Time.deltaTime;
        if (_missionTimer >= _switchIntervalActive)
        {
            //flip the speed goal between fast and slow
            speedGoalIsFastMode = !speedGoalIsFastMode;

            //flip speed mode for HIIT
            if (speedGoalIsFastMode) {
                _switchIntervalActive = switchIntervalFast;
                _speedGoalActive = speedGoalFast; }
            else {
                _switchIntervalActive = switchIntervalSlow;
                _speedGoalActive = speedGoalSlow;
            }

            _missionTimer = 0f;
            UIManager.Instance.RemoveObjective(1);
            UIManager.Instance.AddObjective($"Maintain:\n{_speedGoalActive} for {_switchIntervalActive}s");
        }

        //add life for going fast enough, remove life for going too slow
        playerSpeed = playerTransform.RelativeSpeed.z;
        if (playerSpeed < _speedGoalActive) _lifeRemaining -= Time.deltaTime;
        else _lifeRemaining += Time.deltaTime;

        //update player score and VR score UI
        if (UIManager.Instance != null) {
            UIManager.Instance.SetScore(score);
            UIManager.Instance.UpdateTime("Time Left", _lifeRemaining);
        }

        //update speed score
        if (speedTextTarget != null) speedTextTarget.text = $"Speed: {playerSpeed:F1}";
    }

    //resets the player position, speed, rotation
    private void _ResetPlayer()
    {
        //reset rigid body movement
        Rigidbody rb = playerTransform.gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        //move player back to init point
        playerTransform.transform.position = _initialPosition;
        playerTransform.SetRotation(_initialRotation);
        playerTransform.SetSpeed(_initialSpeed);
    }

    private void OnEnable()
    {
        NetworkManagement.OnManagerReady += OnManagerReady;
        PlayerController.OnPlayerControllerReady += OnPlayerReady;
        UIManager.OnPlayerUIManagerReady += OnPlayerUIReady;
    }
    private void OnDisable()
    {
        NetworkManagement.OnManagerReady -= OnManagerReady;
        PlayerController.OnPlayerControllerReady -= OnPlayerReady;
        UIManager.OnPlayerUIManagerReady -= OnPlayerUIReady;
        playerTransform = null;
    }

    private void OnManagerReady(NetworkManagement mgr)
    {
        NetworkManager = mgr;
    }

    //initialize player
    private void OnPlayerReady(PlayerController player)
    {
        playerTransform = player;

        if (playerTransform.BikeMover is SimpleBikeController simpleBikeController)
        {
            simpleBikeController.BikeGroundMode = SimpleBikeController.GroundLockMode.FreePhysics;
        }
        _initialPosition = player.transform.position;
        _initialRotation = player.transform.rotation;
        _initialSpeed = playerTransform.GetOriginalSpeed();

        speedTextTarget.gameObject.SetActive(true);
        _RestartMission();
    }

    private void OnPlayerUIReady(UIManager UIManager) {
        UIManager.Instance = UIManager;

        UIManager.Instance.ClearObjectives();
        StartCoroutine(UIManager.Instance.ShowNotification("Survive HIIT", 5));
        UIManager.Instance.AddObjective($"Maintain {_speedGoalActive}m/s for {_switchIntervalActive}s");
    }
}