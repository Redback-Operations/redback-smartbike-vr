using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Timer2 : MonoBehaviour
{
    [Header("Timer UI references :")]
    [SerializeField] private Image uiFillImage; // Reference to the UI image that fills the timer
    [SerializeField] private Text uiText; // Reference to the text that shows time remaining
    [SerializeField] private GameObject timerUIParent; // Parent object to hide/show the timer UI


    public int Duration { get; private set; } // Property to store the total duration of the timer
    private int remainingDuration; // Internal variable to track the remaining time

    private void Start()
    {
        // Initialise the timer UI with default values (00:00 and full fill)
        uiText.text = "00:00";
        uiFillImage.fillAmount = 1f; // Starts with the fill circle fully filled
    }

    private void ResetTimer()
    {
        // Resets the timer to the initial state (00:00 and full fill)
        uiText.text = "00:00";
        uiFillImage.fillAmount = 1f; // Keeps the fill circle fully filled when resetting

        Duration = remainingDuration = 0; // Resets both duration and remaining time to 0
    }

    public Timer2 SetDuration(int seconds)
    {
        // Sets the timer duration and updates the UI immediately
        Duration = remainingDuration = seconds;
        UpdateUI(remainingDuration); // Shows the full duration right away
        return this;
    }

    public void BeginWithDelay(float delaySeconds)
    {
        // Starts the timer after a delay
        StartCoroutine(BeginAfterDelay(delaySeconds));
    }

    private IEnumerator BeginAfterDelay(float delaySeconds)
    {
        if (timerUIParent != null)
            timerUIParent.SetActive(false); // Hide timer UI during the delay

        yield return new WaitForSeconds(delaySeconds); // Wait for the delay

        if (timerUIParent != null)
            timerUIParent.SetActive(true); // Show timer UI after the delay

        Begin(); // Start the countdown
    }

    public void Begin()
    {
        // Starts the countdown by stopping any ongoing coroutines and starting a new one
        StopAllCoroutines(); 
        StartCoroutine(UpdateTimer()); // Starts the timer countdown coroutine
    }

    private IEnumerator UpdateTimer()
    {
        // Countdown loop that runs once per second until the timer reaches 0
        while (remainingDuration > 0)
        {
            yield return new WaitForSeconds(1f); // Waits for 1 second
            remainingDuration--; // Decrements the remaining time by 1 second
            UpdateUI(remainingDuration); // Updates the UI after decrementing
        }

        End(); // When the timer reaches 0, it will reset the timer
    }

    private void UpdateUI(int seconds)
    {
        // Updates the timer text and fill amount in the UI
        uiText.text = string.Format("{0:D2}:{1:D2}", seconds / 60, seconds % 60); // Formats the time as MM:SS
        uiFillImage.fillAmount = Mathf.InverseLerp(0, Duration, seconds); // Adjusts the fill amount based on remaining time
    }

    public void End()
    {
        // Resets the timer when the countdown ends
        ResetTimer();
    }

    private void OnDestroy()
    {
        // Stops all coroutines if the object is destroyed
        StopAllCoroutines();
    }
}
