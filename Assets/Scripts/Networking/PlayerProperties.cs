using Fusion;
using UnityEngine;

public class PlayerProperties
{
    public Vector3 Position;
    public Quaternion Rotation;

    public Vector3 HeadLocalPosition;
    public Quaternion HeadLocalRotation;

    public Vector3 HandLeftLocalPosition;
    public Quaternion HandLeftLocalRotation;

    public Vector3 HandRightLocalPosition;
    public Quaternion HandRightLocalRotation;

    public int BikeSelection;
    public string BikeCustomization;

    public PlayerProperties() : this(Vector3.zero, Quaternion.identity) { }
    public PlayerProperties(Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;

        HeadLocalPosition = Vector3.zero;
        HeadLocalRotation = Quaternion.identity;

        HandLeftLocalPosition = Vector3.zero;
        HandLeftLocalRotation = Quaternion.identity;

        HandRightLocalPosition = Vector3.zero;
        HandRightLocalRotation = Quaternion.identity;

        BikeSelection = 0;
        BikeCustomization = "{}";
    }
}
