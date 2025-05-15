using UnityEngine;
using System.Collections;
using System;
public class SpeedBoostArea : MonoBehaviour
{
    public float speedBoostMultiplier = 2.5f;
    public float speedBoostFadeDuration = 0.5f;
    public float maxSpeed = 30f; 

    private Coroutine fadeCoroutine; 

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        var playerController = other.GetComponent<PlayerController>();

        if (playerController == null)
            return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        //Changes speed boost everytime bike runs on the ramp.
        var rand = new System.Random();
        speedBoostMultiplier = rand.Next(1, 5);

        ApplySpeedBoost(playerController);
        fadeCoroutine = StartCoroutine(FadeBackSpeed(playerController));
    }

    private void ApplySpeedBoost(PlayerController controller)
    {
        float currentSpeed = controller.GetSpeed();
        float newSpeed = currentSpeed * speedBoostMultiplier;

        if (newSpeed > maxSpeed)
            newSpeed = maxSpeed;

        controller.SetSpeed(newSpeed);
    }

    private IEnumerator FadeBackSpeed(PlayerController controller)
    {
        float elapsedTime = 0f;
        float startSpeed = controller.GetSpeed();
        float targetSpeed = controller.GetOriginalSpeed();

        while (elapsedTime < speedBoostFadeDuration)
        {
            float newSpeed = Mathf.Lerp(startSpeed, targetSpeed, elapsedTime / speedBoostFadeDuration);
            controller.SetSpeed(newSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        controller.SetSpeed(targetSpeed);
    }
}
