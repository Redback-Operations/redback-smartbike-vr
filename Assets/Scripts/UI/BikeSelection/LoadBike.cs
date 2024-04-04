using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadBike : MonoBehaviour
{
    public GameObject[] bikeSelected;
    public Transform spawnPoint;

    void Start()
    {
        int selectedBike = PlayerPrefs.GetInt("SelectedBike");// get chosen integer
        Debug.Log("selected Character: " + selectedBike);
        GameObject prefab = bikeSelected[selectedBike];//set it to the corresponding bike
        //only hide the ones that arent chosen
        for (int i = 0; i < bikeSelected.Length; i++)
        {
            if (i != selectedBike)
            {
                bikeSelected[i].SetActive(false);//hide the not chosen bikes
            }
        }
    }
}
