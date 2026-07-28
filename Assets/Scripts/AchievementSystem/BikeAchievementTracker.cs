using UnityEngine;

namespace SmartBike.Achievements
{
    public class BikeAchievementTracker : MonoBehaviour
    {
        [Header("Existing Speed Source")]
        [SerializeField] private SpeedListener speedListener;

        [Header("Tracking Settings")]
        [Min(0f)]
        [SerializeField] private float minimumMovingSpeedKmh = 0.5f;

        [Min(0.1f)]
        [SerializeField] private float updateInterval = 1f;

        [Header("Debug")]
        [SerializeField] private bool useDebugSpeed;

        [Min(0f)]
        [SerializeField] private float debugSpeedKmh = 15f;

        private float currentSpeedKmh;
        private float updateTimer;
        private float accumulatedDistanceKm;
        private float accumulatedRideTimeSeconds;

        public float CurrentSpeedKmh => currentSpeedKmh;

        private void Awake()
        {
            if (speedListener == null)
            {
                speedListener = GetComponent<SpeedListener>();
            }
        }

        private void Update()
        {
            if (AchievementManager.Instance == null)
            {
                return;
            }

            if (useDebugSpeed)
            {
                currentSpeedKmh = debugSpeedKmh;
            }
            else if (speedListener != null)
            {
                currentSpeedKmh = Mathf.Max(0f, speedListener.speed);
            }
            else
            {
                return;
            }

            TrackBikeActivity(Time.deltaTime);
        }

        private void TrackBikeActivity(float deltaTime)
        {
            AchievementManager.Instance.SetProgress(
                AchievementType.MaximumSpeed,
                currentSpeedKmh);

            if (currentSpeedKmh < minimumMovingSpeedKmh)
            {
                return;
            }

            updateTimer += deltaTime;
            accumulatedRideTimeSeconds += deltaTime;

            float distanceThisFrameKm =
                currentSpeedKmh / 3600f * deltaTime;

            accumulatedDistanceKm += distanceThisFrameKm;

            if (updateTimer >= updateInterval)
            {
                SubmitAccumulatedProgress();
            }
        }

        private void SubmitAccumulatedProgress()
        {
            if (AchievementManager.Instance == null)
            {
                return;
            }

            if (accumulatedDistanceKm > 0f)
            {
                AchievementManager.Instance.AddProgress(
                    AchievementType.DistanceTravelled,
                    accumulatedDistanceKm);
            }

            if (accumulatedRideTimeSeconds > 0f)
            {
                AchievementManager.Instance.AddProgress(
                    AchievementType.RideTime,
                    accumulatedRideTimeSeconds);
            }

            updateTimer = 0f;
            accumulatedDistanceKm = 0f;
            accumulatedRideTimeSeconds = 0f;
        }

        public void RegisterCheckpoint()
        {
            if (AchievementManager.Instance == null)
            {
                return;
            }

            AchievementManager.Instance.AddProgress(
                AchievementType.CheckpointsReached,
                1f);
        }

        public void RegisterRideCompleted()
        {
            SubmitAccumulatedProgress();

            if (AchievementManager.Instance == null)
            {
                return;
            }

            AchievementManager.Instance.AddProgress(
                AchievementType.RidesCompleted,
                1f);
        }

        private void OnDisable()
        {
            SubmitAccumulatedProgress();
        }
    }
}