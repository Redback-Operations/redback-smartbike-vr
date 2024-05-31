using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
    This script is only for paved road 

    If you want to change all the colours at once then press play on unity and press the "C" button to change between colors
    and if you can add new colors into the color list 
    
    If you want to enable/disable the mask press the "M" button during play mode to do that or you could just change shader of the model to URP

*/
public class ChangeLineColour : MonoBehaviour
{
    public Material roadMaterial;
    private int currentidx = 0;
    
    List<Color> colorList = new List<Color>()
    {
        Color.red,
        Color.green,
        Color.yellow,
        Color.cyan,
        Color.white,
        Color.gray,
        Color.magenta,
        
    };

    void Start()
    {
        // checks if there is a road material attatched and the list has colours
        // then sets the original colour to yellow
        if (roadMaterial != null && colorList.Count > 0)
        {
            roadMaterial.SetColor("_LineColor", colorList[2]);
        }
    }

    public void ChangeLineColor()
    {
        // sets the colour according to the list of colours and changes the colour of the line and then increases index
        Color nextColor = colorList[currentidx];
        roadMaterial.SetColor("_LineColor", nextColor);
        currentidx = (currentidx + 1) % colorList.Count;
    }

    public void ToggleMask(bool enableMask)
    {
        if (roadMaterial!= null)
        {
            // Changes the mask value to be either 1 or 0
            float maskValue = enableMask ? 1.0f : 0.0f;
            roadMaterial.SetFloat("_UseMask", maskValue);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ChangeLineColor();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            // Toggle the current mask state
            float currentMaskValue = roadMaterial.GetFloat("_UseMask");
            ToggleMask(currentMaskValue == 0.0f);
        }
    }
}
