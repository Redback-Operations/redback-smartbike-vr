using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuButtons : MonoBehaviour
{
    /*
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    */

    //to load map selection
    public void PlayGame()
    {
        //MapLoader.Load(MapLoader.Scene.LevelSelection);
        //MapLoader.Load(MapLoader.Scene.SceneSelect);
        SceneManager.LoadScene("LevelSelection");
    }

    public void GoToBikeSelection()
    {
        SceneManager.LoadScene("BikeSelectScene");
    }


    public void QuitGame()
    {
        Application.Quit();
    }

    public void GoToMainMenu() {
        SceneManager.LoadScene("MainMenu");
    }

    public void GoToSceneSelect() {
        SceneManager.LoadScene("SceneSelect");
    }

    public void GoToLevelSelection() {
        SceneManager.LoadScene("LevelSelection");
    }
}
