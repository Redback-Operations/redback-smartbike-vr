using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Collections;
using TMPro;
using UnityEngine.XR;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using UnityEditor.Rendering;

public class PlayerController : MonoBehaviour
{
    public float movementSpeed = 5f;
    private float originalSpeed = 5f;

    public float rotationSpeed = 90f;

    private Rigidbody rb;

    //For score made by Jai
    public int score;

    //to store rotation made by Jai
    private Quaternion OldRotation;

    private float change = 0;

    public string R_Turn = "LOW";
    public string L_Turn = "LOW";
    public float MQTT_Speed = 0f;

    public Transform Bikes;

    private InputDevice? _controller;
    private Vector2 _direction = Vector2.zero;

    private float _groundHeight = 0;
    private float _angleX = 0;

    private BoxCollider _collider;

    void Start()
    {
        // get the initial rotation of the player
        change = transform.rotation.eulerAngles.y;

        // Get the Rigidbody component attached to the bike GameObject
        rb = GetComponent<Rigidbody>();
        _collider = GetComponent<BoxCollider>();

        // Freeze rotation along the X and Z axes to prevent tumbling
        rb.freezeRotation = true;

        //to set score to 0 made by Jai
        score = 0;

        // subscribe to the MQTT topics required
        if (Mqtt.Instance != null)
        {
            Mqtt.Instance.Subscribe(client_MqttMsgReceived, Mqtt.LeftTurnTopic);
            Mqtt.Instance.Subscribe(client_MqttMsgReceived, Mqtt.RightTurnTopic);
            Mqtt.Instance.Subscribe(client_MqttMsgReceived, Mqtt.SpeedTopic);
        }

        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left, devices);

        if (devices.Any())
            _controller = devices.FirstOrDefault();
    }

    void client_MqttMsgReceived(object sender, MqttMsgPublishEventArgs e)
    {
        if (e.Topic == Mqtt.RightTurnTopic)
        {
            R_Turn = System.Text.Encoding.UTF8.GetString(e.Message);
        }
        else if (e.Topic == Mqtt.LeftTurnTopic)
        {
            L_Turn = System.Text.Encoding.UTF8.GetString(e.Message);
        }
        else if (e.Topic == Mqtt.SpeedTopic)
        {
            string json = System.Text.Encoding.UTF8.GetString(e.Message);
            string valueKey = "\"value\":";
            int startIndex = json.IndexOf(valueKey) + valueKey.Length;
            int endIndex = json.IndexOf(",", startIndex);
            string valueStr = json.Substring(startIndex, endIndex - startIndex);
            MQTT_Speed = float.Parse(valueStr);
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

        UpdateDirection();

        MovePlayer();
        RotatePlayer();
    }

    void LateUpdate()
    {
        if (Terrain.activeTerrain == null)
            return;

        _groundHeight = Terrain.activeTerrain.SampleHeight(transform.position);

        var normal = CalculateNormal(Terrain.activeTerrain, transform.position);
        var forward = Vector3.Cross(normal, -transform.right);

        var rotation = Quaternion.LookRotation(forward, normal);
        _angleX = rotation.eulerAngles.x;
    }

    private Vector3 CalculateNormal(Terrain terrain, Vector3 position)
    {
        var offset = 0.1f;

        var heightLeft = terrain.SampleHeight(position + Vector3.left * offset);
        var heightRight = terrain.SampleHeight(position + Vector3.right * offset);
        var heightForward = terrain.SampleHeight(position + Vector3.forward * offset);
        var heightBack = terrain.SampleHeight(position + Vector3.back * offset);

        var tangentX = new Vector3(2.0f * offset, heightRight - heightLeft, 0).normalized;
        var tangentZ = new Vector3(0, heightForward - heightBack, 2.0f * offset).normalized;

        var normal = Vector3.Cross(tangentZ, tangentX).normalized;

        return normal;
    }

    public void UpdateDirection()
    {
        _direction = ControllerDirection();

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            _direction.y += 1;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            _direction.y -= 1;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            _direction.x -= 1;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            _direction.x += 1;

        if (!Mathf.Approximately(MQTT_Speed, 0))
            _direction.y = 1;

        if (L_Turn == "LEFT")
            _direction.x = -1;

        if (R_Turn == "RIGHT")
            _direction.x = 1;
    }

    private void MovePlayer()
    {
        // Get the playter's forward direction made by Jai (updated by Jonathan)
        Vector3 facingDirection = transform.forward;
        // To prevent the bike from moving in the Y-axis
        facingDirection.y = 0;

        var position = transform.position;

        // Move the bike in the player's forward direction made by Jai (updated by Jonathan)
        if (!Mathf.Approximately(MQTT_Speed, 0))
            position += (facingDirection.normalized * MQTT_Speed * Time.deltaTime * _direction.y);
        else
            position += (facingDirection.normalized * movementSpeed * Time.deltaTime * _direction.y);

        position.y = _groundHeight;

        transform.position = position;
    }

    private void RotatePlayer()
    {
        // updated to account for frame rate
        change += _direction.x * rotationSpeed * Time.deltaTime;

        //To make bike go towards new rotation made by Jai
        OldRotation = Quaternion.Euler(_angleX, rb.transform.rotation.y + change, rb.transform.rotation.z);
        //To make bike go towards new rotation made by Jai
        rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, OldRotation, 0.15f);
    }

    private Vector2 ControllerDirection()
    {
        if (!XRSettings.enabled)
            return Vector2.zero;

        if (_controller == null)
        {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left, devices);

            if (devices.Any())
                _controller = devices.FirstOrDefault();
        }

        var value = Vector2.zero;

        if (_controller?.TryGetFeatureValue(CommonUsages.primary2DAxis, out value) != true)
            return Vector2.zero;

        _direction = value;
        return value;
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
