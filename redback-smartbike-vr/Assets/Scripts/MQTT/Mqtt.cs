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
            // store the instance
            _instance = this;
            // ensure it isn't destroyed on scene change
            DontDestroyOnLoad(this);
        }

        // create the mqtt client ready for communication
        _client = new MqttClient(MqttHostname, MqttPort, true, null, null, MqttSslProtocols.TLSv1_2);
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
            _client.Connect(ConnectionID, MqttUsername, MqttPassword);
            _connected = true;

            Debug.Log(" - connection successful");
        }
        catch (Exception e)
        {
            Debug.LogError(" - connection error: " + e.Message);
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
        _client.Subscribe(subscriptions, new[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
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
