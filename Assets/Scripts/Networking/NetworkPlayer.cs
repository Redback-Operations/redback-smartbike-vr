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
    
    [Networked,OnChangedRender(nameof(BikeSelectionChanged))]
    public int BikeSelection {get; set;}
    
    [Networked, Capacity(1024),OnChangedRender(nameof(BikeCustomizationChanged))]
    public string BikeCustomization { get; set; }

    private PlayerController _playerController;
    private void BikeSelectionChanged()
    {
        // Defensive guard (stopgap): selector isn't always assigned on every
        // player prefab variant. Log instead of crashing while the null-ref
        // ticket is investigated.
        if (selector == null)
        {
            Debug.LogWarning($"[NetworkPlayer] selector is null on {name}; cannot display bike {BikeSelection}.");
            return;
        }
        selector.DisplayBike(BikeSelection);
    }

    public void BikeCustomizationChanged()
    {
        if (SaveLoadBike == null)
        {
            Debug.LogWarning($"[NetworkPlayer] SaveLoadBike is null on {name}; cannot load bike customization.");
            return;
        }
        SaveLoadBike.LoadBikeData(BikeCustomization);
    }

    public override void Spawned()
    {
        foreach (var localObject in localObjects)
        {
            localObject.SetActive(HasInputAuthority);
        }
        if (HasInputAuthority)
        {
            _playerController = gameObject.GetComponent<PlayerController>();
            ChangeBikeSelectionRpc( PlayerPrefs.GetInt("SelectedBike"));
            ChangeBikeCustomizationRpc( PlayerPrefs.GetString($"Bike_{BikeSelection}"));
        }
        else
        {
            BikeSelectionChanged();
            BikeCustomizationChanged();
        }
    }
    public override void FixedUpdateNetwork()
    {
        if (HasInputAuthority && _playerController!=null)
        {
            _playerController.Tick(Runner.DeltaTime);
        }
    }
    
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void ChangeBikeSelectionRpc(int selection)
    {
        BikeSelection = selection;
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void ChangeBikeCustomizationRpc(string customization)
    {
        BikeCustomization = customization;
    }
    
}
