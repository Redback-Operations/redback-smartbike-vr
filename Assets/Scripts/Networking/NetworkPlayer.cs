using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject[] localObjects;
    [FormerlySerializedAs("Customization")] public BikeSelector selector;
    public SaveLoadBike SaveLoadBike;
    [Networked]
    public int PlayerID { get; set; } = 1;
    
    
    [Networked,OnChangedRender(nameof(BikeSelectionChanged))]
    public int BikeSelection {get; set;}
    
    [Networked,Capacity(1024),OnChangedRender(nameof(BikeCustomizationChanged))]
    public string BikeCustomization { get; set; }

    private void BikeSelectionChanged()
    {
        selector.DisplayBike(BikeSelection);
    }
    
    public void BikeCustomizationChanged()
    {
        SaveLoadBike.LoadBikeData(BikeCustomization);
    }

    private PlayerController _playerController;
    public override void Spawned()
    {
        foreach (var localObject in localObjects)
        {
            localObject.SetActive(HasInputAuthority);
        }
        if (HasInputAuthority)
        {
            _playerController = gameObject.GetComponent<PlayerController>();
            if(_playerController==null)
                _playerController = gameObject.AddComponent<PlayerController>();

        }

        if (HasInputAuthority)
        {
            BikeSelection = PlayerPrefs.GetInt("SelectedBike");
            BikeCustomization = PlayerPrefs.GetString($"Bike_{BikeSelection}");
        }
    }
    public override void FixedUpdateNetwork()
    {
        if (HasInputAuthority)
        {
            _playerController.Tick(Runner.DeltaTime);
        }
    }
    
}
