using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Rotator : MonoBehaviour
{
    public Transform LinkedDial;

    public int SnapRotationAmount = 25;
    public float AngleTolerance;

    public Vector3 AxisTarget;

    public Vector3 AxisMinRotation;
    public Vector3 AxisMaxRotation;

    private IXRHoverInteractor interactor;
    private float startAngle;
    private bool shouldGetHandRotation = false;

    public void GrabEnd(SelectExitEventArgs args)
    {
        shouldGetHandRotation = false;
    }

    public void GrabbedBy(SelectEnterEventArgs arg0)
    {
        interactor = GetComponent<XRGrabInteractable>().GetOldestInteractorHovering();
        shouldGetHandRotation = true;
        startAngle = GetInteractorRotation();
    }

    void Update()
    {
        if (!shouldGetHandRotation)
            return;

        var rotationAngle = GetInteractorRotation(); //gets the current controller angle
        GetRotationDistance(rotationAngle);
    }

    public float GetInteractorRotation() => interactor.transform.eulerAngles.z;

    #region TheMath!
    private void GetRotationDistance(float currentAngle)
    {
        var angleDifference = Mathf.Abs(startAngle - currentAngle);

        if (angleDifference > AngleTolerance)
        {
            if (angleDifference > 270f) //checking to see if the user has gone from 0-360 - a very tiny movement but will trigger the angletolerance
            {
                float angleCheck;

                if (startAngle < currentAngle) 
                {
                    angleCheck = CheckAngle(currentAngle, startAngle);

                    if (angleCheck < AngleTolerance)
                        return;

                    RotateDialClockwise();
                    startAngle = currentAngle;
                }
                else if (startAngle > currentAngle) 
                {
                    angleCheck = CheckAngle(currentAngle, startAngle);

                    if (angleCheck < AngleTolerance)
                        return;

                    RotateDialAntiClockwise();
                    startAngle = currentAngle;
                }
            }
            else
            {
                if (startAngle < currentAngle)
                {
                    RotateDialAntiClockwise();
                    startAngle = currentAngle;
                }
                else if (startAngle > currentAngle)
                {
                    RotateDialClockwise();
                    startAngle = currentAngle;
                }
            }
        }
    }
    #endregion

    private float CheckAngle(float currentAngle, float startAngle) => (360f - currentAngle) + startAngle;

    private void RotateDialClockwise()
    {
        LinkedDial.localEulerAngles = Clamp(new Vector3(LinkedDial.localEulerAngles.x + SnapRotationAmount * AxisTarget.x,
            LinkedDial.localEulerAngles.y + SnapRotationAmount * AxisTarget.y,
            LinkedDial.localEulerAngles.z + SnapRotationAmount * AxisTarget.z));
    }

    private void RotateDialAntiClockwise()
    {
        LinkedDial.localEulerAngles = Clamp(new Vector3(LinkedDial.localEulerAngles.x - SnapRotationAmount * AxisTarget.x, 
            LinkedDial.localEulerAngles.y - SnapRotationAmount * AxisTarget.y, 
            LinkedDial.localEulerAngles.z - SnapRotationAmount * AxisTarget.z));
    }

    private Vector3 Clamp(Vector3 updatedAngle)
    {
        if (AxisMinRotation.x > 0 || AxisMaxRotation.x > 0)
            updatedAngle.x = Mathf.Clamp(updatedAngle.x, AxisMinRotation.x, AxisMaxRotation.x);
        if (AxisMinRotation.y > 0 || AxisMaxRotation.y > 0)
            updatedAngle.y = Mathf.Clamp(updatedAngle.y, AxisMinRotation.y, AxisMaxRotation.y);
        if (AxisMinRotation.z > 0 || AxisMaxRotation.z > 0)
            updatedAngle.z = Mathf.Clamp(updatedAngle.z, AxisMinRotation.z, AxisMaxRotation.z);

        return updatedAngle;
    }
}
