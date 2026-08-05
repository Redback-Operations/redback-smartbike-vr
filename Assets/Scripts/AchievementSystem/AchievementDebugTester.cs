using UnityEngine;

namespace SmartBike.Achievements
{
    public class AchievementDebugTester : MonoBehaviour
    {
        [Header("Test Values")]

        [Min(0f)]
        [SerializeField]
        private float distanceToAddKm = 1f;

        [Min(0f)]
        [SerializeField]
        private float speedToSetKmh = 20f;

        [Min(0f)]
        [SerializeField]
        private float rideTimeToAddSeconds = 60f;

        [ContextMenu("Test/Add Distance")]
        public void AddDistance()
        {
            if (!ValidateManager())
            {
                return;
            }

            AchievementManager.Instance.AddProgress(
                AchievementType.DistanceTravelled,
                distanceToAddKm);
        }

        [ContextMenu("Test/Set Maximum Speed")]
        public void SetMaximumSpeed()
        {
            if (!ValidateManager())
            {
                return;
            }

            AchievementManager.Instance.SetProgress(
                AchievementType.MaximumSpeed,
                speedToSetKmh);
        }

        [ContextMenu("Test/Add Ride Time")]
        public void AddRideTime()
        {
            if (!ValidateManager())
            {
                return;
            }

            AchievementManager.Instance.AddProgress(
                AchievementType.RideTime,
                rideTimeToAddSeconds);
        }

        [ContextMenu("Test/Add Checkpoint")]
        public void AddCheckpoint()
        {
            if (!ValidateManager())
            {
                return;
            }

            AchievementManager.Instance.AddProgress(
                AchievementType.CheckpointsReached,
                1f);
        }

        [ContextMenu("Test/Complete Ride")]
        public void CompleteRide()
        {
            if (!ValidateManager())
            {
                return;
            }

            AchievementManager.Instance.AddProgress(
                AchievementType.RidesCompleted,
                1f);
        }

        [ContextMenu("Test/Reset All Achievements")]
        public void ResetAchievements()
        {
            if (!ValidateManager())
            {
                return;
            }

            AchievementManager.Instance.ResetAllProgress();
        }

        private bool ValidateManager()
        {
            if (AchievementManager.Instance != null)
            {
                return true;
            }

            Debug.LogError(
                "AchievementManager was not found in the scene.");

            return false;
        }
    }
}