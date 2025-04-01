using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReset : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    public float resetDelay = 1.0f;

    private void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    public void ResetToStartPoint()
    {
        Debug.Log("Resetting player to start point...");

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (TryGetComponent(out PlayMove playMove))
        {
            playMove.ResetToStartPoint();
            Invoke(nameof(EnablePlayerMovement), resetDelay);
        }

        Debug.Log("Player reset complete.");
    }

    private void EnablePlayerMovement()
    {
        if (TryGetComponent(out PlayMove playMove))
        {
            playMove.EnableSpeed();
        }

        Debug.Log("Player movement re-enabled.");
    }
}
