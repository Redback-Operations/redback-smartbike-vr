using System.Collections;
using TMPro;
using UnityEngine;

namespace SmartBike.Achievements
{
    public class AchievementNotificationUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private TMP_Text achievementNameText;
        [SerializeField] private TMP_Text achievementDescriptionText;
        [SerializeField] private TMP_Text rewardPointsText;

        [Header("Settings")]
        [SerializeField] private float displayDuration = 4f;

        private Coroutine hideCoroutine;

        private void Start()
        {
            if (notificationPanel != null)
            {
                notificationPanel.SetActive(false);
            }

            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.OnAchievementUnlocked +=
                    HandleAchievementUnlocked;
            }
            else
            {
                Debug.LogWarning(
                    "AchievementManager instance not found.");
            }
        }

        private void HandleAchievementUnlocked(Achievement achievement)
        {
            if (achievement == null || achievement.Data == null)
            {
                return;
            }

            ShowNotification(
                achievement.Data.AchievementName,
                achievement.Data.Description,
                achievement.Data.RewardPoints);
        }

        private void ShowNotification(
            string achievementName,
            string description,
            int rewardPoints)
        {
            if (notificationPanel == null)
            {
                Debug.LogError(
                    "Notification panel is not assigned.");

                return;
            }

            if (achievementNameText != null)
            {
                achievementNameText.text = achievementName;
            }

            if (achievementDescriptionText != null)
            {
                achievementDescriptionText.text = description;
            }

            if (rewardPointsText != null)
            {
                rewardPointsText.text =
                    $"+{rewardPoints} Points";
            }

            notificationPanel.SetActive(true);

            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
            }

            hideCoroutine =
                StartCoroutine(HideNotificationAfterDelay());
        }

        private IEnumerator HideNotificationAfterDelay()
        {
            yield return new WaitForSeconds(displayDuration);

            if (notificationPanel != null)
            {
                notificationPanel.SetActive(false);
            }

            hideCoroutine = null;
        }

        private void OnDestroy()
        {
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.OnAchievementUnlocked -=
                    HandleAchievementUnlocked;
            }
        }
    }
}