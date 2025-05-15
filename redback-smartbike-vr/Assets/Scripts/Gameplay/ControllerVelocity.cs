using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ControllerVelocity : MonoBehaviour
{

    public InputActionProperty velocityProperty;

    public Vector3 velocity { get; private set; } = Vector3.zero;

    private GameObject skater;
    private Rigidbody skaterRb;
    private TextMeshPro debugText;

    // Start is called before the first frame update
    void Start()
    {
        skater = GameObject.FindWithTag("skater");
        skaterRb = skater.GetComponent<Rigidbody>();
        debugText = GameObject.FindWithTag("debugtext").GetComponent<TextMeshPro>();
        debugText.SetText("0");
    }

    // Update is called once per frame
    void Update()
    {
        velocity = velocityProperty.action.ReadValue<Vector3>();
        skaterRb.AddForce(velocity.magnitude * 2000 * skater.transform.forward);

        string readout = velocity.magnitude.ToString();
        if(readout.Length > 4){
            readout = readout.Substring(0,5);
        }
        debugText.SetText(readout);
    }
}
