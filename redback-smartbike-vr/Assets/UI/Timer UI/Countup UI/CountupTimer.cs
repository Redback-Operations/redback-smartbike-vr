using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Timer1 : MonoBehaviour
{
    [Header("Timer UI references :")]
    [SerializeField] private Text uiText; // Reference to the UI text that shows the timer

    // References for clock hands
    [SerializeField] private Image minuteHand; // Reference to the minute hand UI element
    [SerializeField] private Image secondHand; // Reference to the second hand UI element

    private int elapsedTime; // Tracks the elapsed time in seconds

    private void Start()
    {
        ResetTimer1(); // Resets the timer UI and variables when the script starts
        Begin(); // Automatically starts the timer on start with a delay
    }

    private void ResetTimer1()
    {
        uiText.text = "00:00"; // Sets the initial timer display to 00:00
        minuteHand.transform.rotation = Quaternion.Euler(0, 0, 90); // Sets the minute hand to the 12 o'clock position
        secondHand.transform.rotation = Quaternion.Euler(0, 0, 90); // Sets the second hand to the 12 o'clock position
        elapsedTime = 0; // Resets the elapsed time
    }

    public void Begin()
    {
        StopAllCoroutines(); // Stops any running timer updates
        StartCoroutine(BeginWithDelay()); // Starts the timer with a delay
    }

    private IEnumerator BeginWithDelay()
    {
        yield return new WaitForSeconds(3f); // Waits for 3 seconds before starting the timer
        StartCoroutine(UpdateTimer1()); // Starts updating the timer after the delay
    }

    private IEnumerator UpdateTimer1()
    {
        while (true) // Continuously updates the timer
        {
            yield return new WaitForSeconds(1f); // Waits for 1 second between updates
            elapsedTime++; // Increments the elapsed time by 1 second
            UpdateUI(elapsedTime); // Updates the timer UI and clock hands
        }
    }

    private void UpdateUI(int seconds)
    {
        // Updates the time text in the format MM:SS
        uiText.text = string.Format("{0:D2}:{1:D2}", seconds / 60, seconds % 60);

        // Calculates the rotation for the second and minute hands
        float secondsRotation = (seconds % 60) * 6f; // 360 degrees / 60 seconds = 6 degrees per second
        float minutesRotation = (seconds / 60) * 6f; // 360 degrees / 60 minutes = 6 degrees per minute

        // Applies the calculated rotations to the second and minute hands
        secondHand.transform.rotation = Quaternion.Euler(0, 0, 90 - secondsRotation); // Rotate second hand from 12 o'clock
        minuteHand.transform.rotation = Quaternion.Euler(0, 0, 90 - minutesRotation); // Rotate minute hand from 12 o'clock
    }

    private void OnDestroy()
    {
        StopAllCoroutines(); // Ensure any running coroutines are stopped when the object is destroyed
    }
}
