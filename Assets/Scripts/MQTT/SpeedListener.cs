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