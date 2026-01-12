using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Pause Menu Settings")]
    [Tooltip("The name of the pause menu scene (must be in Build Settings)")]
    public string pauseMenuSceneName = "PauseMenu";

    private bool isPaused = false;

    void Update()
    {
        // Check if Escape key is pressed using new Input System
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        // Load the pause menu scene additively (on top of current scene)
        if (!string.IsNullOrEmpty(pauseMenuSceneName))
        {
            SceneManager.LoadScene(pauseMenuSceneName, LoadSceneMode.Additive);
        }
        else
        {
            Debug.LogError("Pause Menu Scene Name is not set!");
            return;
        }

        // Freeze the game
        Time.timeScale = 0f;
        isPaused = true;

        Debug.Log("Game Paused - Pause Menu Scene Loaded");
    }

    public void ResumeGame()
    {
        // Unload the pause menu scene
        if (SceneManager.GetSceneByName(pauseMenuSceneName).isLoaded)
        {
            SceneManager.UnloadSceneAsync(pauseMenuSceneName);
        }

        // Resume the game
        Time.timeScale = 1f;
        isPaused = false;

        Debug.Log("Game Resumed - Pause Menu Scene Unloaded");
    }

    public void QuitToMainMenu(string mainMenuSceneName)
    {
        // Resume time before loading new scene
        Time.timeScale = 1f;
        isPaused = false;

        // Load main menu (this will unload all scenes and load just the main menu)
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");

        // Resume time before quitting
        Time.timeScale = 1f;

        Application.Quit();

        // This is for testing in the Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
