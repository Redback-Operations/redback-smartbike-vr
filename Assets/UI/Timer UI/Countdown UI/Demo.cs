using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour
{
    [SerializeField] CountdownTimerUI timer1;

    private void Start()
    {
        timer1 = FindObjectOfType<CountdownTimerUI>();
        timer1.SetDuration(300).BeginWithDelay(3f);
    }
}