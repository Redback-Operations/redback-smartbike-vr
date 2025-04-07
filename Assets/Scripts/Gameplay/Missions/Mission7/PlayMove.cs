using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayMove : MonoBehaviour
{
    public float defaultmaxSpeed = 10.0f;
    public float maxReverseSpeed = -5.0f;
    public float acceleration = 5.0f;
    public float deceleration = 3.0f;
    public float turnspeed = 100.0f;

    public float currentSpeed = 0.0f;
    public float speedIncreasePerPoint = 0.5f;

    public Text speedText;

    private bool isSpeedReset = false;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float _maxSpeed;

    private void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        _maxSpeed = defaultmaxSpeed;

        UpdateSpeedText();
    }

    private void Update()
    {
        if (isSpeedReset)
        {
            currentSpeed = 0.0f;
            UpdateSpeedText();
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Accelerate forward
        if (vertical > 0)
        {
            currentSpeed += acceleration * vertical * Time.deltaTime;
        }
        // Accelerate backward
        else if (vertical < 0)
        {
            currentSpeed += acceleration * vertical * Time.deltaTime;
        }
        // Decelerate when no input
        else
        {
            if (currentSpeed > 0)
            {
                currentSpeed -= deceleration * Time.deltaTime;
                currentSpeed = Mathf.Max(currentSpeed, 0);
            }
            else if (currentSpeed < 0)
            {
                currentSpeed += deceleration * Time.deltaTime;
                currentSpeed = Mathf.Min(currentSpeed, 0);
            }
        }

        // Clamp speed between max forward and max reverse speeds
        currentSpeed = Mathf.Clamp(currentSpeed, maxReverseSpeed, _maxSpeed);

        transform.position += currentSpeed * Time.deltaTime * transform.forward;
        transform.rotation *= Quaternion.AngleAxis(turnspeed * horizontal * Time.deltaTime, transform.up);

        UpdateSpeedText();
    }

    private void UpdateSpeedText()
    {
        if (speedText != null)
        {
            speedText.text = $"Speed: {currentSpeed:F1}";
        }
    }

    public void ResetToStartPoint()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        currentSpeed = 0f;
        isSpeedReset = true;

        UpdateSpeedText();

        Invoke(nameof(EnableSpeed), 1.0f);
        Debug.Log("Player reset to start point and speed reset to 0.");
    }

    public void EnableSpeed()
    {
        isSpeedReset = false;
        Debug.Log("Speed reset disabled, player can move again.");
    }

    public void IncreaseMaxSpeed()
    {
        _maxSpeed += speedIncreasePerPoint;
        Debug.Log($"Max speed increased to: {_maxSpeed}");
    }
}
