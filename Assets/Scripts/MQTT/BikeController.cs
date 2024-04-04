using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BikeController : MonoBehaviour
{
    // Set this to the Mqtt object
    public Mqtt mqtt;
    public bool dirty = true;

    private int resistance = 24;    // Set resistance to 24% (valid range 0 to 100)
    private float incline = 0.0f;   // Set incline to flat (valid range -10 to +19 in steps of 0.5 degrees)
    private int fan = 0;            // Set the fan to off (valid range 0 to 100) where 100 is equavlent to 54 km/hr winds

    // Call this function when you want to change the bike resistance, it will be published at the next update
    // Invalid values are ignored
    public void SetResistance(int value)
    {
        if (value < 0 || value > 100)
        {
            Debug.Log("Invalid resistance value " + value);
        }
        else
        {
            resistance = value;
            dirty = true;
        }
    }

    // Call this function when you want to change the bike incline, it will be published at the next update
    // Invalid values are ignored
    public void SetIncline(int value)
    {
        if (value < -10 || value > 19)
        {
            Debug.Log("Invalid incline value " + value);
        }
        else
        {
            incline = value;
            dirty = true;
        }
    }

    // Call this function when you want to change the fan speed, it will be published at the next update
    // Invalid values are ignored
    public void SetFan(int value)
    {
        if (value < 0 || value > 100) {
            Debug.Log("Invalid fan value " + value);
        } else
        {
            fan = value;
            dirty = true;
        }
    }

    // Update is called once per frame
    // After we are connected, if the bike needs updating, publish the new state
    void Update()
    {
        if (mqtt.connected && dirty)
        {
            var ts = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            mqtt.Publish(mqtt.resistanceTopic, "{\"ts\": " + ts + ", \"resistance\": " + resistance + "}");
            mqtt.Publish(mqtt.inclineTopic, "{\"ts\": " + ts + ", \"incline\": " + incline + "}");
            mqtt.Publish(mqtt.fanTopic, "{\"ts\": " + ts + ", \"fan\": " + fan + "}");
            dirty = false;
        }
    }
}
