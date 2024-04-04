// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class CountdownTimer : MonoBehaviour
// {
//     public GameManager gameManager;
//     float currentTime = 0f;
//     float startingTime = 20f;
    
//     public Text CountdownText;
//     // Start is called before the first frame update
//     void Start()
//     {
//         currentTime = startingTime;
//     }

//     // Update is called once per frame
//     void Update()
//     {
//      currentTime -= 1 * Time.deltaTime;
//      CountdownText.text = currentTime.ToString("0");
//      if(currentTime <= 0)
//      {
//         gameManager.missionfail();

//      }
        
//     }
// }
