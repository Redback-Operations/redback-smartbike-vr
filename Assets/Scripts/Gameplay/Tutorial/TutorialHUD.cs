using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Tutorial
{
    
    public class TutorialHUD : MonoBehaviour
    {
        [Header("Instruction card")]
        [SerializeField] private CanvasGroup cardGroup;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private float cardHoldSeconds = 6f;
        [SerializeField] private float cardFadeSeconds = 0.8f;

        [Header("Progress ring")]
        [SerializeField] private CanvasGroup ringGroup;
        [SerializeField] private Image ringFill;
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField] private TextMeshProUGUI captionText;

        [Header("Placement")]
        [SerializeField] private float distance = 3.5f;
        [SerializeField] private float heightOffset = 0.2f;
        [SerializeField] private float followSpeed = 3f;

        private Transform _target;
        private float _cardTimer;
        private bool _cardPersistent;
        private float _ringTargetAlpha;

        public void ShowInstruction(string title, string body, bool persistent = false)
        {
            if (titleText != null) titleText.text = title;
            if (bodyText != null) bodyText.text = body;

            _cardPersistent = persistent;
            _cardTimer = cardHoldSeconds;
            if (cardGroup != null)
                cardGroup.alpha = 1f;
        }

        public void SetProgress(float normalized, string value, string caption)
        {
            _ringTargetAlpha = 1f;

            if (ringFill != null)
                ringFill.fillAmount = Mathf.Clamp01(normalized);
            if (valueText != null)
                valueText.text = value;
            if (captionText != null)
                captionText.text = caption;
        }

        public void HideProgress()
        {
            _ringTargetAlpha = 0f;
        }

        private void Awake()
        {
            if (ringGroup != null)
                ringGroup.alpha = 0f;
        }

        private void LateUpdate()
        {
            FollowView();
            UpdateCardFade();

            if (ringGroup != null)
                ringGroup.alpha = Mathf.MoveTowards(ringGroup.alpha, _ringTargetAlpha, Time.deltaTime * 2f);
        }

        private void UpdateCardFade()
        {
            if (cardGroup == null || _cardPersistent)
                return;

            if (_cardTimer > 0f)
            {
                _cardTimer -= Time.deltaTime;
                return;
            }

            if (cardGroup.alpha > 0f)
                cardGroup.alpha = Mathf.MoveTowards(cardGroup.alpha, 0f, Time.deltaTime / Mathf.Max(cardFadeSeconds, 0.01f));
        }

        private void FollowView()
        {
            if (_target == null)
            {
                var cam = Camera.main;
                if (cam == null)
                    return;
                _target = cam.transform;
            }

            // Follow the horizontal view direction only, so the panel doesn't
            // dive into the ground when the player looks down.
            var forward = _target.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.forward;
            forward.Normalize();

            var targetPosition = _target.position + forward * distance + Vector3.up * heightOffset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

            var lookDirection = transform.position - _target.position;
            lookDirection.y = 0f;
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                var targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, followSpeed * Time.deltaTime);
            }
        }
    }
}
