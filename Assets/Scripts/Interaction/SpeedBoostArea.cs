using UnityEngine;
using System.Collections;

public class SpeedBoostArea : MonoBehaviour
{
    [SerializeField] private float speedBoostMultiplier = 2.5f;
    [SerializeField] private float boostDuration = 3f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float maxSpeed = 30f;

    private Coroutine boostCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        if (boostCoroutine != null)
            StopCoroutine(boostCoroutine);

        boostCoroutine = StartCoroutine(BoostRoutine(player));
    }

    private IEnumerator BoostRoutine(PlayerController player)
    {
        float originalSpeed = player.GetOriginalSpeed();
        float boostedSpeed = Mathf.Min(originalSpeed * speedBoostMultiplier, maxSpeed);

        player.SetSpeed(boostedSpeed);
        Debug.Log($"BOOST START | Speed: {boostedSpeed}");

        yield return new WaitForSeconds(boostDuration);

        float startSpeed = player.GetSpeed();
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            float newSpeed = Mathf.Lerp(startSpeed, originalSpeed, elapsed / fadeDuration);
            player.SetSpeed(newSpeed);

            elapsed += Time.deltaTime;
            yield return null;
        }

        player.SetSpeed(originalSpeed);
        Debug.Log("BOOST END");
    }
}