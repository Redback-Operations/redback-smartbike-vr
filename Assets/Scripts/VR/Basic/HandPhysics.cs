using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
public class HandPhysics : MonoBehaviour
{
    public float SmoothingAmount = 15.0f;
    public Transform Target;

    private Rigidbody _rigidbody;
    private Vector3 _targetPosition = Vector3.zero;
    private Quaternion _targetRotation = Quaternion.identity;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        TeleportToTarget();
    }

    private void Update()
    {
        SetTargetPosition();
        SetTargetRotation();
    }

    private void SetTargetPosition()
    {
        float time = SmoothingAmount * Time.unscaledDeltaTime;
        _targetPosition = Vector3.Lerp(_targetPosition, Target.position, time);
    }

    private void SetTargetRotation()
    {
        float time = SmoothingAmount * Time.unscaledDeltaTime;
        _targetRotation = Quaternion.Slerp(_targetRotation, Target.rotation, time);
    }

    private void FixedUpdate()
    {
        MoveToController();
        RotateToController();
    }

    private void MoveToController()
    {
        var positionDelta = _targetPosition - transform.position;

        //_rigidbody.velocity = Vector3.zero;
        //_rigidbody.MovePosition(transform.position + positionDelta);
    }

    private void RotateToController()
    {
        //_rigidbody.angularVelocity = Vector3.zero;
        //_rigidbody.MoveRotation(_targetRotation);
    }

    public void TeleportToTarget()
    {
        _targetPosition = Target.position;
        _targetRotation = Target.rotation;

        transform.position = _targetPosition;
        transform.rotation = _targetRotation;
    }
}
