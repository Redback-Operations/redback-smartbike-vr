using UnityEngine;

public class WebMqttMovementController : MonoBehaviour
{
    [Header("Speed Mapping (Bike -> Game)")]
    [Tooltip("Max speed your bike/Pi can send (e.g. 40). Anything above will clamp.")]
    public float inputSpeedMax = 40f;

    [Tooltip("Max Unity world speed you allow (units/sec). Start small: 0.5 to 1.")]
    public float maxWorldSpeed = 1f;

    [Tooltip("Extra multiplier AFTER mapping. Use this to fine-tune.")]
    public float speedMultiplier = 1f;

    [Tooltip("Speeds smaller than this become 0 (prevents tiny drift).")]
    public float speedDeadzone = 0.05f;

    [Header("Turning")]
    [Tooltip("Degrees per second when turn = 1 or -1")]
    public float turnSpeed = 90f;

    [Header("Brake")]
    [Tooltip("When brake=true, target speed becomes 0 and we reach it in ~this time.")]
    public float brakeDurationSeconds = 1f;

    [Tooltip("Normal smooth accel/decel time when brake=false.")]
    public float normalSmoothSeconds = 0.15f;

    [Header("Debug")]
    public bool logSpeed = false;

    private float _currentSpeed = 0f;
    private float _speedVelocity = 0f;

    private void Update()
    {
        if (Mqtt.Instance == null) return;
        if (!Mqtt.Instance.IsConnected) return;

        // RAW input from MQTT
        float rawSpeed = Mathf.Max(0f, Mqtt.Instance.WebSpeed);
        int turn = Mqtt.Instance.WebTurn;
        bool braking = Mqtt.Instance.WebBrake;

        // ---- MAP RAW SPEED -> GAME SPEED ----
        // normalize 0..inputSpeedMax -> 0..1
        float t = (inputSpeedMax <= 0.0001f) ? 0f : Mathf.Clamp01(rawSpeed / inputSpeedMax);

        // map to 0..maxWorldSpeed then apply multiplier
        float commandedSpeed = (t * maxWorldSpeed) * speedMultiplier;

        // deadzone
        if (commandedSpeed < speedDeadzone) commandedSpeed = 0f;

        // if braking, force target speed to 0
        float targetSpeed = braking ? 0f : commandedSpeed;

        // IMPORTANT: if brake released, kill SmoothDamp momentum so it stops decelerating immediately
        if (!braking)
            _speedVelocity = 0f;

        float smoothTime = braking ? brakeDurationSeconds : normalSmoothSeconds;

        _currentSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _speedVelocity, smoothTime);

        // movement
        transform.Translate(transform.forward * _currentSpeed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime);

        if (logSpeed)
        {
            Debug.Log($"WebMove: raw={rawSpeed} mapped={commandedSpeed:F3} current={_currentSpeed:F3} brake={braking} turn={turn}");
        }
    }
}
