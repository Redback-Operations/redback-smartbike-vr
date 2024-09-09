using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeCustomization : MonoBehaviour
{
    public GameObject[] Bikes;
    private GameObject _current;

    public void DisplayBike(int id)
    {
        // ensure there are bikes available to select from
        if (Bikes.Length == 0)
            return;

        // ensure the bike is available
        id = Math.Clamp(id, 0, Bikes.Length - 1);

        // loop through each bike available
        for (int index = 0; index < Bikes.Length; index++)
        {
            // update the active status based on id selected
            Bikes[index].SetActive(index == id);

            if (index == id)
                _current = Bikes[index];
        }
    }

    public void RestoreCustomization(string customization)
    {
        if (_current == null || string.IsNullOrEmpty(customization))
            return;

        var data = JsonUtility.FromJson<Customization>(customization);

        foreach (var item in data.CustomColors)
        {
            var target = FindTarget(_current, item.Name);

            if (target == null)
                continue;

            var modified = target.GetComponent<Renderer>();

            if (modified != null)
                modified.materials[item.Material].color = item.Color;
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
    public class Customization
    {
        public List<CustomColor> CustomColors;
    }

    [Serializable]
    public class CustomColor
    {
        public string Name;
        public int Material;
        public Color Color;
    }
}
