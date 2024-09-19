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
    private bool _braking = false;

    private float _groundHeight = 0;
    private float _angleX = 0;

    //Improve bike movement made by Dennis
    private float currentBrakeForce;
    private float currentSteeringAngle;
    public Transform handle;
    private Quaternion initialHandleRotation;
    [SerializeField] private float motorForce;
    [SerializeField] private float brakeForce;
    private float maxSteeringAngle;
    private float speedSteerControlTime = 0.0756F;
    [Range(0f, 0.1f)][SerializeField] private float turnSmoothing;
    [SerializeField] private WheelCollider frontWheel;
    [SerializeField] private WheelCollider backWheel;
    [SerializeField] private Transform frontWheelTransform;
    [SerializeField] private Transform backWheelTransform;
    public float minXRotation = -45f;
    public float maxXRotation = 45f;


    [SerializeField] private float currentSpeed;

    void Start()
    {
        // get the initial rotation of the player
        change = transform.rotation.eulerAngles.y;

        // Store the initial rotation of the handle
        initialHandleRotation = handle.localRotation;

        // Get the Rigidbody component attached to the bike GameObject
        rb = GetComponent<Rigidbody>();

        // Freeze rotation along the Z axes to prevent tumbling
        rb.constraints = RigidbodyConstraints.FreezeRotationZ;

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
        //For bike movement made by Dennis
        UpdateDirection();
        //GetInput();

        HandleEngine();
        HandleSteering();
        UpdateWheels();
        UpdateHandle();

        //For testing the bike movement
        currentSpeed = rb.velocity.magnitude;

    }

    void FixedUpdate()
    {
        Vector3 currentRotation = rb.rotation.eulerAngles;
        float clampedXRotation = currentRotation.x > 180 ? currentRotation.x - 360 : currentRotation.x;
        clampedXRotation = Mathf.Clamp(clampedXRotation, minXRotation, maxXRotation);
        rb.rotation = Quaternion.Euler(clampedXRotation, currentRotation.y, 0);
    }

    //For bike movement input made by Dennis  (Using UpdateDirection() now)
    /*public void GetInput()
    {
        _direction.x = Input.GetAxis("Horizontal");
        _direction.y = Input.GetAxis("Vertical");
        _braking = Input.GetKey(KeyCode.Space);
    }*/

    //For bike movement - Controls the motor torque and braking force based on user input. Made by Dennis
    public void HandleEngine()
    {
        backWheel.motorTorque = _direction.y * motorForce;

        currentBrakeForce = _braking ? brakeForce : 0f;

        if (_braking)
        {
            ApplyBraking();
        }
        else
        {
            ReleaseBraking();
        }
    }

    //For bike movement - Applies brake torque to both wheels. Made by Dennis
    public void ApplyBraking()
    {
        frontWheel.brakeTorque = currentBrakeForce;
        backWheel.brakeTorque = currentBrakeForce;
    }

    //For bike movement - Releases brake torque on both wheels. Made by Dennis
    public void ReleaseBraking()
    {
        frontWheel.brakeTorque = 0;
        backWheel.brakeTorque = 0;
    }


    //For bike movement - Adjusts the maximum steering angle based on the vehicle's speed. Made by Dennis
    public void SpeedSteerinReductor()
    {
        if (rb.velocity.magnitude < 5)
        {
            maxSteeringAngle = Mathf.LerpAngle(maxSteeringAngle, 50, speedSteerControlTime);
        }
        else if (rb.velocity.magnitude >= 5 && rb.velocity.magnitude < 10)
        {
            maxSteeringAngle = Mathf.LerpAngle(maxSteeringAngle, 30, speedSteerControlTime);
        }
        else if (rb.velocity.magnitude >= 10 && rb.velocity.magnitude < 15)
        {
            maxSteeringAngle = Mathf.LerpAngle(maxSteeringAngle, 15, speedSteerControlTime);
        }
        else if (rb.velocity.magnitude >= 15 && rb.velocity.magnitude < 20)
        {
            maxSteeringAngle = Mathf.LerpAngle(maxSteeringAngle, 10, speedSteerControlTime);
        }
        else if (rb.velocity.magnitude >= 20)
        {
            maxSteeringAngle = Mathf.LerpAngle(maxSteeringAngle, 5, speedSteerControlTime);
        }
    }

    //For bike movement - Manages the steering input and updates the front wheel's steer angle. Made by Dennis
    public void HandleSteering()
    {
        SpeedSteerinReductor();

        currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, maxSteeringAngle * _direction.x, turnSmoothing);
        frontWheel.steerAngle = currentSteeringAngle;
    }

    //For bike movement - Updates the handle rotation based on the front wheel's steering angle. Made by Dennis
    public void UpdateHandle()
    {
        handle.localRotation = initialHandleRotation * Quaternion.Euler(0, currentSteeringAngle, 0);
    }

    //For bike movement - Updates the wheel transforms. Made by Dennis
    public void UpdateWheels()
    {
        UpdateSingleWheel(frontWheel, frontWheelTransform);
        UpdateSingleWheel(backWheel, backWheelTransform);
    }

    //For bike movement - Updates the wheel transforms based on the wheel colliders. Made by Dennis
    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
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

        _braking = Input.GetKey(KeyCode.Space);

        if (!Mathf.Approximately(MQTT_Speed, 0))
            _direction.y = 1;

        if (L_Turn == "LEFT")
            _direction.x = -1;

        if (R_Turn == "RIGHT")
            _direction.x = 1;
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

    //For scoring made by Jai
    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);

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
        movementSpeed = newSpeed;
    }

    public float GetOriginalSpeed()
    {
        return originalSpeed;
    }
}
