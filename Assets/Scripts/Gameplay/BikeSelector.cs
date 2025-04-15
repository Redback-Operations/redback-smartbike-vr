using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class BikeSelector : MonoBehaviour
{

    Bike[] bikes;
    private Bike _currentBike;
    public Bike CurrentBike => _currentBike;
    
    public void DisplayBike(int id)
    {
        if (bikes == null)
        {
            bikes = GetComponentsInChildren<Bike>(true);
        }
        // ensure there are bikes available to select from
        if (bikes.Length == 0)
            return;

        // ensure the bike is available
        id = Math.Clamp(id, 0, bikes.Length - 1);
        
        Debug.Log($"id:{id}");

        // loop through each bike available
        for (int index = 0; index < bikes.Length; index++)
        {
            // update the active status based on id selected
            bikes[index].gameObject.SetActive(index == id);

            if (index == id)
                _currentBike = bikes[index];
        }
    }
}
