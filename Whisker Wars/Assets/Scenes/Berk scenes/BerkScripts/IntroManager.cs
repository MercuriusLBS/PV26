using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Manages the intro dialogue sequence with branching choices
/// </summary>
public class IntroManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private Button choice1Button;
    [SerializeField] private Button choice2Button;
    [SerializeField] private Button choice3Button;
    [SerializeField] private Button continueButton;

    [Header("Scene Settings")]
    [SerializeField] private string mainGameSceneName = "MainGame";
    [SerializeField] private float textSpeed = 0.05f;

    private PlayerAbilityData abilityData;
    private bool isTyping = false;
    private string currentFullText = "";

    void Start()
    {
        // Find or create PlayerAbilityData
        abilityData = PlayerAbilityData.Instance;
        if (abilityData == null)
        {
            GameObject dataObj = new GameObject("PlayerAbilityData");
            abilityData = dataObj.AddComponent<PlayerAbilityData>();
        }

        // Reset data for new intro
        abilityData.ResetData();

        // Setup button listeners
        choice1Button.onClick.AddListener(() => OnChoiceSelected(1));
        choice2Button.onClick.AddListener(() => OnChoiceSelected(2));
        if (choice3Button != null)
            choice3Button.onClick.AddListener(() => OnChoiceSelected(3));
        continueButton.onClick.AddListener(OnContinuePressed);

        // Start the intro
        StartIntro();
    }

    void StartIntro()
    {
        ShowDialogue(
            "You are born with shining talent towards combat amongst your peers. How will you spend your days?",
            "Rely on your talent and goof around not training a single day in your life.",
            "Train restlessly polishing your talent further."
        );
    }

    void ShowDialogue(string text, string choice1Text, string choice2Text, string choice3Text = null)
    {
        StopAllCoroutines();
        StartCoroutine(TypeText(text));

        choicesPanel.SetActive(true);
        continueButton.gameObject.SetActive(false);

        choice1Button.GetComponentInChildren<TextMeshProUGUI>().text = choice1Text;
        choice2Button.GetComponentInChildren<TextMeshProUGUI>().text = choice2Text;

        if (choice3Text != null && choice3Button != null)
        {
            choice3Button.gameObject.SetActive(true);
            choice3Button.GetComponentInChildren<TextMeshProUGUI>().text = choice3Text;
        }
        else if (choice3Button != null)
        {
            choice3Button.gameObject.SetActive(false);
        }
    }

    void ShowNarrative(string text, System.Action onContinue)
    {
        StopAllCoroutines();
        StartCoroutine(TypeText(text));

        choicesPanel.SetActive(false);
        continueButton.gameObject.SetActive(true);
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() => onContinue?.Invoke());
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        currentFullText = text;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    void OnContinuePressed()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = currentFullText;
            isTyping = false;
        }
    }

    void OnChoiceSelected(int choiceNumber)
    {
        // Disable buttons while processing
        choice1Button.interactable = false;
        choice2Button.interactable = false;
        if (choice3Button != null)
            choice3Button.interactable = false;

        ProcessChoice(choiceNumber);
    }

    void ProcessChoice(int choice)
    {
        // First choice branching
        if (dialogueText.text.Contains("shining talent towards combat"))
        {
            if (choice == 1)
            {
                // Lazy path - grant Confidence
                abilityData.GrantAbility("Confidence");
                ShowLazyPath();
            }
            else if (choice == 2)
            {
                // Training path
                ShowTrainingPath();
            }
        }
        // Lazy path - mission preparation choice
        else if (dialogueText.text.Contains("How do you prepare for the mission"))
        {
            if (choice == 1)
            {
                // Stock up on healing items
                abilityData.AddHerbs(2);
                LoadMainGame();
            }
            else if (choice == 2)
            {
                // Rush straight to mission
                abilityData.GrantAbility("Rush");
                LoadMainGame();
            }
        }
        // Training path - specialization choice
        else if (dialogueText.text.Contains("pick what your specialisation will be"))
        {
            if (choice == 1)
            {
                // Focus on defence
                abilityData.GrantAbility("FleetFootwork");
                ShowTrainingFinalChoice();
            }
            else if (choice == 2)
            {
                // Focus on attack
                abilityData.GrantAbility("PiercingStrike");
                ShowTrainingFinalChoice();
            }
        }
        // Training path - final choice
        else if (dialogueText.text.Contains("What do you do with your remaining days"))
        {
            if (choice == 1)
            {
                // Train extra hard
                abilityData.GrantAbility("CriticalStrike");
                LoadMainGame();
            }
            else if (choice == 2)
            {
                // Stock up on healing items
                abilityData.AddHerbs(2);
                LoadMainGame();
            }
            else if (choice == 3)
            {
                // Rush straight to mission
                abilityData.GrantAbility("Rush");
                LoadMainGame();
            }
        }
    }

    void ShowLazyPath()
    {
        ShowNarrative(
            "You spend your days goofing around and relying purely on your innate talent. This doesn't take you far in life, eventually your laziness brings disgrace to your family which leads to them kicking you out. You don't mind this because in your mind you're still the prodigy everyone praised you to be. This leads you to become homeless and running out of money.",
            () => ShowLazyPathContinued()
        );
    }

    void ShowLazyPathContinued()
    {
        ShowNarrative(
            "One day you found this recruitment poster to take over another planet to further progress your kind. This job pays well and it will surely prove to your parents and everyone else that you truly are a prodigy. How do you prepare for the mission?",
            () => ShowLazyMissionChoice()
        );
    }

    void ShowLazyMissionChoice()
    {
        ShowDialogue(
            "How do you prepare for the mission?",
            "Stock up on healing items.",
            "Rush straight to the mission."
        );
    }

    void ShowTrainingPath()
    {
        ShowNarrative(
            "You train relentlessly, using your days to the fullest. You polish this innate talent that already was there into something further, proving you truly are a prodigy. You've grown into a fine knight. Now you have to pick what your specialisation will be.",
            () => ShowSpecializationChoice()
        );
    }

    void ShowSpecializationChoice()
    {
        ShowDialogue(
            "Now you have to pick what your specialisation will be.",
            "Focus on your defence.",
            "Focus on your attack."
        );
    }

    void ShowTrainingFinalChoice()
    {
        ShowNarrative(
            "One day whilst you're training your combat trainer comes to you and tells you about this mission to take over another planet to further progress your kind. He further goes to ask you if you'd be interested, without any doubt in your mind you agree to it. What do you do with your remaining days?",
            () => ShowFinalChoice()
        );
    }

    void ShowFinalChoice()
    {
        ShowDialogue(
            "What do you do with your remaining days?",
            "Train extra hard.",
            "Stock up on healing items.",
            "Rush straight to the mission."
        );
    }

    void LoadMainGame()
    {
        Debug.Log("[IntroManager] Loading main game scene...");
        SceneManager.LoadScene(mainGameSceneName);
    }
}