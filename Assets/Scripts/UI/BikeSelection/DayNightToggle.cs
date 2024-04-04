using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightToggle : MonoBehaviour
{
    public Light directionalLight; // Reference to your directional light
    public Material daySkybox;     // Reference to your day skybox material
    public Material nightSkybox;   // Reference to your night skybox material

    private bool isDay = true;     // Track whether it's currently day or night

    public void ToggleDayNight()
    {
        isDay = !isDay; // Toggle day/night

        // Update lighting and skybox based on the time of day
        if (isDay)
        {
            directionalLight.intensity = 1.0f; // Adjust light intensity for day
            RenderSettings.skybox = daySkybox;
        }
        else
        {
            directionalLight.intensity = 0.1f; // Adjust light intensity for night
            RenderSettings.skybox = nightSkybox;
        }
    }
}
