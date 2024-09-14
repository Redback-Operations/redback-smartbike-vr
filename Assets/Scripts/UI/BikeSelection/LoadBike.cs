using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoadBike : MonoBehaviour
{
    public GameObject[] bikeSelected;
    private int _currentSelected;

    void Start()
    {
        // call the reset function
        ResetBike();
    }

    public void DisplayBike(int id)
    {
        // ensure there are bikes available to select from
        if (bikeSelected.Length == 0)
            return;

        // ensure the bike is available
        id = Math.Clamp(id, 0, bikeSelected.Length - 1);

        // loop through each bike available
        for (int index = 0; index < bikeSelected.Length; index++)
        {
            // update the active status based on id selected
            bikeSelected[index].SetActive(index == id);
        }

        _currentSelected = id;
        RestoreCustomization();
    }

    public void ResetBike()
    {
        // store the currently selected bike
        _currentSelected = PlayerPrefs.GetInt("SelectedBike");
        Debug.Log("selected Character: " + _currentSelected);
        // reset the bike to the current selected
        DisplayBike(_currentSelected);
    }

    public void SelectBike()
    {
        // update the player preferences to the selected bike
        PlayerPrefs.SetInt("SelectedBike", _currentSelected);
    }

    public void CustomizeBike(int index, string name, Color color)
    {
        Debug.Log($"Updating Bike {_currentSelected}, setting {name}[{index}] to {ColorUtility.ToHtmlStringRGB(color)}");

        var data = new BikeCustomization{ CustomColors = new List<BikeCustomColor>() };
        var current = PlayerPrefs.GetString($"Bike_{_currentSelected}");

        if (!string.IsNullOrEmpty(current))
            data = JsonUtility.FromJson<BikeCustomization>(current);

        var target = data.CustomColors.FirstOrDefault(e => e.Material == index);

        if (target == null)
            data.CustomColors.Add(target = new BikeCustomColor());

        target.Name = name;
        target.Material = index;
        target.Color = color;

        PlayerPrefs.SetString($"Bike_{_currentSelected}", JsonUtility.ToJson(data));
    }

    public void RestoreCustomization()
    {
        Debug.Log("Restoring Customization (if any)");

        var current = PlayerPrefs.GetString($"Bike_{_currentSelected}");

        if (string.IsNullOrEmpty(current))
            return;

        Debug.Log($" - Customization found, {current}");

        var data = JsonUtility.FromJson<BikeCustomization>(current);

        foreach (var item in data.CustomColors)
        {
            var target = FindTarget(bikeSelected[_currentSelected].gameObject, item.Name);

            if (target == null)
                continue;

            var renderer = target.GetComponent<Renderer>();

            if (renderer != null)
                renderer.materials[item.Material].color = item.Color;
        }
    }

    private GameObject FindTarget(GameObject current, string name)
    {
        if (current.name == name)
            return current;

        foreach (Transform child in current.transform)
        {
            var target = FindTarget(child.gameObject, name);

            if (target != null)
                return target;
        }

        return null;
    }

    [Serializable]
    public class BikeCustomization
    {
        public List<BikeCustomColor> CustomColors;
    }

    [Serializable]
    public class BikeCustomColor
    {
        public string Name;
        public int Material;
        public Color Color;
    }
}
