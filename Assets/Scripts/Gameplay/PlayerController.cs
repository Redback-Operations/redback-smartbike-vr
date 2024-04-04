using UnityEngine;

using System;
using System.Collections;
using System.Net;

using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;

using TMPro;

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
        if (Input.GetKey(KeyCode.W))
        {
            // Move the cycle in the camera's forward direction made by Jai
            MoveForward();
        }

        if (Input.GetKey(KeyCode.S))
        {
            // Move the cycle in the camera's back direction made by Jai
            MoveBackwards();
        }

        if (Input.GetKey(KeyCode.A) || (L_Turn == "LEFT")) // Krishin only added in the (L_Turn == "LEFT") part
        {
            // Move the cycle in the camera's forward direction made by Jai
            RotationLeft();
        }

        if (Input.GetKey(KeyCode.D) || (R_Turn == "RIGHT")) // Krishin only added in the (R_Turn == "RIGHT") part
        {
            // Move the cycle in the camera's forward direction made by Jai
            RotationRight();
        }
    }

    void MoveForward()
    {
        // Get the camera's forward direction made by Jai
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0; // To prevent the bike from moving in the Y-axis

        // Move the bike in the camera's forward direction made by Jai
        transform.position += cameraForward.normalized * movementSpeed * Time.deltaTime;
    }

    void MoveBackwards()
    {
        // Get the camera's forward direction made by Jai
        Vector3 cameraBackwards = Camera.main.transform.forward;
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
        //To make bike go towards new rotation made by Jai
        rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, OldRotation, 0.05f);
    }

    void RotationRight()
    {
        //To make chage in rotation made by Jai
        change -= 1;
        //To set new rotation made by Jai
        OldRotation = Quaternion.Euler(rb.transform.rotation.x, rb.transform.rotation.y - change, rb.transform.rotation.z);
        //To make bike go towards new rotation made by Jai
        rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, OldRotation, 0.05f);
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
