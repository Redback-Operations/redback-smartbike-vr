
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR;
using UnityEngine.SceneManagement;
using Gameplay.BikeMovement;

public class PlayerController : MonoBehaviour
{
    public float movementSpeed = 5f;
    public int score;

    [SerializeField] private bool overridePlayerPrefs;

    [Tooltip("Logs the raw input vector every frame. Leave off - it floods the console.")]
    [SerializeField] private bool logInputEveryFrame;

    [SerializeField] private GameObject bikeMovementHandler;

    [SerializeField] private MovementHandleTypePair[] movementHandleTypePairs;
    [SerializeField] private SpeedListener speedListener;

    private string sceneToLoad = "GarageScene";

    public class MovementHandleTypePair
    {
        public string type;
        public GameObject movementHandler;
    }

    public PlayerInventory inventory;

    private float originalSpeed;
    private IPlayerInput _playerInput;
    private IBikeMover _bikeMover;

    public IBikeMover BikeMover => _bikeMover;
    public Vector3 RelativeSpeed { get; private set; }

    public bool UsingLiveTelemetry => _playerInput is MQTTInput;

    public float LiveSpeed
    {
        get
        {
            if (_playerInput is MQTTInput mqttInput)
                return mqttInput.LiveSpeed;

            return 0f;
        }
    }

    public float LiveCadence
    {
        get
        {
            if (_playerInput is MQTTInput mqttInput)
                return mqttInput.LiveCadence;

            return 0f;
        }
    }

    public static event Action<PlayerController> OnPlayerControllerReady;

    private EventBinding<ItemAddedEvent> itemAddedEventBinding;

    private void OnEnable()
    {
        itemAddedEventBinding =
            new EventBinding<ItemAddedEvent>(HandleItemAdded);

        EventBus<ItemAddedEvent>.Register(itemAddedEventBinding);
    }

    private void HandleItemAdded(ItemAddedEvent itemAddedEvent)
    {
        inventory.AddItem(
            itemAddedEvent.itemName,
            itemAddedEvent.quantity
        );
    }

    private void OnDisable()
    {
        EventBus<ItemAddedEvent>.Deregister(itemAddedEventBinding);

        ReleasePlayerInput();
    }

    private void ReleasePlayerInput()
    {
        if (_playerInput is IDisposable disposableInput)
        {
            disposableInput.Dispose();
        }

        _playerInput = null;
    }

    private void OnValidate()
    {
        if (bikeMovementHandler == null)
            return;

        if (bikeMovementHandler.GetComponent<IBikeMover>() == null)
        {
            bikeMovementHandler = null;

            Debug.LogWarning(
                $"Bike movement handler object should have scripts that implements interface:{typeof(IBikeMover)}"
            );
        }
    }

    IEnumerator Start()
    {
        originalSpeed = movementSpeed;

        // To set score to 0 made by Jai
        score = 0;

        yield return null;

        SetupPlayerInput();

        if (overridePlayerPrefs)
        {
            SetupBikeMover(bikeMovementHandler);
        }
        else
        {
            var handler =
                movementHandleTypePairs.FirstOrDefault((pair) =>
                    pair.type == PlayerPrefs.GetString(
                        "BikeControllerType", "Simple"
                    )
                );

            if (handler != null)
            {
                SetupBikeMover(handler.movementHandler);
            }
        }
    }

    private void SetupPlayerInput()
    {
        ReleasePlayerInput();

        var devices = new List<InputDevice>();

        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.HeldInHand |
            InputDeviceCharacteristics.Left,
            devices
        );

        if (Mqtt.Instance != null && Mqtt.Instance.IsConnected)
        {
            _playerInput = new MQTTInput();
        }
        else if (devices.Any())
        {
            _playerInput = new XRInput(devices.FirstOrDefault());
        }
        else
        {
            _playerInput = new AxisInput();
        }

