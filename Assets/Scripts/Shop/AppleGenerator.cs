using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleGenerator : MonoBehaviour
{
    //public GameObject apple; //Old code for testing

    //public Transform spawnPoint;  //Old code for testing

    public PlayerController playerController;

    private AppleCounter appleCounter;


    private void Start()
    {
        appleCounter = AppleCounter.GetInstance();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && IsPlayerInShopArea())
        {
            if (playerController.score > 0)
            {
                //Instantiate(apple, spawnPoint.position, Quaternion.identity); //Old code for testing
                
                appleCounter.IncrementApplesGenerated();
                Debug.Log("Apple bought!");
                playerController.DecrementScore(); 
            }
            else
            {
                Debug.Log("Not enough score!");
            }
        }
    }
  
    private bool IsPlayerInShopArea()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position, GetComponent<Collider>().bounds.size / 2, transform.rotation);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;

    }



}