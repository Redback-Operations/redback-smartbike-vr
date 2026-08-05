using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmartBike.Achievements
{
    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }

        [Header("Achievement Definitions")]
        [SerializeField]
        private List<AchievementData> achievementDefinitions =
            new List<AchievementData>();

        [Header("Settings")]
        [SerializeField] private bool loadProgressOnStart = true;
        [SerializeField] private bool saveProgressAutomatically = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugMessages = true;

        private readonly List<Achievement> achievements =
            new List<Achievement>();

        private int totalAchievementPoints;

        public IReadOnlyList<Achievement> Achievements => achievements;
        public int TotalAchievementPoints => totalAchievementPoints;

        public event Action<Achievement> OnAchievementProgressChanged;
        public event Action<Achievement> OnAchievementUnlocked;

        private const string PointsSaveKey =
            "SmartBike_TotalAchievementPoints";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAchievements();

            if (loadProgressOnStart)
            {
                LoadAllProgress();
            }
        }

        private void InitializeAchievements()
        {
            achievements.Clear();

            foreach (AchievementData definition in achievementDefinitions)
            {
                if (definition == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(definition.AchievementId))
                {
                    Debug.LogWarning(
                        $"Achievement '{definition.name}' has no ID.",
                        definition);

                    continue;
                }

                if (GetAchievement(definition.AchievementId) != null)
                {
                    Debug.LogWarning(
                        $"Duplicate achievement ID: {definition.AchievementId}",
                        definition);

                    continue;
                }

                achievements.Add(new Achievement(definition));
            }
        }

        public void AddProgress(
            AchievementType achievementType,
            float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            foreach (Achievement achievement in achievements)
            {
                if (achievement.Data == null ||
                    achievement.IsUnlocked ||
                    achievement.Data.AchievementType != achievementType)
                {
                    continue;
                }

                bool unlockedNow = achievement.AddProgress(amount);

                OnAchievementProgressChanged?.Invoke(achievement);

                if (unlockedNow)
                {
                    HandleAchievementUnlocked(achievement);
                }
            }

            if (saveProgressAutomatically)
            {
                SaveAllProgress();
            }
        }

        public void SetProgress(
            AchievementType achievementType,
            float value)
        {
            if (value < 0f)
            {
                return;
            }

            foreach (Achievement achievement in achievements)
            {
                if (achievement.Data == null ||
                    achievement.IsUnlocked ||
                    achievement.Data.AchievementType != achievementType)
                {
                    continue;
                }

                bool unlockedNow = achievement.SetProgress(value);

                OnAchievementProgressChanged?.Invoke(achievement);

                if (unlockedNow)
                {
                    HandleAchievementUnlocked(achievement);
                }
            }

            if (saveProgressAutomatically)
            {
                SaveAllProgress();
            }
        }

        private void HandleAchievementUnlocked(Achievement achievement)
        {
            if (achievement == null || achievement.Data == null)
            {
                return;
            }

            totalAchievementPoints += achievement.Data.RewardPoints;

            if (showDebugMessages)
            {
                Debug.Log(
                    $"Achievement unlocked: " +
                    $"{achievement.Data.AchievementName}. " +
                    $"Reward: {achievement.Data.RewardPoints} points.");
            }

            OnAchievementUnlocked?.Invoke(achievement);
        }

        public Achievement GetAchievement(string achievementId)
        {
            if (string.IsNullOrWhiteSpace(achievementId))
            {
                return null;
            }

            return achievements.Find(
                achievement =>
                    achievement.Data != null &&
                    achievement.Data.AchievementId == achievementId);
        }

        public List<Achievement> GetAchievementsByType(
            AchievementType achievementType)
        {
            return achievements.FindAll(
                achievement =>
                    achievement.Data != null &&
                    achievement.Data.AchievementType == achievementType);
        }

        public void SaveAllProgress()
        {
            foreach (Achievement achievement in achievements)
            {
                if (achievement.Data == null)
                {
                    continue;
                }

                string id = achievement.Data.AchievementId;

                PlayerPrefs.SetFloat(
                    GetProgressKey(id),
                    achievement.CurrentProgress);

                PlayerPrefs.SetInt(
                    GetUnlockedKey(id),
                    achievement.IsUnlocked ? 1 : 0);

                PlayerPrefs.SetString(
                    GetUnlockedDateKey(id),
                    achievement.UnlockedDate);
            }

            PlayerPrefs.SetInt(
                PointsSaveKey,
                totalAchievementPoints);

            PlayerPrefs.Save();
        }

        public void LoadAllProgress()
        {
            foreach (Achievement achievement in achievements)
            {
                if (achievement.Data == null)
                {
                    continue;
                }

                string id = achievement.Data.AchievementId;

                float savedProgress = PlayerPrefs.GetFloat(
                    GetProgressKey(id),
                    0f);

                bool savedUnlocked = PlayerPrefs.GetInt(
                    GetUnlockedKey(id),
                    0) == 1;

                string savedDate = PlayerPrefs.GetString(
                    GetUnlockedDateKey(id),
                    string.Empty);

                achievement.LoadProgress(
                    savedProgress,
                    savedUnlocked,
                    savedDate);
            }

            totalAchievementPoints = PlayerPrefs.GetInt(
                PointsSaveKey,
                0);
        }

        public void ResetAllProgress()
        {
            foreach (Achievement achievement in achievements)
            {
                if (achievement.Data == null)
                {
                    continue;
                }

                string id = achievement.Data.AchievementId;

                PlayerPrefs.DeleteKey(GetProgressKey(id));
                PlayerPrefs.DeleteKey(GetUnlockedKey(id));
                PlayerPrefs.DeleteKey(GetUnlockedDateKey(id));

                achievement.ResetProgress();
            }

            PlayerPrefs.DeleteKey(PointsSaveKey);
            PlayerPrefs.Save();

            totalAchievementPoints = 0;
        }

        private static string GetProgressKey(string id)
        {
            return $"SmartBike_Achievement_{id}_Progress";
        }

        private static string GetUnlockedKey(string id)
        {
            return $"SmartBike_Achievement_{id}_Unlocked";
        }

        private static string GetUnlockedDateKey(string id)
        {
            return $"SmartBike_Achievement_{id}_Date";
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveAllProgress();
            }
        }

        private void OnApplicationQuit()
        {
            SaveAllProgress();
        }
    }
}