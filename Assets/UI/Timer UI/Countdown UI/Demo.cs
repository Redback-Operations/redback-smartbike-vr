using UnityEngine;

public class Demo : MonoBehaviour
{
    [SerializeField] private CountdownTimerUI countdownTimer;
    [SerializeField] private Timer2 timer;
    [SerializeField] private int timerDuration = 15;
    [SerializeField] private float startDelay = 3f;

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
}