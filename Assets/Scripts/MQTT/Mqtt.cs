using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography.X509Certificates;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt;
using System.Net.Security;
using System;
using UnityEditor;

public class Mqtt : MonoBehaviour
{
    // ensure the credentials are NEVER CHECKED INTO THE REPOSITORY
    public string MqttHostname = "localhost";
    public int MqttPort = 1883;
    public string MqttUsername = "";
    public string MqttPassword = "";
    public bool AutoConnect = false;

    // Device ID of the Bike being connected to
    public static string DeviceId = "000001";

    // Send commands to these topics to change the experience on the bike
    public static string ControlTopic => $"bike/{DeviceId}/control";
    public static string ResistanceTopic => $"bike/{DeviceId}/resistance";
    public static string InclineTopic => $"bike/{DeviceId}/incline/control";
    public static string FanTopic => $"bike/{DeviceId}/fan";
    // Subscribe to these topics to receive information from the bike/cyclist
    public static string HeartRateTopic => $"bike/{DeviceId}/heartrate";
    public static string CadenceTopic => $"bike/{DeviceId}/cadence";
    public static string SpeedTopic => $"bike/{DeviceId}/speed";
    public static string PowerTopic => $"bike/{DeviceId}/power";

    public string WildcardTopic => $"bike/{DeviceId}/#";

    public static string LeftTurnTopic => $"Turn/Left";
    public static string RightTurnTopic => $"Turn/Right";

    public string ConnectionID => Guid.NewGuid().ToString();

    private static Mqtt _instance;
    public static Mqtt Instance => _instance;

    private MqttClient _client;

    private bool _connected;
    public bool IsConnected => _connected;

    void Start()
    {
        // if this is the first one, make it a singleton accessible anywhere
        if (_instance == null)
        {
            _instance = this;

            // DontDestroyOnLoad only works on root GameObjects. The MQTT component
            // is root in the persistent Loading/City scene objects (where we do want
            // the connection to survive scene loads), but it is also present on a
            // non-root child inside Player_New. Guard the call so we only invoke it
            // where it can actually take effect, instead of letting Unity log a
            // "DontDestroyOnLoad only works for root GameObjects" warning for the
            // child case (where the call was always a no-op anyway).
            if (transform.parent == null)
                DontDestroyOnLoad(gameObject);
        }

        if (!string.IsNullOrWhiteSpace(PlayerPrefs.GetString("MQTTHost")))
            MqttHostname = PlayerPrefs.GetString("MQTTHost");
        if (!string.IsNullOrWhiteSpace(PlayerPrefs.GetString("MQTTUsername")))
            MqttUsername = PlayerPrefs.GetString("MQTTUsername");
        if (!string.IsNullOrWhiteSpace(PlayerPrefs.GetString("MQTTPassword")))
            MqttPassword = PlayerPrefs.GetString("MQTTPassword");

        _client = new MqttClient(MqttHostname, MqttPort, false, null, null, MqttSslProtocols.None);
        _connected = false;

        if (AutoConnect)
            Connect();
    }

    // connection system to connect to this instance
    public bool Connect()
    {
        try
        {
            Debug.Log($"Trying to connect to {MqttHostname}:{MqttPort}");

            if (string.IsNullOrWhiteSpace(MqttUsername))
                _client.Connect(ConnectionID);
            else
                _client.Connect(ConnectionID, MqttUsername, MqttPassword);

            _connected = _client.IsConnected;
            Debug.Log("Connection successful: " + _connected);
        }
        catch (Exception e)
        {
            Debug.LogError("Connection error: " + e);
            _connected = false;
        }

        return _connected;
    }

    // subscribe to the following events with the handler callback, passing no subscriptions will subscribe to the wildcard topic
    public void Subscribe(MqttClient.MqttMsgPublishEventHandler handler, params string[] subscriptions)
    {
        if (subscriptions.Length == 0)
            subscriptions = new[] { WildcardTopic };

        _client.MqttMsgPublishReceived += handler;

        byte[] qosLevels = new byte[subscriptions.Length];
        for (int i = 0; i < subscriptions.Length; i++)
        {
            qosLevels[i] = MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE;
        }

        _client.Subscribe(subscriptions, qosLevels);
        Debug.Log($"Subscribed to messages: {string.Join(", ", subscriptions)}");
    }

    public void Unsubscribe(MqttClient.MqttMsgPublishEventHandler handler)
    {
        _client.MqttMsgPublishReceived -= handler;
        Debug.Log("Unsubscribed from messages");
    }

    // Send a message to the broker on a certain topic
    // Topics for the bike are provided as public member variables
    // The message is in JSON format and should include a timestamp (seconds since 1/1/70 UTC)
    //
    // Payload for resistance: {"ts": 176854940, "resistance": 24} 
    // The value for resistance should be an integer between 0 and 100, and is percentage of the maximum
    // Values around 24 seem good for cycling with a light resistance (otherwise the pedals feel too easy)
    // and 100 is the maximum resistance.
    //
    // Payload for incline: {"ts": 176854940, "incline": 0.0)
    // The value for incline should be a float between -10 and +19 (in steps of 0.5)
    // and represents the angle the front wheel should be raised. Use 0 to have the bike flat.
    //
    // Payload for fan: ("ts": 17685940, "fan": 100)
    // The value for fan should be an integer between 0 and 100 and is percentage of the maximum
    // 0 is no wind
    // 100 is winds that feel similar to riding at 54 km/hr
    //
    // Since this is used to send commands, QOS is set to provide a guarantee tha the message will be received,
    // and that it will not appear duplicate times. This incurs a 2 RTT overhead.
    public void Publish(string topic, string msg)
    {
        _client.Publish(topic, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
    }
}
