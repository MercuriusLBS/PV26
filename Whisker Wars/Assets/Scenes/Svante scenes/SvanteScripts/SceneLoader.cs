using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class SceneLoaderButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scene Settings")]
    [Tooltip("Name of the scene to load (must be added in Build Settings)")]
    public string sceneName = "MainMenu";

    [Header("Visual Feedback (Optional)")]
    [Tooltip("Color when hovering over the text")]
    public Color hoverColor = Color.yellow;

    private TMP_Text textMesh;
    private Color originalColor;

    void Start()
    {
        // Try to get either TextMeshProUGUI or TextMeshPro component
        textMesh = GetComponent<TMP_Text>();

        if (textMesh != null)
        {
            originalColor = textMesh.color;
            Debug.Log("TextMeshPro component found successfully on " + gameObject.name);
        }
        else
        {
            Debug.LogError("No TextMeshPro component found on " + gameObject.name);
        }

        // Check for EventSystem
        if (FindObjectOfType<EventSystem>() == null)
        {
            Debug.LogWarning("No EventSystem found in scene! Please add one: Right-click in Hierarchy > UI > Event System");
        }
    }

    // Called when the pointer clicks on the text
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Text clicked!");
        LoadScene();
    }

    // Called when the pointer enters the text area
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Mouse entered text area");
        if (textMesh != null)
        {
            textMesh.color = hoverColor;
        }
    }

    // Called when the pointer exits the text area
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Mouse exited text area");
        if (textMesh != null)
        {
            textMesh.color = originalColor;
        }
    }

    void LoadScene()
    {
        // Check if the scene exists in build settings
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.Log("Loading scene: " + sceneName);
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Scene '" + sceneName + "' not found! Make sure it's added to Build Settings.");
        }
    }
}