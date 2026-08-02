
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
    [SerializeField] private GameObject bikeMovementHandler;

    [SerializeField] private MovementHandleTypePair[] movementHandleTypePairs;
    [SerializeField] private SpeedListener speedListener;

    // Scene to load when F1 or the Oculus X button is pressed
    private string sceneToLoad = "GarageScene";

    
    public class MovementHandleTypePair
    {
        public string type;
        public GameObject movementHandler;
    }
    
    public PlayerInventory inventory;

    // For speed reference made by Dennis
    private float originalSpeed;
    private IPlayerInput _playerInput;
    private IBikeMover _bikeMover;
    public IBikeMover BikeMover => _bikeMover;
    public Vector3 RelativeSpeed { get; private set; }
    public static event Action<PlayerController> OnPlayerControllerReady;

    private EventBinding<ItemAddedEvent> itemAddedEventBinding;

    private void OnEnable()
    {
        itemAddedEventBinding = new EventBinding<ItemAddedEvent>(HandleItemAdded);
        EventBus<ItemAddedEvent>.Register(itemAddedEventBinding);
    }

    private void HandleItemAdded(ItemAddedEvent itemAddedEvent)
    {
        inventory.AddItem(itemAddedEvent.itemName, itemAddedEvent.quantity);
    }

    private void OnDisable()
    {
        EventBus<ItemAddedEvent>.Deregister(itemAddedEventBinding);
    }

    private void OnValidate()
    {
        if (bikeMovementHandler == null) return;

        if (bikeMovementHandler.GetComponent<IBikeMover>() == null)
        {
            bikeMovementHandler = null;
            Debug.LogWarning(
                $"Bike movement handler object should have scripts that implements interface:{typeof(IBikeMover)}");
        }
    }

    IEnumerator Start()
    {
        originalSpeed = movementSpeed;

        // To set score to 0 made by Jai
        score = 0;

        var devices = new List<InputDevice>();

        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left,
            devices);

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

        Debug.Log($"MQTT INSTANCE exists:{Mqtt.Instance}", Mqtt.Instance);
        Debug.Log($"Player input:{_playerInput.GetType()}");

        yield return null;

        if (overridePlayerPrefs)
        {
            SetupBikeMover(bikeMovementHandler);
        }
        else
        {
            var handler =
                movementHandleTypePairs.FirstOrDefault((pair) =>
                    pair.type == PlayerPrefs.GetString("BikeControllerType", "Simple"));

            if (handler != null)
            {
                SetupBikeMover(handler.movementHandler);
            }
        }
    }

    private void SetupBikeMover(GameObject handler)
    {
        _bikeMover = handler.GetComponent<IBikeMover>();
        _bikeMover.Speed = movementSpeed;
        OnPlayerControllerReady?.Invoke(this);
        _bikeMover.Init(gameObject);
    }

    void Update()
    {
        // Check for F1 or Oculus/Meta Y button
        CheckSceneChangeInput();

        // TODO this should be moved into a mission start system, create a mission activate zone
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
            leftHandDevices);

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
            Debug.LogWarning("No scene has been assigned to 'Scene To Load' in the PlayerController.");
            return;
        }

        Debug.Log($"Loading scene: {sceneToLoad}");
        SceneManager.LoadScene(sceneToLoad);
    }

    public void Tick(float deltaTime)
    {
        if (_bikeMover == null) return;

        _bikeMover.DeltaTime = deltaTime;

        Vector2 input = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );

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

        if (_bikeMover != null)
            _bikeMover.Speed = movementSpeed;
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
