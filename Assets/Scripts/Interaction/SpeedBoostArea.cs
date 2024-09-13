using UnityEngine;
using System.Collections;
public class SpeedBoostArea : MonoBehaviour
{
    public float speedBoostMultiplier = 2.5f;
    public float speedBoostFadeDuration = 0.5f;
    public float maxSpeed = 30f; 

    private PlayerController playerController;
    private Coroutine fadeCoroutine; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                }

                ApplySpeedBoost();
                fadeCoroutine = StartCoroutine(FadeBackSpeed());

                //Added by Delia - to change up the speed boost ramp for speeds up or slow downs.
                var rand = new System.Random();
                speedBoostMultiplier = rand.Next(1, 10);
            }
        }
    }

    private void ApplySpeedBoost()
    {
        float currentSpeed = playerController.GetSpeed();
        float newSpeed = currentSpeed * speedBoostMultiplier;

        if (newSpeed > maxSpeed)
        {
            newSpeed = maxSpeed;
        }

        playerController.SetSpeed(newSpeed);
    }

    private IEnumerator FadeBackSpeed()
    {
        float elapsedTime = 0f;
        float startSpeed = playerController.GetSpeed();
        float targetSpeed = playerController.GetOriginalSpeed();

        while (elapsedTime < speedBoostFadeDuration)
        {
            float newSpeed = Mathf.Lerp(startSpeed, targetSpeed, elapsedTime / speedBoostFadeDuration);
            playerController.SetSpeed(newSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerController.SetSpeed(targetSpeed);
    }
}
