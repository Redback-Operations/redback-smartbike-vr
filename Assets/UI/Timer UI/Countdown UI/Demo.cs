

using UnityEngine;

public class Demo : MonoBehaviour
{
<<<<<<< Updated upstream
    [SerializeField] Timer2 timer1;
    [SerializeField] int timerDuration = 15;
    [SerializeField] float startDelay = 3f;

    private void Start()
    {
        timer1 = FindObjectOfType<Timer2>();
        timer1.SetDuration(timerDuration).BeginWithDelay(startDelay);
    }
}


/* This script was in this state upon reception of the project in T2 2026. I can't find any uses of this script, but I've made a 'replacement' anyway. If anyone else finds themselves here the past script is here for your info if you're trying to fix something
-DM
using UnityEngine;
public class Demo : MonoBehaviour
{
    [SerializeField] CountdownTimerUI timer1;

    private void Start()
    {
        timer1 = FindObjectOfType<CountdownTimerUI>();
        timer1.SetDuration(300).BeginWithDelay(3f);
=======
>>>>>>> Stashed changes
    [SerializeField] private Timer2 timer1;

    [SerializeField] private int timerDuration = 15;
    [SerializeField] private float startDelay = 3f;
    }

    private void Start()
    {
        if (countdownTimer == null)
        {
            countdownTimer = FindObjectOfType<CountdownTimerUI>();
        }

        if (countdownTimer != null)
        {
            countdownTimer
                .SetDuration(300)
                .BeginWithDelay(3f);
        }

        if (timer == null)
        {
            timer = FindObjectOfType<Timer2>();
        }

        if (timer != null)
        {
            timer
                .SetDuration(timerDuration)
                .BeginWithDelay(startDelay);
        }
    }
} */