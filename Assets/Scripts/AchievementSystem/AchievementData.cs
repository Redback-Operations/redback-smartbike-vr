using UnityEngine;

namespace SmartBike.Achievements
{
    [CreateAssetMenu(
        fileName = "NewAchievement",
        menuName = "SmartBike/Achievement",
        order = 1)]
    public class AchievementData : ScriptableObject
    {
        [Header("Achievement Information")]
        [SerializeField] private string achievementId;
        [SerializeField] private string achievementName;

        [TextArea(2, 4)]
        [SerializeField] private string description;

        [SerializeField] private Sprite icon;

        [Header("Unlock Requirement")]
        [SerializeField] private AchievementType achievementType;

        [Min(1f)]
        [SerializeField] private float targetValue = 1f;

        [Header("Reward")]
        [Min(0)]
        [SerializeField] private int rewardPoints = 10;

        public string AchievementId => achievementId;
        public string AchievementName => achievementName;
        public string Description => description;
        public Sprite Icon => icon;
        public AchievementType AchievementType => achievementType;
        public float TargetValue => targetValue;
        public int RewardPoints => rewardPoints;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(achievementId))
            {
                achievementId = name
                    .Trim()
                    .ToLowerInvariant()
                    .Replace(" ", "_");
            }

            targetValue = Mathf.Max(1f, targetValue);
            rewardPoints = Mathf.Max(0, rewardPoints);
        }
#endif
    }
}