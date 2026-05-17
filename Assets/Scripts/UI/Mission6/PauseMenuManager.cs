using UnityEngine;
using Fusion;

// Handles the pause menu functionality.
public class PauseMenuManager : MonoBehaviour
{
    [Header("UI")]

    // Reference to the pause menu panel UI.
    [SerializeField] private GameObject pauseMenuUI;

    [Header("Scene Names")]

    // Name of the gameplay scene containing Mission 6.
    [SerializeField] private string citySceneName = "CityScene";

    // Name of the avatar selection / main menu scene.
    [SerializeField] private string avatarSelectionSceneName = "AvatarSelection";

    [Header("Mission Settings")]

    // Mission number used when restarting the level.
    [SerializeField] private int missionNumber = 6;

    // Tracks whether the game is currently paused.
    private bool isPaused = false;

    private void Start()
    {
        // Hide the pause menu when the scene starts.
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        // Ensure the game is running normally.
        Time.timeScale = 1f;
    }

    private void Update()
    {
        // Toggle pause menu when ESC is pressed.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    // Opens the pause menu and pauses the game.
    public void PauseGame()
    {
        isPaused = true;

        // Show the pause menu UI.
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        // Pause the game.
        Time.timeScale = 0f;
    }

    // Closes the pause menu and resumes the game.
    public void ResumeGame()
    {
        isPaused = false;

        // Hide the pause menu UI.
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        // Resume normal game time.
        Time.timeScale = 1f;
    }

    // Restarts the current mission.
    public void RestartLevel()
    {
        // Save the mission number for the mission system.
        PlayerPrefs.SetInt("MissionNumber", missionNumber);
        PlayerPrefs.Save();

        // Shutdown Fusion and reload the gameplay scene.
        CleanupAndLoad(citySceneName);
    }

    // Returns to the avatar selection / main menu scene.
    public void GoToMainMenu()
    {
        // Shutdown Fusion and load the avatar selection scene.
        CleanupAndLoad(avatarSelectionSceneName);
    }

    // Properly shuts down Fusion before changing scenes.
    private async void CleanupAndLoad(string sceneName)
    {
        // Resume normal game time before leaving the scene.
        Time.timeScale = 1f;

        // Reset the score UI back to 0.
        if (UIManager.Instance != null)
            UIManager.Instance.SetScore(0);

        // Find the Fusion network runner.
        NetworkRunner runner = FindObjectOfType<NetworkRunner>();

        // Properly shut down Fusion networking.
        if (runner != null)
            await runner.Shutdown();

        // Load the target scene.
        MapLoader.LoadScene(sceneName);
    }
}