using System;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Globalization;

public class SpeedListener : MonoBehaviour
{
    public bool subscribed = false;
    public float speed = 0.0f;
    public bool logMessages = true;
    public float turnSpeed = 60.0f;
    private readonly object _stateLock = new object();
    private float _pendingSpeed = 0.0f;
    private float _pendingTurn = 0.0f;
    private bool _pendingBrake = false;

    void Update()
    {
        if (Mqtt.Instance != null){
            if (Mqtt.Instance.IsConnected && !subscribed)
            {
                Mqtt.Instance.Subscribe(OnMessage, Mqtt.SpeedTopic, Mqtt.ControlTopic);
                subscribed = true;
                Debug.Log($"SpeedListener subscribed {Mqtt.SpeedTopic} and {Mqtt.ControlTopic}.");
            }else if (!Mqtt.Instance.IsConnected && subscribed){
                subscribed = false;
            }
        }

        float currentSpeed;
        float currentTurn;
        bool isBraking;

        lock (_stateLock){
            currentSpeed = _pendingSpeed;
            currentTurn = _pendingTurn;
            isBraking = _pendingBrake;
        }

        speed = isBraking ? 0.0f : currentSpeed;


        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        if (currentTurn < 0.0f){
            transform.Rotate(0.0f, -turnSpeed * Time.deltaTime, 0.0f);
        }else if (currentTurn > 0.0f){
            transform.Rotate(0.0f, turnSpeed * Time.deltaTime, 0.0f);
        }
    }

    private void UnsubscribeIfNeeded(){
        if (subscribed && Mqtt.Instance != null){
            Mqtt.Instance.Unsubscribe(OnMessage);
            subscribed = false;
        }
    }

    private void OnDisable(){
        UnsubscribeIfNeeded();
    }
    private void OnDestroy(){
        UnsubscribeIfNeeded();
    }

    public void OnMessage(object sender, MqttMsgPublishEventArgs e)
    {
        string topic = e.Topic;
        string msg = System.Text.Encoding.UTF8.GetString(e.Message);

        if (logMessages)
            Debug.Log($"MQTT received | Topic: {topic} | Message: {msg}");

        if (topic == Mqtt.SpeedTopic || topic == Mqtt.ControlTopic)
        {
            TryReadControl(msg);
        }
    }

    private void TryReadControl(string msg)
    {
        try
        {
            bool hasSpeed = HasField(msg, "speed");
            bool hasTurn = HasField(msg, "turn");
            bool hasBrake = HasField(msg, "brake");

            float parsedSpeed = hasSpeed ? ReadFloatField(msg, "speed") : 0f;
            float parsedTurn = hasTurn ? ReadFloatField(msg, "turn") : 0f;
            bool parsedBrake = hasBrake && ReadBoolField(msg, "brake");

            float currentSpeed;
            float currentTurn;
            bool currentBrake;

            lock (_stateLock)
            {
                if (hasSpeed)
                    _pendingSpeed = parsedSpeed;
                if (hasTurn)
                    _pendingTurn = parsedTurn;
                if (hasBrake)
                    _pendingBrake = parsedBrake;
                if (_pendingBrake)
                    _pendingSpeed = 0f;
                currentSpeed = _pendingSpeed;
                currentTurn = _pendingTurn;
                currentBrake = _pendingBrake;
            }

            if (logMessages){
                Debug.Log($"Parsed control | speed: {currentSpeed}, turn: {currentTurn}, brake: {currentBrake}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Control parse error: " + ex.Message);
        }
    }

    private bool HasField(string msg, string fieldName){
        return msg.IndexOf($"'{fieldName}'") != -1 || msg.IndexOf($"\"{fieldName}\"") != -1;
    }

    private float ReadFloatField(string msg, string fieldName)
    {
        int keyIndex = msg.IndexOf($"'{fieldName}'");
        if (keyIndex == -1)
            keyIndex = msg.IndexOf($"\"{fieldName}\"");

        if (keyIndex == -1)
            return 0f;

        int colonIndex = msg.IndexOf(':', keyIndex);
        int endIndex = msg.IndexOfAny(new char[] { ',', '}' }, colonIndex + 1);

        if (colonIndex == -1 || endIndex == -1)
            return 0f;

        string rawValue = msg.Substring(colonIndex + 1, endIndex - colonIndex - 1).Trim().Trim('\'', '"');

        if (float.TryParse(rawValue, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float value))
        {
            return value;
        }

        return 0f;
    }

    private bool ReadBoolField(string msg, string fieldName)
    {
        int keyIndex = msg.IndexOf($"'{fieldName}'");
        if (keyIndex == -1)
            keyIndex = msg.IndexOf($"\"{fieldName}\"");

        if (keyIndex == -1)
            return false;

        int colonIndex = msg.IndexOf(':', keyIndex);
        int endIndex = msg.IndexOfAny(new char[] { ',', '}' }, colonIndex + 1);

        if (colonIndex == -1 || endIndex == -1)
            return false;

        string rawValue = msg.Substring(colonIndex + 1, endIndex - colonIndex - 1).Trim().Trim('\'', '"');

        return rawValue.Equals("true", StringComparison.OrdinalIgnoreCase);
    }
}