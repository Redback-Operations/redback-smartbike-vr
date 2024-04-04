using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Random = UnityEngine.Random;

public class FakeSpeedPublisher : MonoBehaviour
{
    // Connect this to the MQTT object in the inspector
    // TODO: Do this via looking up the gameobject in the constructor instead
    public Mqtt mqtt;

    private float timeInterval = 0.0F;
    public float simulatedSpeed = 0.0F;
    public float publishPeriod = 1.0F;

    // Update is called once per frame
    // Roughly once every publishPeriod, publish a random speed value
    void Update()
    {
        timeInterval += Time.deltaTime;
        if (timeInterval >= publishPeriod)
        {
            var ts = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            // Randomly increase or decease the speed by up to 1 m/s, clamping it between the valid range
            float speed = Mathf.Clamp(simulatedSpeed + Random.Range(-1.0F, 1.0F), 0.0F, 30.0F);
            string payload = "{\"ts\": " + ts + ", \"speed\": " + speed + "}";
            mqtt.Publish(mqtt.speedTopic, payload);

            // Save the new speed for the next run of the simulation
            // and reset the publishing timer
            simulatedSpeed = speed;
            timeInterval = 0.0F;
        }
    }
}
