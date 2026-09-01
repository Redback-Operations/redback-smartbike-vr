using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Mission6 : Mission
{
    public override int MissionNumber => 6;
    public override string MissionName => "Star Rush";

    [Header("Mission UI")]
    public string missionName;
    public TextMeshProUGUI missionNameText;
    public Text timerText;
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI missionStatusText;

    [Header("Ordered Collectibles")]
    [SerializeField] private Collectable[] orderedCollectables;

    [Header("Mission Settings")]
    [SerializeField] private float startingTime = 30f;
    [SerializeField] private float timeBonusPerCollectible = 5f;

    private int points;
    private float remainingTime;
    private bool missionCompletion;
    private int currentCollectibleIndex;

    private void Awake()
    {
        missionCompletion = false;
        points = 0;
        currentCollectibleIndex = 0;
        remainingTime = startingTime;

        // Register every collectible, including inactive objects.
        Collectable[] collectables =
            GetComponentsInChildren<Collectable>(true);

        foreach (Collectable collectable in collectables)
        {
            collectable.Register(Collect);
        }

        PrepareCollectibles();
        UpdateUI();

        StartCoroutine(CountdownTimer());
    }

    private void PrepareCollectibles()
    {
        if (orderedCollectables == null ||
            orderedCollectables.Length == 0)
        {
            Debug.LogWarning(
                "Mission 6 has no ordered collectibles assigned.");

            return;
        }

        // Hide all collectibles first.
        foreach (Collectable collectable in orderedCollectables)
        {
            if (collectable != null)
            {
                collectable.gameObject.SetActive(false);
            }
        }

        // Only show the first required collectible.
        if (orderedCollectables[0] != null)
        {
            orderedCollectables[0].gameObject.SetActive(true);
        }
    }

    private void Collect(Collectable item)
    {
        if (missionCompletion ||
            item == null ||
            orderedCollectables == null ||
            currentCollectibleIndex >= orderedCollectables.Length)
        {
            return;
        }

        // Only accept the collectible currently required.
        if (item != orderedCollectables[currentCollectibleIndex])
        {
            Debug.Log(
                $"Wrong collectible. Expected: " +
                $"{orderedCollectables[currentCollectibleIndex].name}");

            return;
        }

        points += item.Value;
        remainingTime += timeBonusPerCollectible;

        Debug.Log(
            $"Collected: {item.gameObject.name} | " +
            $"Points: {points}");

        currentCollectibleIndex++;

        // Mission completed.
        if (currentCollectibleIndex >= orderedCollectables.Length)
        {
            missionCompletion = true;
            UpdateUI();
            EndMission(true);
            return;
        }

        // Activate the next collectible.
        if (orderedCollectables[currentCollectibleIndex] != null)
        {
            orderedCollectables[currentCollectibleIndex]
                .gameObject.SetActive(true);
        }

        UpdateUI();
    }

    private void Update()
    {
        if (remainingTime <= 0f && !missionCompletion)
        {
            remainingTime = 0f;
            missionCompletion = true;

            UpdateUI();
            EndMission(false);
        }
    }

    private void UpdateUI()
    {
        if (missionNameText != null)
        {
            if (!missionCompletion &&
                orderedCollectables != null &&
                currentCollectibleIndex < orderedCollectables.Length)
            {
                missionNameText.text =
                    "Mission: " + missionName +
                    "\nNext: " +
                    orderedCollectables[currentCollectibleIndex].name;
            }
            else
            {
                missionNameText.text =
                    "Mission: " + missionName;
            }
        }

        if (timerText != null)
        {
            timerText.text =
                "Time: " + Mathf.Ceil(remainingTime) + "s";
        }

        if (pointsText != null)
        {
            int total =
                orderedCollectables != null
                    ? orderedCollectables.Length
                    : 0;

            pointsText.text =
                $"Collected: {currentCollectibleIndex}/{total}" +
                $"\nPoints: {points}";
        }
    }

    private IEnumerator CountdownTimer()
    {
        while (remainingTime > 0f && !missionCompletion)
        {
            yield return new WaitForSeconds(1f);

            remainingTime -= 1f;

            if (remainingTime < 0f)
            {
                remainingTime = 0f;
            }

            UpdateUI();
        }
    }

    private void EndMission(bool success)
    {
        Debug.Log(
            $"Mission ended. Success: {success} | " +
            $"Points: {points} | " +
            $"Remaining Time: {remainingTime}");

        if (missionStatusText != null)
        {
            if (success)
            {
                missionStatusText.text =
                    $"Mission Complete!\nFinal Points: {points}";
            }
            else
            {
                missionStatusText.text =
                    $"Time's Up!\nFinal Points: {points}";
            }

            missionStatusText.gameObject.SetActive(true);

            StartCoroutine(HideMissionStatusText());
        }

        if (missionNameText != null)
        {
            missionNameText.gameObject.SetActive(false);
        }

        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }

        if (pointsText != null)
        {
            pointsText.gameObject.SetActive(false);
        }
    }

    private IEnumerator HideMissionStatusText()
    {
        yield return new WaitForSeconds(3f);

        if (missionStatusText != null)
        {
            missionStatusText.gameObject.SetActive(false);
        }
    }
}