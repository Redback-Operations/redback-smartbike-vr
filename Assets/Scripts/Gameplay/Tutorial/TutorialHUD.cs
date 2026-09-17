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
        [SerializeField] private float ringFadeSeconds = 0.5f;

        [Header("Placement")]
        [SerializeField] private float distance = 3.5f;
        [SerializeField] private float heightOffset = 0.2f;
        [SerializeField] private float followSpeed = 3f;

        private Transform _target;
        private bool _placed;
        private float _cardTimer;
        private bool _cardPersistent;
        private float _ringTargetAlpha;

        public void ShowInstruction(string title, string body, bool persistent = false)
        {
            if (titleText != null) titleText.text = title;
            if (bodyText != null) bodyText.text = body;

            _cardPersistent = persistent;
            _cardTimer = cardHoldSeconds;
            SetCardAlpha(1f);
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
            {
                ringGroup.alpha = 0f;
                ringGroup.blocksRaycasts = false;
                ringGroup.interactable = false;
            }

            SetCardAlpha(0f);
        }

        private void LateUpdate()
        {
            FollowView();
            UpdateCardFade();
            UpdateRingFade();
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
                SetCardAlpha(Mathf.MoveTowards(cardGroup.alpha, 0f, Time.deltaTime / Mathf.Max(cardFadeSeconds, 0.01f)));
        }

        private void UpdateRingFade()
        {
            if (ringGroup == null)
                return;

            ringGroup.alpha = Mathf.MoveTowards(ringGroup.alpha, _ringTargetAlpha,
                Time.deltaTime / Mathf.Max(ringFadeSeconds, 0.01f));
        }

        private void SetCardAlpha(float alpha)
        {
            if (cardGroup == null)
                return;

            cardGroup.alpha = alpha;

            var visible = alpha > 0.01f;
            cardGroup.blocksRaycasts = visible;
            cardGroup.interactable = visible;
        }

        private bool ResolveTarget()
        {
            if (_target != null && _target.gameObject.activeInHierarchy)
                return true;

            var cam = Camera.main;
            if (cam == null)
            {
                _target = null;
                return false;
            }

            if (_target != cam.transform)
            {
                _target = cam.transform;
                _placed = false;
            }

            return true;
        }

        private void FollowView()
        {
            if (!ResolveTarget())
                return;

            // Follow the horizontal view direction only, so the panel doesn't
            // dive into the ground when the player looks down.
            var forward = _target.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.forward;
            forward.Normalize();

            var targetPosition = _target.position + forward * distance + Vector3.up * heightOffset;
            var targetRotation = Quaternion.LookRotation(forward);

            if (!_placed)
            {
                transform.SetPositionAndRotation(targetPosition, targetRotation);
                _placed = true;
                return;
            }

            var t = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, targetPosition, t);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t);
        }
    }
}
