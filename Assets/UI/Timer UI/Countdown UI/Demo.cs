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

        timer1.SetDuration(timerDuration)
              .BeginWithDelay(startDelay);
    }
}