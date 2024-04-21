using System;
using System.Collections;
using System.Collections.Generic;
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
}
