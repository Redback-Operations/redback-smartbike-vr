using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject CompleteLevelUI;
    public GameObject MissionFailUI;
    public void CompleteLevel()
    {
        Debug.Log("LEVEL WON!");
        CompleteLevelUI.SetActive(true);
    }
    public void Missionfail()
    {
        MissionFailUI.SetActive(true);
    }
}
