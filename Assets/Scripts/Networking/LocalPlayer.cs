using Fusion;
using UnityEngine;
using UnityEngine.XR;

public class LocalPlayer : MonoBehaviour
{
    public PlayerProperties Properties { get; private set; }
    public GameObject RightHand;

    public int PlayerID { get; set; } = 1;
    public int BikeSelection { get; set; }
    public string BikeCustomization { get; set; }

    void Start()
    {
        Properties = new PlayerProperties
        {
            Position = Vector3.zero,
            Rotation = Quaternion.identity,
            HeadLocalPosition = Vector3.zero,
            HeadLocalRotation = Quaternion.identity,
            HandLeftLocalPosition = Vector3.zero,
            HandLeftLocalRotation = Quaternion.identity,
            HandRightLocalPosition = Vector3.zero,
            HandRightLocalRotation = Quaternion.identity
        };

        PlayerID = PlayerPrefs.GetInt("PlayerID");
    }

    void Update()
    {
        Properties.Position = transform.position;
        Properties.Rotation = transform.rotation;

        Properties.HeadLocalPosition = GetPosition(XRNode.Head);
        Properties.HeadLocalRotation = GetRotation(XRNode.Head);

        Properties.HandLeftLocalPosition = GetPosition(XRNode.LeftHand);
        Properties.HandLeftLocalRotation = GetRotation(XRNode.LeftHand);

        Properties.HandRightLocalPosition = GetPosition(XRNode.RightHand);
        Properties.HandRightLocalRotation = GetRotation(XRNode.RightHand);

        var bike = PlayerPrefs.GetInt("SelectedBike");
        var customisation = PlayerPrefs.GetString($"Bike_{bike}");

        BikeSelection = bike;
        BikeCustomization = customisation;
    }

    Vector3 GetPosition(XRNode node)
    {
        var device = InputDevices.GetDeviceAtXRNode(node);

        if (!device.isValid || !device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position))
            return Vector3.zero;

        return position;
    }

    Quaternion GetRotation(XRNode node)
    {
        var device = InputDevices.GetDeviceAtXRNode(node);

        if (!device.isValid || !device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
            return Quaternion.identity;

        return rotation;
    }
}
