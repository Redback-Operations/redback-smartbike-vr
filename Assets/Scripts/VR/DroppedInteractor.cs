using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DroppedInteractor : MonoBehaviour
{
	public Transform TargetLocation;
	public float ResetDelay;
	
	private IXRSelectInteractable _interactable;

	private Vector3 _target;
    private Quaternion _direction;
    private Rigidbody _rigidbody;

	private bool _return;

	void Awake()
	{
		_interactable = GetComponent<XRGrabInteractable>();

        _target = TargetLocation != null ? TargetLocation.position : transform.position;
        _direction = TargetLocation != null ? TargetLocation.rotation : transform.rotation;
        _rigidbody = GetComponent<Rigidbody>();

        _return = true;
	}

	void OnEnable()
	{
		_interactable.selectExited.AddListener(OnSelectExit); 
		_interactable.selectEntered.AddListener(OnSelect);
	}

	void OnDisable()
	{
		_interactable.selectExited.RemoveListener(OnSelectExit);
		_interactable.selectEntered.RemoveListener(OnSelect);
	}

	void OnSelect(SelectEnterEventArgs args) => CancelInvoke(nameof(ReturnHome));
	void OnSelectExit(SelectExitEventArgs args) => Invoke(nameof(ReturnHome), ResetDelay);

	protected virtual void ReturnHome()
    {
        if (!_return)
            return;

		transform.position = _target;
        transform.rotation = _direction;

        if (_rigidbody == null || _rigidbody.isKinematic)
            return;

        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }

	private void OnTriggerEnter(Collider other)
	{
		if (ControllerCheck(other.gameObject))
			return;

		var socket = other.gameObject.GetComponent<XRSocketInteractor>();

		if (socket == null)
			_return = true;
		else if (socket.CanSelect(_interactable)) 
			_return = false;
		else
			_return = true;
	}

	private void OnTriggerExit(Collider other)
	{
		if (ControllerCheck(other.gameObject))
			return;

		_return = true;
	}

	private bool ControllerCheck(GameObject collidedObject)
	{
		return collidedObject.gameObject.GetComponent<XRBaseController>() != null ? true : false;
	}
}
