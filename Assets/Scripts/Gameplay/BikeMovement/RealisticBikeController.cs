using UnityEngine;
using UnityEngine.Serialization;


namespace Gameplay.BikeMovement
{
    public class RealisticBikeController : MonoBehaviour, IBikeMover
    {
        [Header("Parameters")] [SerializeField]
        private float pedalRotSpeed = 60;

        [SerializeField] private float maxSteer = 45;
        [SerializeField] private float maxLean = 45;
        [SerializeField] private float maxAccelleration = 5f;
        [SerializeField] private float centerOfMassY = 0.6f;
        [SerializeField] private float balancingForce = 10f;

        [SerializeField] private AnimationCurve balanceResponseCurve;
        private WheelCollider _frontWheelCol;
        private WheelCollider _rearWheelCol;
        private Transform _frontWheelTransform;
        private Transform _frontHandlePivot;
        private Transform _rearWheelTransform;
        private Transform _pedalTf;
        private Rigidbody _rb;
        private Transform _tf;
        private float _previousLean;
        private float _mass;
        private float _currentLean;

        private Vector3 _startingPosition;
        private Vector3 _startingVelocity;

        private Quaternion _startingRotation;
        private bool _isSelected;

        private float _wheelbase = 1;

        public float DeltaTime { get; set; }
        public float Speed { get; set; }

        public void Init(GameObject controller)
        {
            _isSelected = true;
            _tf = controller.transform;
            _rb = controller.GetComponent<Rigidbody>();
            _rb.isKinematic = false;
            
            
            var currentBike = controller.GetComponentInChildren<BikeSelector>().CurrentBike;

            _frontWheelCol = currentBike.frontWheelCollider;
            _rearWheelCol = currentBike.rearWheelCollider;
            _frontWheelTransform = currentBike.frontWheelTransform;
            _frontHandlePivot = currentBike.frontHandlePivot;
            _rearWheelTransform = currentBike.rearWheelTransform;
            _pedalTf = currentBike.pedalTransform;


            _wheelbase = Vector3.Distance(_frontWheelCol.transform.position, _rearWheelCol.transform.position);

            CalculatePhysicalProperties();
        }

        private void CalculatePhysicalProperties()
        {
            // Calculate mass and center of mass adjustments.
            _mass = _rb.mass + _frontWheelCol.mass + _rearWheelCol.mass;
            Vector3 centerOfMass = _rb.centerOfMass;
            centerOfMass.y = centerOfMassY;
            centerOfMass.z = 0;
            _rb.centerOfMass = centerOfMass;

            // Set up inertia tensor (approximation).
            _frontWheelCol.GetWorldPose(out Vector3 pos1, out Quaternion rot1);
            _rearWheelCol.GetWorldPose(out Vector3 pos2, out Quaternion rot2);
            float wheelbase = (pos1 - pos2).magnitude;
            float h = _rb.centerOfMass.y;
            Vector3 offset = _rb.centerOfMass - _tf.InverseTransformPoint(pos2);
            offset.y = 0;
            float x = h / 2;
            float yVal = h / 2;
            float z = wheelbase / 2;
            x *= x;
            yVal *= yVal;
            z *= z;
            _rb.inertiaTensor = new Vector3(yVal + z, x + z, x + yVal) * _rb.mass / 2;

            // Store starting conditions for Reset().
            _startingPosition = _tf.position;
            _startingRotation = _tf.rotation;
            _startingVelocity = _rb.velocity;
        }

        /// <summary>
        /// Sets rear wheel torque according to the given acceleration.
        /// </summary>
        public void SetAcceleration(float value)
        {
            float maxA = GetMaxForwardAcceleration();
            value = Mathf.Clamp(value, -maxA, maxA);

            float rpm = (_rb.velocity.magnitude + 1) * 30 / Mathf.PI / _rearWheelCol.radius;
            float k = _rearWheelCol.rpm / rpm * 0.75f;
            if (k > 1)
                value /= k;

            float force = value * _mass;
            _rearWheelCol.motorTorque = force * _rearWheelCol.radius;
        }

