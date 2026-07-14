using UnityEngine;

public class BikeAchievementTracker : MonoBehaviour
{
    [SerializeField] private SpeedListener speedListener;

    private float totalDistance;
    private float highestSpeed;

    public float TotalDistance => totalDistance;
    public float HighestSpeed => highestSpeed;

    private void Update()
    {
        if (speedListener == null)
        {
            return;
        }

        float currentSpeed = Mathf.Abs(speedListener.speed);

        totalDistance += currentSpeed * Time.deltaTime;

        if (currentSpeed > highestSpeed)
        {
            highestSpeed = currentSpeed;
        }
    }
}