using System;
using UnityEngine;

public class AvatarIkController : MonoBehaviour
{
    private Bike.IkTarget[] targets;
    private Animator animator;

    public void Setup(Bike bike)
    {
        animator = GetComponent<Animator>();
        targets = bike.IkTargets;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (targets == null) return;
        foreach (var ikTarget in targets)
        {
            animator.SetIKPosition(ikTarget.target, ikTarget.tf.position);
            animator.SetIKPositionWeight(ikTarget.target, ikTarget.positionWeight);
            animator.SetIKRotation(ikTarget.target, ikTarget.tf.rotation);
            animator.SetIKRotationWeight(ikTarget.target, ikTarget.rotationWeight);
        }
    }
}