using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Collections;
using TMPro;
using UnityEngine.XR;

public class PlayerController : MonoBehaviour
{
    public float movementSpeed = 5f;
    private Rigidbody rb;

    //For score made by Jai
    public int score;

    public TextMeshProUGUI scoreUI;

    //to store rotation made by Jai
    private Quaternion OldRotation;

    private float change = 0;


    //For IOT Mad by Kirshin
    protected MqttClient client;
    private string clientId = Guid.NewGuid().ToString();

    public string R_Turn = "LOW";
    public string L_Turn = "LOW";


    //For mission complete made by Dennis
    public TextMeshProUGUI missionCompleteText;
    private bool missionCompleted = false;

    //For achieve the fastest speed made by Dennis
    private bool timerActive = false;
    private float timerStartTime;




    private InputDevice? _controller;
    private Vector2 _direction = Vector2.zero;

    private float originalSpeed;

    void Start()
    {
        originalSpeed = movementSpeed;
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

        //// Check for the "W" key press made by Jai
        //if (IsForward())
        //{
        //    // Move the cycle in the camera's forward direction made by Jai
        //    MoveForward();
        //}

        //if (IsBackward())
        //{
        //    // Move the cycle in the camera's back direction made by Jai
        //    MoveBackwards();
        //}

        //if (IsLeft()) // Krishin only added in the (L_Turn == "LEFT") part
        //{
        //    // Move the cycle in the camera's forward direction made by Jai
        //    RotationLeft();
        //}

        //if (IsRight()) // Krishin only added in the (R_Turn == "RIGHT") part
        //{
        //    // Move the cycle in the camera's forward direction made by Jai
        //    RotationRight();
        //}

        UpdateDirection();

        MovePlayer();
        RotatePlayer();

        //To make bike go towards new rotation made by Jai
        rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, OldRotation, 0.05f);


        //For achieve the fastest speed made by Dennis
        if (timerActive)
        {
            float currentTime = Time.time;
            missionCompleteText.text = "Timer: " + (currentTime - timerStartTime).ToString("F1") + "s";
        }
    }

    public void UpdateDirection()
    {
        _direction = Vector2.zero;

        if (XRSettings.enabled)
        {
            _direction = ControllerDirection();
        }
        else
        {
            if (Input.GetKey(KeyCode.W))
                _direction.y += 1;

            if (Input.GetKey(KeyCode.S))
                _direction.y -= 1;

            if (Input.GetKey(KeyCode.A))
                _direction.x -= 1;

            if (Input.GetKey(KeyCode.D))
                _direction.x += 1;
        }

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

        // Move the bike in the player's forward direction made by Jai (updated by Jonathan)
        transform.position += (facingDirection.normalized * movementSpeed * Time.deltaTime * _direction.y);
    }

    private void RotatePlayer()
    {
        change += _direction.x;
        OldRotation = Quaternion.Euler(rb.transform.rotation.x, rb.transform.rotation.y + change, rb.transform.rotation.z);
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

    //For speed reference made by Dennis
    public float GetSpeed()
    {
        return movementSpeed;
    }

    public void SetSpeed(float newSpeed) //Update achieved speed made by Dennis
    {
        if (!timerHasBeenActivated && newSpeed > movementSpeed)
        {
            timerActive = true;
            timerHasBeenActivated = true;
            timerStartTime = Time.time;
            missionCompleteText.text = "0.0s";
        }

        movementSpeed = newSpeed;

        if (timerActive && newSpeed >= 5 * originalSpeed)
        {
            timerActive = false;
            float endTime = Time.time;
            missionCompleteText.text = "Timer: " + (endTime - timerStartTime).ToString("F1") + "s";
            StartCoroutine(ClearMissionCompleteText());
        }
    }

    public float GetOriginalSpeed()
    {
        return originalSpeed;
    }


    //For exchange apple with score and display message made by Dennis
    public void DecrementScore()
    {
        score--;

        if (!missionCompleted)
        {
            missionCompleteText.text = "Mission Complete";
            missionCompleted = true;
            StartCoroutine(DisplayMissionCompleteText());
        }


    }

    //For display message made by Dennis
    private IEnumerator DisplayMissionCompleteText()
    {
        yield return new WaitForSeconds(3f);
        missionCompleteText.text = ""; 
    }


    private bool timerHasBeenActivated = false;

    //For clear message made by Dennis
    private IEnumerator ClearMissionCompleteText()
    {
        yield return new WaitForSeconds(2.5f);
        missionCompleteText.text = "Speed up Complete";
        yield return new WaitForSeconds(3); 
        missionCompleteText.text = "";
    }






}






