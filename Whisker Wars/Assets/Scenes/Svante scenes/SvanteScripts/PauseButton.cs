using UnityEngine;

/// <summary>
/// Attach this to a GameObject in your PauseMenu scene
/// Connect your buttons to call these public methods
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    private PauseMenuManager pauseMenuManager;

    void Start()
    {
        // Find the PauseMenuManager in the loaded scenes
        pauseMenuManager = FindFirstObjectByType<PauseMenuManager>();

        if (pauseMenuManager == null)
        {
            Debug.LogError("PauseMenuManager not found! Make sure it exists in your game scene.");
        }
    }

    // Connect this to your Resume button's OnClick event
    public void OnResumeButtonClicked()
    {
        if (pauseMenuManager != null)
        {
            pauseMenuManager.ResumeGame();
        }
    }

    // Connect this to your Quit to Main Menu button's OnClick event
    public void OnQuitToMainMenuClicked()
    {
        if (pauseMenuManager != null)
        {
            pauseMenuManager.QuitToMainMenu("MainMenu"); // Replace "MainMenu" with your actual main menu scene name
        }
    }

    // Connect this to your Quit Game button's OnClick event
    public void OnQuitGameClicked()
    {
        if (pauseMenuManager != null)
        {
            pauseMenuManager.QuitGame();
        }
    }
}