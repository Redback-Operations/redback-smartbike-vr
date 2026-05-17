using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.BikeMovement
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class BikeBuildingCollisionHandler : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private string buildingTag = "Building";

        [Header("Response")]
        [SerializeField] private bool stopOnHit = true;
        [SerializeField] private bool pushBackOnHit = true;
        [SerializeField] private float pushBackDistance = 0.75f;
        [SerializeField] private float pushBackSpeed = 8f;

        [Header("Cooldown")]
        [SerializeField] private float collisionCooldown = 0.15f;

        private Rigidbody _rb;
        private bool _isColliding;
        private bool _isPushingBack;
        private float _lastHitTime;

        public bool IsColliding => _isColliding || _isPushingBack;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!IsBuilding(collision.collider)) return;
            HandleHit(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            if (!IsBuilding(collision.collider)) return;
            _isColliding = true;
        }

        private void OnCollisionExit(Collision collision)
        {
            if (!IsBuilding(collision.collider)) return;
            _isColliding = false;
        }

        private bool IsBuilding(Collider other)
        {
            return other.CompareTag(buildingTag);
        }

        private void HandleHit(Collision collision)
        {
            if (Time.time - _lastHitTime < collisionCooldown) return;

            _lastHitTime = Time.time;
            _isColliding = true;

            if (stopOnHit)
            {
                _rb.velocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }

            if (pushBackOnHit && !_isPushingBack)
            {
                ContactPoint contact = collision.GetContact(0);
                Vector3 pushDirection = (transform.position - contact.point).normalized;
                pushDirection.y = 0f;

                if (pushDirection.sqrMagnitude < 0.001f)
                    pushDirection = -transform.forward;

                StartCoroutine(PushBack(pushDirection));
            }
        }

        private System.Collections.IEnumerator PushBack(Vector3 direction)
        {
            _isPushingBack = true;

            Vector3 start = transform.position;
            Vector3 target = start + direction.normalized * pushBackDistance;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * pushBackSpeed;
                transform.position = Vector3.Lerp(start, target, t);
                yield return null;
            }

            _isPushingBack = false;
            _isColliding = false;
        }
    }
}