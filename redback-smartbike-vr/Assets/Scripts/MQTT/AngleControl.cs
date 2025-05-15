using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleControl : MonoBehaviour
{
    //Declaring the game object that has the collider that will be used for calculations attatched to it.
    public GameObject Player;

    //Declaring the two angles which will be used. InGameAngle is the calculated angle of the in game avatar.
    //BikeAngle is the angle that will be sent to the IoT stuff for the actual bike, which has to be clamped to a range between -10 and +30.
    private float InGameAngle;
    public float BikeAngle;

    //The numbers that the angle will be divided by dpending on whether it is positive or negative.
    public float positiveModifier = 2;
    public float negativeModifier = 3;

    //The minimum and maximum of the angle the bike can take.
    public float maximumAngle = 19;
    public float minimumAngle = -10;

    //Intervals that will be used for the when the code updates the bike angle.
    //Will be updating mqtt.inclineTopic as that is the portion of the Mqtt that handles the bike's incline.
    private float timeInterval = 0.0f;
    public float publishPeriod = 1.0f;

    // Update is called once per frame
    void Update()
    {
        if (Mqtt.Instance == null)
            return;

        //This gets the GameObject's forward position, while taking angle into account.
        //This Vector3 is normalised.
        Vector3 playerVector = Player.transform.forward;

        //This gets the x rotation of the GameObject (Player)
        Quaternion rotation = Player.transform.localRotation;
        float InGameAngle = rotation.eulerAngles.x;

        // Normalize the angle to range from -180 to 180 degrees
        if (InGameAngle > 180)
        {
            InGameAngle -= 360;
        }
        else if (InGameAngle < -180)
        {
            InGameAngle += 360;
        }
        InGameAngle = -InGameAngle;

        BikeAngle = RefineAngle(InGameAngle);

        timeInterval += Time.deltaTime;
        if (timeInterval >= publishPeriod)
        {
            var ts = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            //Clamps the data sent to the bike to within the valid range.
            float incline = Mathf.Clamp(BikeAngle, minimumAngle, maximumAngle);
            string payload = "{\"ts\": " + ts + ", \"incline\": " + incline + "}";

            Mqtt.Instance.Publish(Mqtt.InclineTopic, payload);

            BikeAngle = incline;
            timeInterval = 0.0f;
        }
    }

    private float RefineAngle(float RawAngle)
    {
        float Refined;

        if (RawAngle < 0)
        {
            //Divides negative angles by 3, then round to the nearest 0.5
            Refined = RawAngle / negativeModifier;
            Refined = MathF.Round(Refined * 2) / 2;
        }
        else if (RawAngle > 0)
        {
            //Divides positive angles by 2, then round to the nearest 0.5
            Refined = RawAngle / positiveModifier;
            Refined = MathF.Round(Refined * 2) / 2;
        }
        else
        {
            Refined = 0.0f;
        }
        return Refined;
    }
}
