using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceTowardsCamera : MonoBehaviour
{
    void Update()
    {
        if (Camera.main == null)
            return;

        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }
}
