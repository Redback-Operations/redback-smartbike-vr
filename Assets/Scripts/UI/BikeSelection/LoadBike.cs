using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoadBike : MonoBehaviour
{
    public BikeCustomization Customization;
    private int _currentSelected;

    void Start()
    {
        // call the reset function
        ResetBike();
    }

    public void DisplayBike(int id)
    {
        _currentSelected = id;
        Customization.DisplayBike(id);
    }

    public void ResetBike()
    {
        // store the currently selected bike
        _currentSelected = PlayerPrefs.GetInt("SelectedBike");
        Debug.Log("Restored Bike: " + _currentSelected);

        // reset the bike to the current selected
        Customization.DisplayBike(_currentSelected);
        Customization.RestoreCustomization(PlayerPrefs.GetString($"Bike_{_currentSelected}"));
    }

    public void SelectBike()
    {
        // update the player preferences to the selected bike
        PlayerPrefs.SetInt("SelectedBike", _currentSelected);
        Debug.Log("Selected Bike: " + _currentSelected);
    }

    public void CustomizeBike(int index, string name, Color color)
    {
        Debug.Log($"Updating Bike {_currentSelected}, setting {name}[{index}] to {ColorUtility.ToHtmlStringRGB(color)}");

        var data = new BikeCustomization.Customization{ CustomColors = new List<BikeCustomization.CustomColor>() };
        var current = PlayerPrefs.GetString($"Bike_{_currentSelected}");

        if (!string.IsNullOrEmpty(current))
            data = JsonUtility.FromJson<BikeCustomization.Customization>(current);

        var target = data.CustomColors.FirstOrDefault(e => e.Material == index);

        if (target == null)
            data.CustomColors.Add(target = new BikeCustomization.CustomColor());

        target.Name = name;
        target.Material = index;
        target.Color = color;

        var json = JsonUtility.ToJson(data);

        PlayerPrefs.SetString($"Bike_{_currentSelected}", json);
        Debug.Log($"JSON is {json.Length} bytes");
    }
}
