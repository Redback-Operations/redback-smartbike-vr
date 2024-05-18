using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprayTarget : MonoBehaviour
{
    public Renderer Target;
    public int MaterialIndex;

    private LoadBike _bikes;

    void Start()
    {
        _bikes = GetComponentInParent<LoadBike>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (Target == null || SprayGun.Instance == null)
            return;

        if (MaterialIndex < 0 || MaterialIndex >= Target.materials.Length)
            return;

        if (SprayGun.Instance.SprayCollider == other)
            Target.materials[MaterialIndex].color = SprayGun.Instance.SprayColor;

        if (_bikes != null)
            _bikes.CustomizeBike(MaterialIndex, Target.name, SprayGun.Instance.SprayColor);
    }
}
