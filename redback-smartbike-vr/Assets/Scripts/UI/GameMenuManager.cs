using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class GameMenuManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject pausePanel;
    public GameObject optionsPanel;

    public InputActionProperty showButton;

    public float Distance;
    public Transform lookAtLocation;

    // Update is called once per frame
    void Update()
    {
        if (showButton.action.WasPressedThisFrame() && !optionsPanel.activeInHierarchy)
        {
            //Debug.Log("presseddd");
            pausePanel.SetActive(!pausePanel.activeSelf);
        }
        if (pausePanel.activeInHierarchy || optionsPanel.activeInHierarchy)
        {
            pauseMenu.transform.LookAt(new Vector3(lookAtLocation.position.x, pauseMenu.transform.position.y, lookAtLocation.position.z));
            pauseMenu.transform.position = lookAtLocation.position + new Vector3(lookAtLocation.forward.x, 0, lookAtLocation.forward.z).normalized * Distance;
        }
    }

    //Pause Panel Functionality
    public void Resume()
    {
        pausePanel.SetActive(!pausePanel.activeSelf);
    }
    public void Options()
    {
        //Debug.Log("load options");
        pausePanel.SetActive(!pausePanel.activeSelf);
        optionsPanel.SetActive(!optionsPanel.activeSelf);
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void Quit()
    {
        Application.Quit();
    }
    public AudioMixer audioMixer;

    //Options Panel Functionality
    public void SetVolume(float volume)
    {
        //Debug.Log("Volume: " + volume);
        audioMixer.SetFloat("volume", volume);
    }
    public void Back()
    {
        //Debug.Log("back");
        optionsPanel.SetActive(!optionsPanel.activeSelf);
        pausePanel.SetActive(!pausePanel.activeSelf);
    }
}
