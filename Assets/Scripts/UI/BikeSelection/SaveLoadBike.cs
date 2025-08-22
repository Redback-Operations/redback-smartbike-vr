using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class SaveLoadBike : MonoBehaviour
{
    [FormerlySerializedAs("Customization")] public BikeSelector selector;
    public UnityAction<Bike> onBikeSelected;
    public Bike CurrentBike => selector.CurrentBike;
    [SerializeField] private bool loadBikeDataAtStart;
    private int _currentSelected;
    
    void Start()
    {
        DisplayBike(PlayerPrefs.GetInt("SelectedBike",0));
        SelectBike();
    }
    public void DisplayBike(int id)
    {
        _currentSelected = id;
        selector.DisplayBike(id);
        
        if(loadBikeDataAtStart)
            LoadBikeData(id);
    }

    public void DisplayCurrentBike()
    {
        DisplayBike(_currentSelected);
    }
    public void SelectBike()
    {
        PlayerPrefs.SetInt("SelectedBike", _currentSelected);
        selector.DisplayBike(_currentSelected);
        selector.CurrentBike.OnBikeDataChange += SaveBikeData;
        onBikeSelected?.Invoke(selector.CurrentBike);
    }
    public void LoadBikeData(int id)
    {
        LoadBikeData(PlayerPrefs.GetString($"Bike_{id}"));
    }
    public void LoadBikeData(string data)
    {
        selector.CurrentBike.LoadBikeData(JsonUtility.FromJson<BikeData>(data));
    }
    public void SaveBikeData(BikeData bikeData)
    {
        var json = JsonUtility.ToJson(bikeData);
        Debug.Log($"Saving bikeData bike data:{json}");
        PlayerPrefs.SetString($"Bike_{_currentSelected}", json);
    }
}
