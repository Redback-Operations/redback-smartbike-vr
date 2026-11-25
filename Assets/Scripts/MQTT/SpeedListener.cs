
using System;
using System.Globalization;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;

public class SpeedListener : MonoBehaviour
{
    public bool subscribed = false;
    public float speed = 0.0f;
    public bool logMessages = true;

    private PlayerController playerController;
    private Mqtt mqtt;

    private readonly object controlLock = new object();

    private float controlSpeed;
    private float controlTurn;
    private bool controlBrake;

    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
    }

    void Update()
    {
        // PlayerController handles movement when live
        // MQTT telemetry is active.
        if (playerController != null &&
            playerController.UsingLiveTelemetry)
        {
            speed = playerController.LiveSpeed;
            return;
        }

        if (Mqtt.Instance == null)
            return;

        if (Mqtt.Instance.IsConnected && !subscribed)
        {
            mqtt = Mqtt.Instance;

            mqtt.Subscribe(OnMessage, Mqtt.ControlTopic);

            subscribed = true;

            Debug.Log("SpeedListener subscribed.");
        }

        float turn;
        bool brake;

        lock (controlLock)
        {
            speed = controlSpeed;
            turn = controlTurn;
            brake = controlBrake;
        }

        if (brake || !Mqtt.Instance.IsConnected)
            speed = 0f;

        // Avoid independent movement when the existing
        // PlayerController has an active movement handler.
        if (playerController != null &&
            playerController.BikeMover != null)
        {
            return;
        }

        if (!Mqtt.Instance.IsConnected)
            return;

        transform.Translate(
            Vector3.forward * speed * Time.deltaTime
        );

        if (turn < 0f)
        {
            transform.Rotate(
                0f, -60f * Time.deltaTime, 0f
            );
        }
        else if (turn > 0f)
        {
            transform.Rotate(
                0f, 60f * Time.deltaTime, 0f
            );
        }
    }

    public void OnMessage(
        object sender,
        MqttMsgPublishEventArgs e)
    {
        string topic = e.Topic;

        if (topic != Mqtt.ControlTopic)
            return;

        string msg =
            System.Text.Encoding.UTF8.GetString(e.Message);

        if (logMessages)
        {
            Debug.Log(
                $"MQTT received | Topic: {topic} | Message: {msg}"
            );
        }

        TryReadControl(msg);
    }

    private void TryReadControl(string msg)
    {
        try
        {
            float newSpeed = ReadFloatField(msg, "speed");
            float newTurn = ReadFloatField(msg, "turn");
            bool newBrake = ReadBoolField(msg, "brake");

            if (float.IsNaN(newSpeed) ||
                float.IsInfinity(newSpeed) ||
                float.IsNaN(newTurn) ||
                float.IsInfinity(newTurn))
            {
                return;
            }

            lock (controlLock)
            {
                controlSpeed = newSpeed;
                controlTurn = newTurn;
                controlBrake = newBrake;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "Control parse error: " + ex.Message
            );
        }
    }

    private float ReadFloatField(
        string msg,
        string fieldName)
    {
        int keyIndex = msg.IndexOf($"'{fieldName}'");

        if (keyIndex == -1)
            keyIndex = msg.IndexOf($"\"{fieldName}\"");

        if (keyIndex == -1)
            return 0f;

        int colonIndex = msg.IndexOf(':', keyIndex);

        if (colonIndex == -1)
            return 0f;

        int endIndex = msg.IndexOfAny(
            new char[] { ',', '}' },
            colonIndex + 1
        );

        if (endIndex == -1)
            return 0f;

        string rawValue = msg.Substring(
            colonIndex + 1,
            endIndex - colonIndex - 1
        ).Trim().Trim('\'', '"');

        if (float.TryParse(
            rawValue,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out float value))
        {
            return value;
        }

        return 0f;
    }

    private bool ReadBoolField(
        string msg,
        string fieldName)
    {
        int keyIndex = msg.IndexOf($"'{fieldName}'");

        if (keyIndex == -1)
            keyIndex = msg.IndexOf($"\"{fieldName}\"");

        if (keyIndex == -1)
            return false;

        int colonIndex = msg.IndexOf(':', keyIndex);

        if (colonIndex == -1)
            return false;

        int endIndex = msg.IndexOfAny(
            new char[] { ',', '}' },
            colonIndex + 1
        );

        if (endIndex == -1)
            return false;

        string rawValue = msg.Substring(
            colonIndex + 1,
            endIndex - colonIndex - 1
        ).Trim().Trim('\'', '"');

        return rawValue.Equals(
            "true",
            StringComparison.OrdinalIgnoreCase
        );
    }

    private void OnDisable()
    {
        if (subscribed && mqtt != null)
        {
            mqtt.Unsubscribe(OnMessage);
        }

        subscribed = false;
        speed = 0f;

        lock (controlLock)
        {
            controlSpeed = 0f;
            controlTurn = 0f;
            controlBrake = false;
        }
    }
}
