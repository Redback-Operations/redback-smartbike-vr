using UnityEngine;
using Fusion;

// Handles restarting levels and returning to avatar selection.
public class EndScreenManager : MonoBehaviour
{
    [Header("Scene Names")]

    // Gameplay scene containing Mission 6.
    [SerializeField] private string citySceneName = "CityScene";

    // Avatar selection / menu scene.
    [SerializeField] private string avatarSelectionSceneName = "AvatarSelection";

    [Header("Mission Settings")]

    // Mission number used when restarting.
    [SerializeField] private int missionNumber = 6;

    // Restart Mission 6.
    public void RestartLevel()
    {
        PlayerPrefs.SetInt("MissionNumber", missionNumber);
        PlayerPrefs.Save();

        CleanupAndLoad(citySceneName);
    }

    // Return to avatar selection scene.
    public void GoToMainMenu()
    {
        CleanupAndLoad(avatarSelectionSceneName);
    }

    // Properly shuts down Fusion before loading another scene.
    private async void CleanupAndLoad(string sceneName)
    {
        // Resume game time.
        Time.timeScale = 1f;

        // Reset score UI.
        if (UIManager.Instance != null)
            UIManager.Instance.SetScore(0);

        // Find Fusion runner.
        NetworkRunner runner = FindObjectOfType<NetworkRunner>();

        // Properly shut down networking session.
        if (runner != null)
            await runner.Shutdown();

        // Load the target scene.
        MapLoader.LoadScene(sceneName);
    }
}