using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private float gapBetweenNotifications = 0.35f;

        private readonly Queue<PendingNotification> pendingNotifications =
            new Queue<PendingNotification>();

        private Coroutine displayCoroutine;

        private readonly struct PendingNotification
        {
            public readonly string Name;
            public readonly string Description;
            public readonly int RewardPoints;

            public PendingNotification(
                string name,
                string description,
                int rewardPoints)
            {
                Name = name;
                Description = description;
                RewardPoints = rewardPoints;
            }
        }

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

            EnqueueNotification(
                new PendingNotification(
                    achievement.Data.AchievementName,
                    achievement.Data.Description,
                    achievement.Data.RewardPoints));
        }

        private void EnqueueNotification(PendingNotification notification)
        {
            if (notificationPanel == null)
            {
                Debug.LogError(
                    "Notification panel is not assigned.");

                return;
            }

            pendingNotifications.Enqueue(notification);

            if (displayCoroutine == null)
            {
                displayCoroutine =
                    StartCoroutine(DisplayPendingNotifications());
            }
        }

        private IEnumerator DisplayPendingNotifications()
        {
            while (pendingNotifications.Count > 0)
            {
                ApplyNotification(pendingNotifications.Dequeue());

                yield return new WaitForSeconds(
                    Mathf.Max(displayDuration, 0.1f));

                if (notificationPanel == null)
                {
                    break;
                }

                notificationPanel.SetActive(false);

                if (pendingNotifications.Count > 0 &&
                    gapBetweenNotifications > 0f)
                {
                    yield return new WaitForSeconds(
                        gapBetweenNotifications);
                }
            }

            displayCoroutine = null;
        }

        private void ApplyNotification(PendingNotification notification)
        {
            if (achievementNameText != null)
            {
                achievementNameText.text = notification.Name;
            }

            if (achievementDescriptionText != null)
            {
                achievementDescriptionText.text = notification.Description;
            }

            if (rewardPointsText != null)
            {
                rewardPointsText.text =
                    $"+{notification.RewardPoints} Points";
            }

            notificationPanel.SetActive(true);
        }

        private void OnEnable()
        {
            if (pendingNotifications.Count > 0 && displayCoroutine == null)
            {
                displayCoroutine =
                    StartCoroutine(DisplayPendingNotifications());
            }
        }

        private void OnDisable()
        {
            displayCoroutine = null;

            if (notificationPanel != null)
            {
                notificationPanel.SetActive(false);
            }
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
