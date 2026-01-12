using UnityEngine;

public class QuitButton : MonoBehaviour
{
    // Call this method from your button's OnClick event
    public void QuitToDesktop()
    {
        Debug.Log("Quitting to Desktop...");

        // Resume time in case game is paused
        Time.timeScale = 1f;

        // Quit the application
        Application.Quit();

        // This line makes it work in the Unity Editor for testing
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}