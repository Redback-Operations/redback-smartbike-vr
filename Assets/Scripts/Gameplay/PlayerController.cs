using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR;
using Gameplay.BikeMovement;

public class PlayerController : MonoBehaviour
{
    public float movementSpeed = 5f;
    public int score;

    [SerializeField] private bool overridePlayerPrefs;
    [SerializeField] private GameObject bikeMovementHandler;

    [SerializeField] private MovementHandleTypePair[] movementHandleTypePairs;

    [Serializable]
    public class MovementHandleTypePair
    {
        public string type;
        public GameObject movementHandler;
    }

    //For speed reference made by Dennis
    private float originalSpeed;
    private IPlayerInput _playerInput;
    private IBikeMover _bikeMover;


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
        //to set score to 0 made by Jai
        score = 0;

        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left, devices);

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
            _bikeMover = bikeMovementHandler.GetComponent<IBikeMover>();
            _bikeMover.Speed = movementSpeed;
            _bikeMover.Init(gameObject);
        }
        else
        {
            var handler =
                movementHandleTypePairs.FirstOrDefault((pair) =>
                    pair.type == PlayerPrefs.GetString("BikeControllerType"));
            if (handler != null)
            {
                _bikeMover = handler.movementHandler.GetComponent<IBikeMover>();
                _bikeMover.Speed = movementSpeed;
                _bikeMover.Init(gameObject);
            }
        }
    }

    void Update()
    {
        // TODO this should be moved into a mission start system, create a mission activate zone
        if (Mission_Activator.ActiveMission != null)
        {
            if (!Mission_Activator.ActiveMission.MissionStarted)
                Mission_Activator.ActiveMission.StartMission();
        }
    }

    private void FixedUpdate()
    {
        if (_bikeMover != null && _playerInput != null)
        {
            _bikeMover.HanldeInput(_playerInput.GetDirection());
        }
    }

    // trigger system updated to be use Collectable MonoBehaviour by Jonathan
    void OnTriggerEnter(Collider other)
    {
        var collectable = other.GetComponent<Collectable>();
        if (collectable != null)
        {
            if (collectable.Tag == this.tag)
                score += collectable.Collect();
            UIManager.Instance.SetScore(score);
        }
        else
        {
            // old collision code by Jai
            // TODO replace other pickups with collectable script as above, see Prefabs/Pickups/Star for example
            if (other.tag == "1")
            {
                score = score + 1;
                other.gameObject.SetActive(false);
            }

            if (other.tag == "2")
            {
                score = score + 2;
                other.gameObject.SetActive(false);
            }

            if (other.tag == "5")
            {
                score = score + 5;
                other.gameObject.SetActive(false);
            }
        }
    }

    //For speed reference made by Dennis
    public float GetSpeed()
    {
        return movementSpeed;
    }

    public void SetSpeed(float newSpeed) //Update achieved speed made by Dennis
    {
        movementSpeed = newSpeed;
    }

    public float GetOriginalSpeed()
    {
        return originalSpeed;
    }
}