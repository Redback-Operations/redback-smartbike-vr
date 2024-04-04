using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography.X509Certificates;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt;
using System.Net.Security;
using System;

public class Mqtt : MonoBehaviour
{
    // TODO: Work out how to set these variables to the appropriate values but
    // ensure the credentials are NEVER CHECKED INTO THE REPOSITORY
    public string mqttHostname = "localhost";
    public int mqttPort = 1883;
    public string mqttUsername = "";
    public string mqttPassword = "";

    // The deviceId should be set to the id of the bike, and then not changed at runtime
    // While the new topic names will be returned, the client will still be subscribed to
    // the topics for the previous bike
    public string deviceId = "";

    // Connect the hivemq.certificate.txt file to this in the Inspector
    public TextAsset certificate;

    // Send commands to these topics to change the experience on the bike
    public string resistanceTopic { get { return "/bike/" + deviceId + "/resistance"; } }
    public string inclineTopic { get { return "/bike/" + deviceId + "/incline"; } }
    public string fanTopic { get { return "/bike/" + deviceId + "/fan"; } }

    // Subscribe to these topics to receive information from sensors on the bike/cyclist
    public string heartrateTopic { get { return "/bike/" + deviceId + "/heartrate"; } }
    public string cadenceTopic { get { return "/bike/" + deviceId + "/cadence"; } }
    public string speedTopic { get { return "/bike/" + deviceId + "/speed"; } }
    public string powerTopic { get { return "/power/" + deviceId + "/power"; } }

    // Set to true when connection is established. Don't subscribe before this occurs
    public bool connected = false;

    // Wildcard topic to receive all messages for this bike
    public string wildcardTopic { get { return "/bike/" + deviceId + "/#"; } }

    // Private objects and variables
    private MqttClient client;

    // Start is called before the first frame update
    void Start()
    {
        Connect();
        //client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

        // To log all received messages to the console for debugging use the following line
        Subscribe(client_MqttMsgPublishReceived);

        // Subscribe to the wildcard topic to receive all messages for this bike
        // If you don't need all messages, you can subscribe just to the topic(s) required
        byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE };
        client.Subscribe(new string[] { wildcardTopic }, qosLevels);
        //Debug.Log("Subscribed to " + wildcardTopic);
    }

    private void Connect()
    {
        X509Certificate cert = new X509Certificate();
        cert.Import(certificate.bytes);

        client = new MqttClient(mqttHostname, mqttPort, true, cert, null, MqttSslProtocols.TLSv1_2);
        string clientId = Guid.NewGuid().ToString();
        try
        {
            client.Connect(clientId, mqttUsername, mqttPassword);
            connected = true;
            //Debug.Log("Connected to " + mqttHostname + ":" + mqttPort);
        }
        catch (Exception e)
        {
            Debug.LogError("Connection error: " + e);
        }
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
        client.Publish(topic, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        //Debug.Log("Published: topic " + topic + " msg " + msg);
    }

    // Add a delegate to receive notifications for this bike
    public void Subscribe(uPLibrary.Networking.M2Mqtt.MqttClient.MqttMsgPublishEventHandler callback) {
        client.MqttMsgPublishReceived += callback;
    }

    // Remove the delegate to stop receiving notifications for this bike
    public void Unsubscribe(uPLibrary.Networking.M2Mqtt.MqttClient.MqttMsgPublishEventHandler callback) {
        client.MqttMsgPublishReceived -= callback;
    }

    // An example of the callback function would look like
    public void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string msg = System.Text.Encoding.UTF8.GetString(e.Message);
        Debug.Log("Received message from " + e.Topic + " : " + msg);
    }
}
