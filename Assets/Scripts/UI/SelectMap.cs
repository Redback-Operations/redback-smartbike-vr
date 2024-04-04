using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectMap : MonoBehaviour
{

    [SerializeField] private Button desertButton;
    [SerializeField] private Button cityButton;


    // Start is called before the first frame update
    void Start()
    {
        desertButton.onClick.AddListener(desertClick);
        cityButton.onClick.AddListener(cityClick);
    }

    private void desertClick()
    {
        MapLoader.Load(MapLoader.Scene.MainScene); //using MapLoader to load a scene
    }
    private void cityClick()
    {
        MapLoader.Load(MapLoader.Scene.CityScene); //using MapLoader to load a scene
    }

}


