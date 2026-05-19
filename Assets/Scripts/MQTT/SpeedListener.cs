using System;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;
using static UnityEngine.GraphicsBuffer;

public class SpeedListener : MonoBehaviour
{
    public bool subscribed = false;

    /*
    MQTT speed 25.0 = bike target speed 25.0
    MQTT turn -1 = full left
    MQTT turn 0 = straight
    MQTT turn 1 = full right
    brake true = target speed 0
    */

    public float speed = 0.0f; //0 to 25
    public float turn = 0.0f;// -1 to 1
    public bool brake = false;

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

        // DO NOT move the bike here.
        // No transform.Translate here.
        // No transform.Rotate here.
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
            turn = ReadFloatField(msg, "turn");
            brake = ReadBoolField(msg, "brake");

            if (brake)
                speed = 0f;

            Debug.Log($"Parsed control | speed: {speed}, turn: {turn}, brake: {brake}");
        }
        catch (Exception ex)
        {
            Debug.LogError("Control parse error: " + ex.Message);
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

    public Vector2 GetInput()
    {
        float finalSpeed = brake ? 0f : speed;

        // Convert raw MQTT 0–25 into input 0–1
        float normalizedSpeed = Mathf.Clamp01(finalSpeed / 25f);

        Debug.Log($"MQTT INPUT | Raw Speed: {speed} | Normalized: {normalizedSpeed} | Turn: {turn} | Brake: {brake}");

        return new Vector2(turn, normalizedSpeed);
    }
}