using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

using TMPro;
using UnityEngine.XR;

public class PlayerController : MonoBehaviour
{
    public float movementSpeed = 5f;
    private Rigidbody rb;
    
    //For score made by Jai
    private int score;
    
    public TextMeshProUGUI scoreUI;

    //to store rotation made by Jai
    private Quaternion OldRotation;
    private int change = 0;
    

    //For IOT Mad by Kirshin
    protected MqttClient client;
    private string clientId = Guid.NewGuid().ToString();
    
    public string R_Turn = "LOW";
    public string L_Turn = "LOW";

    private InputDevice? _controller;

    void Start()
    {
        // Get the Rigidbody component attached to the bike GameObject
        rb = GetComponent<Rigidbody>();

        // Freeze rotation along the X and Z axes to prevent tumbling
        rb.freezeRotation = true;

        //to set score to 0 made by Jai
        score = 0;

        //IOT Made by Krishin
        client = new MqttClient("broker.emqx.io");
        client.MqttMsgPublishReceived += client_MqttMsgReceived;
        client.Subscribe(new string[] { "Turn/Right" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
        client.Subscribe(new string[] { "Turn/Left" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });

        client.Connect(clientId);

        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left, devices);

        if (devices.Any())
            _controller = devices.FirstOrDefault();
    }
    
    void client_MqttMsgReceived(object sender, MqttMsgPublishEventArgs e)
    {
        if (e.Topic == "Turn/Right")
        {
            R_Turn = System.Text.Encoding.UTF8.GetString(e.Message);
        }
        else
        {
            L_Turn = System.Text.Encoding.UTF8.GetString(e.Message);
        }
    }

    void Update()
    {
        //To check score made by Jai
        scoreUI.text = score.ToString();

        // Check for the "W" key press made by Jai
        if (IsForward())
        {
            // Move the cycle in the camera's forward direction made by Jai
            MoveForward();
        }

        if (IsBackward())
        {
            // Move the cycle in the camera's back direction made by Jai
            MoveBackwards();
        }

        if (IsLeft()) // Krishin only added in the (L_Turn == "LEFT") part
        {
            // Move the cycle in the camera's forward direction made by Jai
            RotationLeft();
        }

        if (IsRight()) // Krishin only added in the (R_Turn == "RIGHT") part
        {
            // Move the cycle in the camera's forward direction made by Jai
            RotationRight();
        }

        //To make bike go towards new rotation made by Jai
        rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, OldRotation, 0.05f);
    }

    public bool IsForward()
    {
        if (Input.GetKey(KeyCode.W))
            return true;

        return ControllerDirection().y > 0;
    }

    public bool IsBackward()
    {
        if (Input.GetKey(KeyCode.S))
            return true;

        return ControllerDirection().y < 0;
    }

    public bool IsLeft()
    {
        if (Input.GetKey(KeyCode.A) || L_Turn == "LEFT")
            return true;

        return ControllerDirection().x < 0;
    }

    public bool IsRight()
    {
        if (Input.GetKey(KeyCode.D) || L_Turn == "RIGHT")
            return true;

        return ControllerDirection().x > 0;
    }

    private Vector2 ControllerDirection()
    {
        if (_controller == null)
            return Vector2.zero;

        return _controller.Value.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 value) ? value : Vector2.zero;
    }

    void MoveForward()
    {
        // Get the camera's forward direction made by Jai
        Vector3 cameraForward = transform.forward;
        cameraForward.y = 0; // To prevent the bike from moving in the Y-axis

        // Move the bike in the camera's forward direction made by Jai
        transform.position += cameraForward.normalized * movementSpeed * Time.deltaTime;
    }

    void MoveBackwards()
    {
        // Get the camera's forward direction made by Jai
        Vector3 cameraBackwards = transform.forward;
        cameraBackwards.y = 0; // To prevent the bike from moving in the Y-axis

        // Move the bike in the camera's forward direction made by Jai
        transform.position -= cameraBackwards.normalized * movementSpeed * Time.deltaTime;
    }

    void RotationLeft()
    {
        //To make chage in rotation made by Jai
        change += 1;
        //To set new rotation
        OldRotation = Quaternion.Euler(rb.transform.rotation.x, rb.transform.rotation.y - change, rb.transform.rotation.z);
    }

    void RotationRight()
    {
        //To make chage in rotation made by Jai
        change -= 1;
        //To set new rotation made by Jai
        OldRotation = Quaternion.Euler(rb.transform.rotation.x, rb.transform.rotation.y - change, rb.transform.rotation.z);
    }

    //For scoring made by Jai

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "1")
        {
            score = score + 1;
            other.gameObject.SetActive(false);
        }

        if(other.tag == "2")
        {
            score = score + 2;
            other.gameObject.SetActive(false);
        }

        if(other.tag == "5")
        {
            score = score + 5;
            other.gameObject.SetActive(false);
        }
    }
}
