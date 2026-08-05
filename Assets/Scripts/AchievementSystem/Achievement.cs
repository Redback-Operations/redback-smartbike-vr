using System;
using UnityEngine;

namespace SmartBike.Achievements
{
    [Serializable]
    public class Achievement
    {
        [SerializeField] private AchievementData data;
        [SerializeField] private float currentProgress;
        [SerializeField] private bool isUnlocked;
        [SerializeField] private string unlockedDate;

        public AchievementData Data => data;
        public float CurrentProgress => currentProgress;
        public bool IsUnlocked => isUnlocked;
        public string UnlockedDate => unlockedDate;

        public float ProgressPercentage
        {
            get
            {
                if (data == null || data.TargetValue <= 0f)
                {
                    return 0f;
                }

                return Mathf.Clamp01(currentProgress / data.TargetValue);
            }
        }

        public Achievement(AchievementData achievementData)
        {
            data = achievementData;
            currentProgress = 0f;
            isUnlocked = false;
            unlockedDate = string.Empty;
        }

        public bool AddProgress(float amount)
        {
            if (isUnlocked || data == null || amount <= 0f)
            {
                return false;
            }

            currentProgress += amount;
            currentProgress = Mathf.Min(currentProgress, data.TargetValue);

            return CheckForUnlock();
        }

        public bool SetProgress(float value)
        {
            if (isUnlocked || data == null)
            {
                return false;
            }

            currentProgress = Mathf.Max(currentProgress, value);
            currentProgress = Mathf.Min(currentProgress, data.TargetValue);

            return CheckForUnlock();
        }

        private bool CheckForUnlock()
        {
            if (data == null || isUnlocked)
            {
                return false;
            }

            if (currentProgress >= data.TargetValue)
            {
                Unlock();
                return true;
            }

            return false;
        }

        public void Unlock()
        {
            if (isUnlocked)
            {
                return;
            }

            isUnlocked = true;

            if (data != null)
            {
                currentProgress = data.TargetValue;
            }

            unlockedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public void LoadProgress(
            float savedProgress,
            bool savedUnlockedState,
            string savedUnlockedDate)
        {
            currentProgress = Mathf.Max(0f, savedProgress);
            isUnlocked = savedUnlockedState;
            unlockedDate = savedUnlockedDate ?? string.Empty;

            if (data != null)
            {
                currentProgress = Mathf.Min(currentProgress, data.TargetValue);
            }
        }

        public void ResetProgress()
        {
            currentProgress = 0f;
            isUnlocked = false;
            unlockedDate = string.Empty;
        }
    }
}