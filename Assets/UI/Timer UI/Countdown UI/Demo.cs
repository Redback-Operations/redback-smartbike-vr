using UnityEngine;

public class Demo : MonoBehaviour
{
    [SerializeField] private Timer2 timer1;
    [SerializeField] private int timerDuration = 15;
    [SerializeField] private float startDelay = 3f;

    private void Start()
    {
        // Fallback: auto-find the timer if not assigned in the Inspector
        if (timer1 == null)
            timer1 = FindObjectOfType<Timer2>();

        if (timer1 == null)
        {
            Debug.LogError("Demo: No Timer2 found in the scene!");
            return;
        }

        timer1
            .SetDuration(timerDuration)
            .BeginWithDelay(startDelay);
    }
}

/*
 * Found this file in a mess when we picked the project up in T2 2026.
 * Couldn't find anything actually calling it, so I wrote a replacement above
 * rather than untangle it. Leaving the original here in case someone's
 * chasing a bug and needs to see what it used to do.
 * -DM
 *
 * public class Demo : MonoBehaviour
 * {
 *     [SerializeField] CountdownTimerUI timer1;
 *     private void Start()
 *     {
 *         timer1 = FindObjectOfType<CountdownTimerUI>();
 *         timer1.SetDuration(300).BeginWithDelay(3f);
 *     }
 * }
 */