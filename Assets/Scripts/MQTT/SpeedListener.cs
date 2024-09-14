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
    // Connect this to the MQTT object in the inspector
    // TODO: Do this via looking up the gameobject in the constructor instead
    public Mqtt mqtt;

    // The device ID of the bike - Used for validation
    public string deviceId;

    // Set to true once this client has subscribed
    public bool subscribed = false;

    public float speed = 0.0F;

    void Update()
    {
        // Once MQTT connects, subscribe for updates (if not already subscribed)
        if (mqtt.connected && !subscribed)
        {
            mqtt.Subscribe(OnMessage);
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
        if (topicTokens[0] != "" || topicTokens[1] != "bike" || topicTokens[2] != deviceId || topicTokens[3] != "speed")
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