        /// <summary>
        /// Returns max forward acceleration. Acceleration is limited by slipping and the possibility of rolling over.
        /// </summary>
        private float GetMaxForwardAcceleration()
        {
            float mm = (_rb.mass / 2 + _rearWheelCol.mass) / _mass;
            float a = -Physics.gravity.y * mm * _rearWheelCol.forwardFriction.extremumValue * 1.0f; // 0.85
            float wheelbase = (_frontWheelCol.transform.position - _rearWheelCol.transform.position).magnitude;
            float h = _rb.centerOfMass.y;
            float safeA = -Physics.gravity.y * wheelbase / 2 / h * 1.0f; // 0.3
            a = Mathf.Min(a, safeA);
            return a;
        }

        /// <summary>
        /// Returns bike to the starting position.
        /// </summary>
        public void Reset()
        {
            _tf.position = _startingPosition;
            _tf.rotation = _startingRotation;
            _rb.velocity = _startingVelocity;
            _rb.angularVelocity = Vector3.zero;
            _frontWheelCol.steerAngle = 0;
            _rearWheelCol.motorTorque = 0;
        }

        public void HanldeInput(Vector2 direction)
        {
            if (direction.y > 0)
            {
                _pedalTf.transform.localRotation = Quaternion.Euler(0, 0, pedalRotSpeed * DeltaTime) *
                                                   _pedalTf.transform.localRotation;
            }

            ApplyMotor(direction);
            HandleSteering(direction);
            ApplyBalance();
        }


        private void ApplyBalance()
        {
            // Get current tilt around Z (sideways)
            float tiltAngle = Vector3.SignedAngle(_tf.up, Vector3.up, _tf.forward);
            float delta = tiltAngle - _currentLean;
            float output = balanceResponseCurve.Evaluate(Mathf.Lerp(1, 0, delta / 30f)) * delta;
            float balanceTorque = output * balancingForce;
            _rb.AddTorque(_tf.forward * balanceTorque);
        }

        private void HandleSteering(Vector2 direction)
        {
            var horizontalInput = direction.x;
            float normalizedSpeed = Mathf.Clamp01(_tf.InverseTransformDirection(_rb.velocity).z / 10f);
            float effectiveSteer = horizontalInput * Mathf.Lerp(maxSteer, 0f, normalizedSpeed);
            float targetLean = horizontalInput * Mathf.Lerp(0f, maxLean, normalizedSpeed);
            _currentLean = targetLean;
            SetSteer(effectiveSteer);
        }

        private void SetSteer(float value)
        {
            float clampedValue = Mathf.Clamp(value, -maxSteer, maxSteer);
            _frontWheelCol.steerAngle = clampedValue;
        }

        private void ApplyMotor(Vector2 input)
        {
            var targetSpeed = input.y * Speed;
            Vector3 localV = _tf.InverseTransformVector(_rb.velocity);
            float diff = targetSpeed - localV.z;
            float a = Mathf.Clamp(diff, -maxAccelleration, maxAccelleration);

            if (a > 0)
            {
                SetAcceleration(a);
                SetBrake(0);
            }
            else
            {
                SetAcceleration(0);
                SetBrake(-a);
            }
        }

        private void SetBrake(float value)
        {
            _rearWheelCol.brakeTorque = value;
        }

        private void Update()
        {
            if (!_isSelected) return;
            // Update rear wheel transform based on the collider's world pose.
            _rearWheelCol.GetWorldPose(out var rearWheelPos, out var rearWheelRot);
            _rearWheelTransform.position = rearWheelPos;
            _rearWheelTransform.rotation = rearWheelRot;

            _frontWheelCol.GetWorldPose(out var frontWheelPos, out var frontWheelRot);
            _frontWheelTransform.localRotation = Quaternion.Euler(frontWheelRot.eulerAngles.x, -90, 0);
            _frontHandlePivot.localRotation = Quaternion.Euler(0, _frontWheelCol.steerAngle, 0);
        }
    }
}