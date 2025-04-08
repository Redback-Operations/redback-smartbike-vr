using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR;

public class NetworkPlayer : NetworkBehaviour
{
    public Transform Head;
    public Transform HandLeft;
    public Transform HandRight;

    [FormerlySerializedAs("Customization")] public BikeSelector selector;
    public SaveLoadBike SaveLoadBike;

    private bool _firstRun = true;

    [Networked]
    public int PlayerID { get; set; } = 1;

    [Networked]
    public bool Dirty { get; set; } = false;
    [Networked]
    public int BikeSelection { get; set; } = -1;
    [Networked]
    [Capacity(1024)]
    public string BikeCustomization { get; set; }

    private NetworkTransform _networkTransform;

    public bool IsLocalNetworkRig => Object.HasInputAuthority;

    void Start()
    {
        _networkTransform = GetComponent<NetworkTransform>();

        if (HasInputAuthority)
            gameObject.AddComponent<PlayerController>();
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        if (!GetInput<RigInput>(out var input))
            return;

        transform.position = input.playAreaPosition;
        transform.rotation = input.playAreaRotation;

        Head.localPosition = input.headsetPosition;
        Head.localRotation = input.headsetRotation;
        HandLeft.localPosition = input.leftHandPosition;
        HandLeft.localRotation = input.leftHandRotation;
        HandRight.localPosition = input.rightHandPosition;
        HandRight.localRotation = input.rightHandRotation;

        if (Dirty)
        {
            Debug.Log($"Updating to {BikeSelection} with {BikeCustomization}");

            selector.DisplayBike(BikeSelection);
            SaveLoadBike.LoadBikeData(BikeCustomization);
            
            Dirty = false;
        }

        if (!IsLocalNetworkRig)
            return;

        var bike = PlayerPrefs.GetInt("SelectedBike");
        var customisation = PlayerPrefs.GetString($"Bike_{bike}");

        if (!_firstRun && BikeSelection == bike)
            return;

        BikeSelection = bike;
        BikeCustomization = customisation;

        Dirty = true;
        _firstRun = false;
    }
}
