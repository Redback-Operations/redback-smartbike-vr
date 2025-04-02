using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BikeSelection : MonoBehaviour
{
    //bikes
    public GameObject[] bikes;
    public int selectedBike = 0;

    //buttons
    //[SerializeField] private Button nextButton;
    //[SerializeField] private Button returnButton;
    //[SerializeField] private Button playButton;

    void Start()
    {
        //nextButton.onClick.AddListener(NextChar);
        //playButton.onClick.AddListener(LevelSelect);
        //returnButton.onClick.AddListener(ReturnMenu);

        //hide all bikes other than the first one
        for (int i = 1; i < bikes.Length; i++)
        {
            bikes[i].SetActive(false);
        }
    }

    public void NextChar()//go to next bike
    {
        bikes[selectedBike].SetActive(false);
        selectedBike = (selectedBike + 1) % bikes.Length;
        Debug.Log("next character, " + selectedBike);
        bikes[selectedBike].SetActive(true);
    }

    public void PrevChar()//goes to previous bike
    {
        bikes[selectedBike].SetActive(false);
        selectedBike--;
        Debug.Log("next character, " + selectedBike);
        if (selectedBike < 0)
        {
            selectedBike += bikes.Length;
        }
        bikes[selectedBike].SetActive(true);
    }

    //when select button is clicked
    public void LevelSelect()
    {
        PlayerPrefs.SetInt("SelectedBike", selectedBike);
        SceneManager.LoadScene("LevelSelection");
    }

}
