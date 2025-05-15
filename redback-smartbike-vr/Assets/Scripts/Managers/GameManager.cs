using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    private static GameManager instance; //Keep apple counter alive by DennisYu

    public GameObject CompleteLevelUI;
    public GameObject MissionFailUI;

    public void CompleteLevel()
    {

        Debug.Log("LEVEL WON!");
        CompleteLevelUI.SetActive(true);
    }

    public void missionfail()
    {

        MissionFailUI.SetActive(true);
    }



    private void Awake() //Keep apple counter alive by DennisYu
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static GameManager GetInstance() //Keep apple counter alive by DennisYu
    {
        return instance;
    }



}
