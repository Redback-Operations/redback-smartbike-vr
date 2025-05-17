using System;
using UnityEngine;

namespace Gameplay.BikeMovement
{
    public class SimpleBikeController : MonoBehaviour, IBikeMover
    {
        public enum GroundLockMode
        {
            TerrainLock,
            FreePhysics
        }

        public GroundLockMode BikeGroundMode;

        public float rotationSpeed = 90f;
        private float _groundHeight = 0;
        private float _angleX = 0;
        private float change;
        private Quaternion OldRotation;

        private Rigidbody rb;
        private Transform tf;

        private bool _selected = false;

        public float Speed { get; set; }

        private Transform _pedalTf;
        private Transform _frontWheelTf;
        private Transform _rearWheelTf;

        private float wheelRadius;
        private Collider _collider;

        public void Init(GameObject controller)
        {
            _selected = true;
            tf = controller.transform;
            rb = controller.GetComponent<Rigidbody>();
            rb.isKinematic = true;

            var bike = controller.GetComponentInChildren<BikeSelector>().CurrentBike;

            _frontWheelTf = bike.frontWheelTransform;
            _rearWheelTf = bike.rearWheelTransform;
            _pedalTf = bike.pedalTransform;
            wheelRadius = bike.frontWheelCollider.radius;
            change = tf.rotation.y;


            _collider = controller.GetComponent<Collider>();

            if (BikeGroundMode == GroundLockMode.FreePhysics)
            {
                Collider[] _childColliders = controller.GetComponentsInChildren<Collider>();

                Debug.Log(_childColliders.Length);

                //stop base collider from impacting with child colliders
                foreach (Collider child in _childColliders)
                {
                    Debug.Log("Finding collision");
                    if (child != _collider)
                    {
                        Debug.Log("Removing collision");
                        Physics.IgnoreCollision(_collider, child);
                    }
                }

                //fix collisions
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                rb.constraints = RigidbodyConstraints.FreezeRotationZ;

                //disable kinematic mode in rigidbody, and istrigger mode on collider to unlock physics
                rb.isKinematic = false;
                _collider.isTrigger = false;
            }
        }

        public void HanldeInput(Vector2 direction)
        {
            Rotate(direction);
            if (direction.y > 0)
            {
                _pedalTf.transform.localRotation = Quaternion.Euler(0, 0, 360f * Time.fixedDeltaTime) *
                                                   _pedalTf.transform.localRotation;
            }

            var wheelRot = Quaternion.AngleAxis(Mathf.Rad2Deg * Speed * direction.y * Time.fixedDeltaTime
                                                / wheelRadius,
                _frontWheelTf.right);
            _frontWheelTf.rotation = wheelRot * _frontWheelTf.rotation;
            _rearWheelTf.rotation = wheelRot * _rearWheelTf.rotation;

            Move(direction);
        }

        private void Move(Vector2 direction)
        {
            // Get the playter's forward direction made by Jai (updated by Jonathan)
            Vector3 facingDirection = tf.forward;
            // To prevent the bike from moving in the Y-axis
            facingDirection.y = 0;
            var position = tf.position;
            position += facingDirection.normalized * (Speed * Time.fixedDeltaTime * direction.y);
            if (BikeGroundMode == GroundLockMode.TerrainLock)
            {
                position.y = _groundHeight;
            }

            tf.position = position;
        }

        private void Rotate(Vector2 direction)
        {
            // updated to account for frame rate
            change += direction.x * rotationSpeed * Time.fixedDeltaTime;

            //To make bike go towards new rotation made by Jai
            OldRotation = Quaternion.Euler(_angleX, rb.transform.rotation.y + change, rb.transform.rotation.z);
            //To make bike go towards new rotation made by Jai
            rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, OldRotation, 0.15f);
        }

        void LateUpdate()
        {
            if (!_selected) return;
            if (Terrain.activeTerrain == null)
                return;

            _groundHeight = Terrain.activeTerrain.SampleHeight(tf.position);

            var normal = CalculateNormal(Terrain.activeTerrain, tf.position);
            var forward = Vector3.Cross(normal, -tf.right);

            var rotation = Quaternion.LookRotation(forward, normal);
            _angleX = rotation.eulerAngles.x;
        }

        private Vector3 CalculateNormal(Terrain terrain, Vector3 position)
        {
            var offset = 0.1f;

            var heightLeft = terrain.SampleHeight(position + Vector3.left * offset);
            var heightRight = terrain.SampleHeight(position + Vector3.right * offset);
            var heightForward = terrain.SampleHeight(position + Vector3.forward * offset);
            var heightBack = terrain.SampleHeight(position + Vector3.back * offset);

            var tangentX = new Vector3(2.0f * offset, heightRight - heightLeft, 0).normalized;
            var tangentZ = new Vector3(0, heightForward - heightBack, 2.0f * offset).normalized;

            var normal = Vector3.Cross(tangentZ, tangentX).normalized;

            return normal;
        }
    }
}