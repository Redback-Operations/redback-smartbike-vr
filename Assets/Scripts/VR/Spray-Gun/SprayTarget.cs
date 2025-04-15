using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprayTarget : MonoBehaviour
{
    public Renderer Target;
    public int MaterialIndex;
    public Bike bike;
    void OnTriggerEnter(Collider other)
    {
        if (Target == null || SprayGun.Instance == null)
            return;

        if (MaterialIndex < 0 || MaterialIndex >= Target.materials.Length)
            return;

        if (SprayGun.Instance.SprayCollider == other)
        {
            bike.SetPartColor(Target,MaterialIndex,SprayGun.Instance.SprayColor,true);
        }
    }
}
