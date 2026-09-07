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
            speed = MqttFieldParser.ReadFloat(msg, "speed");
            float turn = MqttFieldParser.ReadFloat(msg, "turn");
            bool brake = MqttFieldParser.ReadBool(msg, "brake");

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

}