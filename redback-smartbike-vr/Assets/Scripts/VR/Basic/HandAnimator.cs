using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class HandAnimator : MonoBehaviour
{
    public float Speed = 5.0f;
    public XRController Controller = null;

    private Animator _animator = null;

    private readonly List<Finger> _gripFingers = new List<Finger>
    {
        new Finger(FingerType.Middle),
        new Finger(FingerType.Ring),
        new Finger(FingerType.Pinky)
    };

    private readonly List<Finger> _pointerFingers = new List<Finger>
    {
        new Finger(FingerType.Index),
        new Finger(FingerType.Thumb)
    };

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // store input
        CheckGrip();
        CheckPointer();

        // smooth input values
        SmoothFinger(_gripFingers);
        SmoothFinger(_pointerFingers);

        // apply smoothed values
        AnimateFinger(_gripFingers);
        AnimateFinger(_pointerFingers);
    }

    private void CheckGrip()
    {
        if (Controller.inputDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
            SetFingerTargets(_gripFingers, gripValue);
    }

    private void CheckPointer()
    {
        if (Controller.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float pointerValue))
            SetFingerTargets(_pointerFingers, pointerValue);
    }

    private void SetFingerTargets(List<Finger> fingers, float value)
    {
        foreach (var finger in fingers)
        {
            finger.Target = value;
        }
    }

    private void SmoothFinger(List<Finger> fingers)
    {
        foreach (var finger in fingers)
        {
            float time = Speed * Time.deltaTime;
            finger.Current = Mathf.MoveTowards(finger.Current, finger.Target, time);
        }
    }

    private void AnimateFinger(List<Finger> fingers)
    {
        foreach (var finger in fingers)
        {
            AnimateFinger(finger.Type.ToString(), finger.Current);
        }
    }

    private void AnimateFinger(string finger, float blend)
    {
        _animator.SetFloat(finger, blend);
    }
}