        Debug.Log(
            $"Player input: {_playerInput.GetType().Name}"
        );
    }

    private void SetupBikeMover(GameObject handler)
    {
        if (handler == null)
        {
            Debug.LogWarning("No bike movement handler assigned.");
            return;
        }

        _bikeMover = handler.GetComponent<IBikeMover>();

        if (_bikeMover == null)
        {
            Debug.LogWarning("Bike movement handler is missing IBikeMover.");
            return;
        }

        _bikeMover.Speed = movementSpeed;

        OnPlayerControllerReady?.Invoke(this);

        _bikeMover.Init(gameObject);
    }

    void Update()
    {
        CheckSceneChangeInput();

        if (Mission_Activator.ActiveMission != null)
        {
            if (!Mission_Activator.ActiveMission.MissionStarted)
                Mission_Activator.ActiveMission.StartMission();
        }
    }

    private void CheckSceneChangeInput()
    {
        // F1 keyboard input
        if (Input.GetKeyDown(KeyCode.F1))
        {
            LoadTargetScene();
            return;
        }

        // Oculus/Meta Y button
        var leftHandDevices = new List<InputDevice>();

        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.HeldInHand |
            InputDeviceCharacteristics.Left |
            InputDeviceCharacteristics.Controller,
            leftHandDevices
        );

        foreach (var device in leftHandDevices)
        {
            if (device.TryGetFeatureValue(
                    CommonUsages.primaryButton,
                    out bool primaryButtonPressed) &&
                primaryButtonPressed)
            {
                LoadTargetScene();
                return;
            }
        }
    }

    private void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning(
                "No scene has been assigned to 'Scene To Load' in the PlayerController."
            );

            return;
        }

        Debug.Log($"Loading scene: {sceneToLoad}");

        SceneManager.LoadScene(sceneToLoad);
    }

    public void Tick(float deltaTime)
    {
        if (_bikeMover == null || !isActiveAndEnabled)
            return;

        if (_playerInput == null)
            SetupPlayerInput();

        bool mqttConnected =
            Mqtt.Instance != null &&
            Mqtt.Instance.IsConnected;

        // Switch to live telemetry if MQTT connects after startup.
        if (mqttConnected && !UsingLiveTelemetry)
        {
            SetupPlayerInput();
        }

        _bikeMover.DeltaTime = deltaTime;

        Vector2 input;

        if (_playerInput is MQTTInput mqttInput)
        {
            input = mqttInput.GetDirection();

            float liveSpeed = mqttConnected
                ? input.y
                : 0f;

            // Both bike movers multiply Speed by input.y.
            // Use the measured speed directly without multiplying it.
            _bikeMover.Speed = liveSpeed;

            input.y = liveSpeed > 0f ? 1f : 0f;

            if (!mqttConnected)
                input.x = 0f;
        }
        else
        {
            _bikeMover.Speed = movementSpeed;

            input = _playerInput.GetDirection();
        }

        if (logInputEveryFrame)
        {
            Debug.Log(
                $"PlayerController input: {input} | " +
                $"Speed: {LiveSpeed:F2} m/s | " +
                $"Cadence: {LiveCadence:F1} RPM"
            );
        }

        _bikeMover.HanldeInput(input);
    }

    void OnTriggerEnter(Collider other)
    {
        var collectable = other.GetComponent<Collectable>();

        if (collectable != null)
        {
            if (collectable.Tag == this.tag)
            {
                score += collectable.Collect();

                UIManager.Instance.SetScore(score);
            }
        }
    }

    // For speed reference made by Dennis
    public float GetSpeed()
    {
        return movementSpeed;
    }

    public void SetSpeed(float newSpeed)
    {
        movementSpeed = newSpeed;

        if (_bikeMover != null && !UsingLiveTelemetry)
        {
            _bikeMover.Speed = movementSpeed;
        }
    }

    public float GetOriginalSpeed()
    {
        return originalSpeed;
    }

    public void SetRotation(Quaternion initialRotation)
    {
        transform.rotation = initialRotation;
    }
}
