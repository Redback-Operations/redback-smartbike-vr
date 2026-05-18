using System;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;

public class SpeedListener : MonoBehaviour
{
    public bool subscribed = false;
    public float speed = 0.0f;
    public bool logMessages = true;

    void Update()
    {
        if (Mqtt.Instance == null)
            return;

        if (Mqtt.Instance.IsConnected && !subscribed)
        {
            Mqtt.Instance.Subscribe(OnMessage, Mqtt.ControlTopic);
            subscribed = true;
            Debug.Log("SpeedListener subscribed.");
        }

        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    public void OnMessage(object sender, MqttMsgPublishEventArgs e)
    {
        string topic = e.Topic;
        string msg = System.Text.Encoding.UTF8.GetString(e.Message);

        if (logMessages)
            Debug.Log($"MQTT received | Topic: {topic} | Message: {msg}");

        if (topic == Mqtt.ControlTopic)
        {
            TryReadControl(msg);
        }
    }

    private void TryReadControl(string msg)
    {
        try
        {
            speed = ReadFloatField(msg, "speed");
            float turn = ReadFloatField(msg, "turn");
            bool brake = ReadBoolField(msg, "brake");

            Debug.Log($"Parsed control | speed: {speed}, turn: {turn}, brake: {brake}");

            if (brake)
                speed = 0f;

            if (turn < 0)
                transform.Rotate(0f, -60f * Time.deltaTime, 0f);
            else if (turn > 0)
                transform.Rotate(0f, 60f * Time.deltaTime, 0f);
        }
        catch (Exception ex)
        {
            Debug.LogError("Control parse error: " + ex.Message);
        }
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

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography.X509Certificates;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt;
using System.Net.Security;
using System;

public class SpeedListener : MonoBehaviour
{
    // Set to true once this client has subscribed
    public bool subscribed = false;

    public float speed = 0.0F;

    void Update()
    {
        // Once MQTT connects, subscribe for updates (if not already subscribed)
        if (Mqtt.Instance.IsConnected && !subscribed)
        {
            Mqtt.Instance.Subscribe(OnMessage);
            subscribed = true;
        }

        // TODO: Each frame, do something with speed
        // like update the transform of the avatar
        transform.Translate(transform.right * speed * Time.deltaTime);
    }

    // Process the messages to retrieve the current speed
    public void OnMessage(object sender, MqttMsgPublishEventArgs e)
    {
        // Return if this is not the message we are interested in
        String[] topicTokens = e.Topic.Split('/');
        if (topicTokens[0] != "" || topicTokens[1] != "bike" || topicTokens[2] != Mqtt.Instance.ConnectionID || topicTokens[3] != "speed")
            return;

        // Parse the JSON payload
        // TODO: Use JSON.Net https://assetstore.unity.com/packages/tools/input-management/json-net-for-unity-11347
        string msg = System.Text.Encoding.UTF8.GetString(e.Message);
        int start = msg.IndexOf("{") + 1;
        int end = msg.LastIndexOf("}");
        string contents = msg.Substring(start, end - start);
        String[] messageTokens = contents.Split(',');
        // Find the token containing speed
        foreach (String msgToken in messageTokens)
        {
            if (msgToken.Contains("\"speed\"")) {
                String[] parts = msgToken.Split(':');
                // Save the current speed for later use in Update
                speed = float.Parse(parts[1]);
                //Debug.Log("SPEED of bike " + topicTokens[2] + " is " + speed);
            }
        }
    }
}
*/

