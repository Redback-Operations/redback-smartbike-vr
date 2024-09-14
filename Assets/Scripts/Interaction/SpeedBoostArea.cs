using UnityEngine;

public class SpeedBoostArea : MonoBehaviour
{
    public float speedBoostMultiplier = 2f; 
    public float speedBoostFadeDuration = 1f;

    private float originalSpeed; 
    private PlayerController playerController;

    private void OnTriggerEnter(Collider other)
    {
       if (other.CompareTag("Player"))
        {
            playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                originalSpeed = playerController.GetSpeed(); 
                playerController.SetSpeed(originalSpeed * speedBoostMultiplier); 
                StartCoroutine(FadeBackSpeed()); 
            }
        }
    }


    private System.Collections.IEnumerator FadeBackSpeed()
    {
        float elapsedTime = 0f;
        float startSpeed = playerController.GetSpeed();
        float targetSpeed = originalSpeed;

        // Loop until the fade duration is reached
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
