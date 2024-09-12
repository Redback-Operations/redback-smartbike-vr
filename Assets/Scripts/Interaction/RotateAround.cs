using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAround : MonoBehaviour
{
    public Transform Target;

    void LateUpdate()
    {
        if (Target == null)
            return;

        var target = Target.rotation.eulerAngles.y;
        var current = transform.rotation.eulerAngles.y;

        if (target > 300 && current < 60)
            target -= 360;

        if (target < 60 && current > 300)
            current -= 360;

        var difference = target - current;
        transform.RotateAround(Target.position, Vector3.up, difference * Time.deltaTime);
    }
}
