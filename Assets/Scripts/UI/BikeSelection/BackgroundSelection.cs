using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class BackgroundSelection : MonoBehaviour
{
    //backgrounds
    public GameObject[] backgrounds;
    public int currentBackgroundIndex = 0;

    // background names
    public TMP_Text currentBackground;
    public string[] backgroundNames;

    void Start()
    {
        // Show initial background
        showBackground(currentBackgroundIndex);
        updateBackgroundText(currentBackgroundIndex);
        backgrounds[(currentBackgroundIndex + 1) % backgrounds.Length].SetActive(false);
    }

    public void SwitchBackground()
    {
        // Deactivate the current background.
        backgrounds[currentBackgroundIndex].SetActive(false);

        // Increment the index or reset it to 0 if it goes out of bounds.
        currentBackgroundIndex = (currentBackgroundIndex + 1) % backgrounds.Length;

        //updates the background text to the new background
        updateBackgroundText(currentBackgroundIndex);

        // Activate the new background.
        showBackground(currentBackgroundIndex);
    }

    void showBackground(int index)
    {
        backgrounds[index].SetActive(true);
    }

    public void updateBackgroundText(int currentBackgroundIndex)
    {
        currentBackground.text = "Background: " + backgroundNames[currentBackgroundIndex];
    }
}
