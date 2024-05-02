using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class MouseInteraction : MonoBehaviour
{
    public float Sensitivity = 45;

    public float XRotation;
    public float YRotation;

    private float _originSpeed;

    void Start()
    {
        if (XRSettings.enabled)
            return;

        // disable the controllers
        var controllers = GetComponentsInChildren<XRController>();

        foreach (var controller in controllers)
        {
            controller.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // if the headset is enabled, don't enable mouse interaction
        if (XRSettings.enabled)
            return;

        var mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * Sensitivity;
        var mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * Sensitivity;

        XRotation -= mouseY;
        XRotation = Mathf.Clamp(XRotation, -90f, 90f);

        YRotation += mouseX;

        transform.rotation = Quaternion.Euler(XRotation, YRotation, 0);
    }
}
