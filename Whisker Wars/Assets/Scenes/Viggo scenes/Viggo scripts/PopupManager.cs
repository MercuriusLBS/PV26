using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Shows tutorial/info popups (e.g. first defeat whisker info, 10 whiskers milestone).
/// Add this to a GameObject that persists across scenes (e.g. same as EncounterManager or its own DontDestroyOnLoad object).
/// Assign the UI references in the Inspector — see POPUP_SETUP_TUTORIAL.md for step-by-step setup.
/// </summary>
public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    [Header("UI References (assign in Inspector)")]
    [Tooltip("The panel that contains the popup (will be shown/hidden).")]
    [SerializeField] private GameObject popupPanel;
    [Tooltip("TextMeshProUGUI that displays the popup message.")]
    [SerializeField] private TextMeshProUGUI messageText;
    [Tooltip("Optional. If set, popup closes when clicked. If not set, uses auto-close after duration.")]
    [SerializeField] private Button continueButton;

    [Header("Settings")]
    [Tooltip("Seconds before popup auto-closes. Always runs as fallback; button can close earlier.")]
    [SerializeField] private float displayDuration = 4f;

    [Header("First-time popup messages (edit as needed)")]
    [SerializeField] [TextArea(2, 4)] private string firstDefeatWhiskerMessage = "Enemies drop <b>Whiskers</b> when defeated. Collect them — they can be used later!";
    [SerializeField] [TextArea(2, 4)] private string tenWhiskersMessage = "You've collected 10 Whiskers! Keep fighting to gather more.";

    private bool hasShownFirstDefeatPopup;
    private bool hasShownTenWhiskersPopup;
    private Coroutine autoCloseCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (popupPanel != null)
                popupPanel.SetActive(false);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(HidePopup);
    }

    /// <summary>
    /// Show a generic popup with the given message.
    /// </summary>
    public void ShowPopup(string message)
    {
        if (popupPanel == null || messageText == null)
        {
            Debug.LogWarning("[PopupManager] popupPanel or messageText not assigned. Cannot show popup.");
            return;
        }

        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }

        messageText.text = message;
        popupPanel.SetActive(true);

        // Always run auto-close so the popup never stays forever (button can still close earlier)
        float duration = displayDuration > 0f ? displayDuration : 4f;
        autoCloseCoroutine = StartCoroutine(AutoCloseAfterSeconds(duration));
    }

    /// <summary>
    /// Hide the popup panel.
    /// </summary>
    public void HidePopup()
    {
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    private IEnumerator AutoCloseAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        autoCloseCoroutine = null;
        HidePopup();
    }

    /// <summary>
    /// Show the "first time you defeated an enemy — here's what whiskers are" popup.
    /// Only shows once per session. Call from EncounterManager when returning to overworld after first victory.
    /// </summary>
    public void TryShowFirstDefeatWhiskerInfo()
    {
        if (hasShownFirstDefeatPopup) return;
        hasShownFirstDefeatPopup = true;
        ShowPopup(firstDefeatWhiskerMessage);
    }

    /// <summary>
    /// Show the "you reached 10 whiskers" milestone popup.
    /// Only shows once per session. Call from BattleLootDropper after awarding whiskers when count >= 10.
    /// </summary>
    public void TryShowTenWhiskersMilestone()
    {
        if (hasShownTenWhiskersPopup) return;
        hasShownTenWhiskersPopup = true;
        ShowPopup(tenWhiskersMessage);
    }

    /// <summary>
    /// Whether the first-defeat popup has already been shown this session.
    /// </summary>
    public bool HasShownFirstDefeatPopup => hasShownFirstDefeatPopup;

    /// <summary>
    /// Whether the 10-whiskers popup has already been shown this session.
    /// </summary>
    public bool HasShownTenWhiskersPopup => hasShownTenWhiskersPopup;
}
