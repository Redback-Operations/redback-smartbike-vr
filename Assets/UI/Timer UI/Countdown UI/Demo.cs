using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour
{
    [SerializeField] Timer2 timer1;

    private void Start()
    {
        timer1 = FindObjectOfType<Timer2>(); // Automatically finds the first Timer2 in the scene
        timer1.SetDuration(300).BeginWithDelay(3f); // Sets the timer for a duration in seconds and starts it
    }

}
