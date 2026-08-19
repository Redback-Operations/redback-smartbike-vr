using UnityEngine;
using UnityEngine.Serialization;


namespace Gameplay.BikeMovement
{
   
    public class RealisticBikeController : MonoBehaviour, IBikeMover
    {
        [Header("Parameters")]
        [SerializeField]
        private float pedalRotSpeed = 60;

        [SerializeField] private float maxSteer = 45;
        [SerializeField] private float maxLean = 45;
        [SerializeField] private float maxAccelleration = 3f;
        [SerializeField] private float centerOfMassY = 0.6f;
        [SerializeField] private float balancingForce = 10f;

        [SerializeField] private AnimationCurve balanceResponseCurve;
        [SerializeField] private float stoppedBalanceForce = 80f;
        [SerializeField] private float stoppedBalanceSpeedThreshold = 1.5f;

        private IPlayerInput _playerInput;
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
            // set true only once every reference below is confirmed assigned
            _isSelected = false;
            _tf = controller.transform;
            _rb = controller.GetComponent<Rigidbody>();
            _rb.isKinematic = false;
            _rb.constraints =
                RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationZ;

            var selector = controller.GetComponentInChildren<BikeSelector>();
            var currentBike = selector == null ? null : selector.CurrentBike;

            if (!BikeRigIsComplete(currentBike))
            {
                // _isSelected stays false, so Update/HanldeInput/Reset all no-op.
                // Without this guard a bike that was never rigged throws an
                // UnassignedReferenceException every single frame.
                _isSelected = false;
                return;
            }

            _frontWheelCol = currentBike.frontWheelCollider;
            _rearWheelCol = currentBike.rearWheelCollider;
            _frontWheelTransform = currentBike.frontWheelTransform;
            _frontHandlePivot = currentBike.frontHandlePivot;
            _rearWheelTransform = currentBike.rearWheelTransform;
            _pedalTf = currentBike.pedalTransform;


            _wheelbase = Vector3.Distance(_frontWheelCol.transform.position, _rearWheelCol.transform.position);

            _isSelected = true;
            CalculatePhysicalProperties();

        }

        /// <summary>
        /// True when every reference this controller dereferences per-frame is
        /// assigned. Logs one actionable error naming the bike and the missing
        /// fields instead of letting Update() throw forever.
        /// </summary>
        private static bool BikeRigIsComplete(Bike bike)
        {
            if (bike == null)
            {
                Debug.LogError("[RealisticBikeController] No bike selected (BikeSelector missing or CurrentBike null). Bike movement disabled.");
                return false;
            }

            var missing = new System.Collections.Generic.List<string>();
            if (bike.frontWheelCollider == null) missing.Add("frontWheelCollider");
            if (bike.rearWheelCollider == null) missing.Add("rearWheelCollider");
            if (bike.frontWheelTransform == null) missing.Add("frontWheelTransform");
            if (bike.rearWheelTransform == null) missing.Add("rearWheelTransform");
            if (bike.frontHandlePivot == null) missing.Add("frontHandlePivot");
            if (bike.pedalTransform == null) missing.Add("pedalTransform");

            if (missing.Count == 0)
                return true;

            Debug.LogError(
                $"[RealisticBikeController] Bike '{bike.name}' is not rigged for the realistic controller. " +
                $"Unassigned on its Bike component: {string.Join(", ", missing)}. " +
                "Bike movement is disabled for this bike. Rig it the way RoadBikeV5 is rigged " +
                "(Tools > Missions > Rig Selected Bike), or select a rigged bike.",
                bike);
            return false;
        }
        private void LimitAngularVelocity()
        {
            float maxAngularSpeed = 3f;

            if (_rb.angularVelocity.magnitude > maxAngularSpeed)
            {
                _rb.angularVelocity = _rb.angularVelocity.normalized * maxAngularSpeed;
            }
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
            if (!_isSelected) return;

            _tf.position = _startingPosition;
            _tf.rotation = _startingRotation;
            _rb.velocity = _startingVelocity;
            
            _frontWheelCol.steerAngle = 0;
            _rearWheelCol.motorTorque = 0;
        }

        public void HanldeInput(Vector2 direction)
        {
            if (!_isSelected) return;

            if (direction.y > 0)
            {
                _pedalTf.transform.localRotation = Quaternion.Euler(0, 0, pedalRotSpeed * DeltaTime) *
                                                   _pedalTf.transform.localRotation;
            }

            ApplyMotor(direction);
            HandleSteering(direction);
            ApplyBalance();
            LimitAngularVelocity();
        }


        private void ApplyBalance()
        {
            float tiltAngle = Vector3.SignedAngle(_tf.up, Vector3.up, _tf.forward);

            float delta = tiltAngle - _currentLean;

            float output =
                balanceResponseCurve.Evaluate(Mathf.Lerp(1, 0, delta / 30f)) * delta;

            float balanceTorque = output * balancingForce;

            _rb.AddTorque(_tf.forward * balanceTorque);
        }

        private void HandleSteering(Vector2 direction)
        {
            float horizontalInput = direction.x;

            float forwardSpeed = Mathf.Abs(_tf.InverseTransformDirection(_rb.velocity).z);
            float normalizedSpeed = Mathf.Clamp01(forwardSpeed / 25f);

            // Less steering at high speed
            float steerMultiplier = Mathf.Lerp(1f, 0.15f, normalizedSpeed);
            float effectiveSteer = horizontalInput * maxSteer * steerMultiplier;

            // Less lean at high speed to stop spinning/flipping
            float leanMultiplier = Mathf.Lerp(1f, 0.35f, normalizedSpeed);
            float targetLean = horizontalInput * maxLean * leanMultiplier;

            _currentLean = 0f;

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
            float currentSpeed = localV.z;
            float diff = targetSpeed - currentSpeed;
            float a = Mathf.Clamp(diff, -maxAccelleration, maxAccelleration);

            bool shouldBrake = a < 0 && currentSpeed > 0.1f;

            if (shouldBrake)
            {
                SetAcceleration(0);
                SetBrake(-a);
            }
            else
            {
                SetAcceleration(a);
                SetBrake(0);
            }
        }

        private void SetBrake(float value)
        {
            _rearWheelCol.brakeTorque = value;
        }

        private void Update()
        {
            if (!_isSelected) return;

            _rearWheelCol.GetWorldPose(out var rearWheelPos, out var rearWheelRot);
            _rearWheelTransform.position = rearWheelPos;
            _rearWheelTransform.rotation = rearWheelRot;

            _frontWheelCol.GetWorldPose(out var frontWheelPos, out var frontWheelRot);

            _frontWheelTransform.localRotation =
                Quaternion.Euler(frontWheelRot.eulerAngles.x, -90, 0);

            _frontHandlePivot.localRotation =
                Quaternion.Euler(0, _frontWheelCol.steerAngle, 0);
        }
    }
}