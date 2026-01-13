using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUI : MonoBehaviour
{
    [Header("Player UI")]
    [SerializeField] private Slider playerHealthBar;
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private TextMeshProUGUI playerNameText;

    [Header("Enemy UI")]
    [SerializeField] private Slider enemyHealthBar;
    [SerializeField] private TextMeshProUGUI enemyHealthText;
    [SerializeField] private TextMeshProUGUI enemyNameText;

    [Header("Main Action Menu")]
    [SerializeField] private GameObject mainActionMenuPanel;
    [SerializeField] private Button abilitiesButton;

    [Header("Abilities Submenu")]
    [SerializeField] private GameObject abilitiesSubmenuPanel;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button specialAttackButton;
    [SerializeField] private Button defenseButton;
    [SerializeField] private Button backButton; // Button to go back to main menu

    [Header("Battle Log")]
    [SerializeField] private TextMeshProUGUI battleLogText;

    private Battlemanager battleManager;

    private void Awake()
    {
        battleManager = FindFirstObjectByType<Battlemanager>();
        if (battleManager == null)
        {
            Debug.LogWarning("BattleManager not found! BattleUI may not work correctly.");
        }
    }

    private void Start()
    {
        // Connect main menu button
        if (abilitiesButton != null)
        {
            abilitiesButton.onClick.AddListener(OnAbilitiesButtonClicked);
        }
        else
        {
            Debug.LogWarning("Abilities button not assigned in BattleUI!");
        }

        // Connect ability buttons
        if (attackButton != null)
        {
            attackButton.onClick.AddListener(OnAttackButtonClicked);
        }
        else
        {
            Debug.LogWarning("Attack button not assigned in BattleUI!");
        }

        if (specialAttackButton != null)
        {
            specialAttackButton.onClick.AddListener(OnSpecialAttackButtonClicked);
        }
        else
        {
            Debug.LogWarning("Special Attack button not assigned in BattleUI!");
        }

        if (defenseButton != null)
        {
            defenseButton.onClick.AddListener(OnDefenseButtonClicked);
        }
        else
        {
            Debug.LogWarning("Defense button not assigned in BattleUI!");
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        // Initialize UI - start with main menu visible, submenu hidden
        if (mainActionMenuPanel != null)
        {
            mainActionMenuPanel.SetActive(true);
        }
        if (abilitiesSubmenuPanel != null)
        {
            abilitiesSubmenuPanel.SetActive(false);
        }

        // Initialize health bars
        if (battleManager != null && battleManager.Player != null && battleManager.Enemy != null)
        {
            UpdateHealthBars(battleManager.Player, battleManager.Enemy);
        }
    }

    public void UpdateHealthBars(Character player, Character enemy)
    {
        // Update player health bar
        if (playerHealthBar != null && player != null)
        {
            playerHealthBar.value = player.GetHealthPercentage();
        }

        if (playerHealthText != null && player != null)
        {
            playerHealthText.text = $"{player.CurrentHealth} / {player.MaxHealth}";
        }

        if (playerNameText != null && player != null)
        {
            playerNameText.text = player.CharacterName;
        }

        // Update enemy health bar
        if (enemyHealthBar != null && enemy != null)
        {
            enemyHealthBar.value = enemy.GetHealthPercentage();
        }

        if (enemyHealthText != null && enemy != null)
        {
            enemyHealthText.text = $"{enemy.CurrentHealth} / {enemy.MaxHealth}";
        }

        if (enemyNameText != null && enemy != null)
        {
            enemyNameText.text = enemy.CharacterName;
        }
    }

    public void SetActionMenuActive(bool active)
    {
        if (mainActionMenuPanel != null)
        {
            mainActionMenuPanel.SetActive(active);
        }

        if (abilitiesButton != null)
        {
            abilitiesButton.interactable = active;
        }

        // Hide submenu when disabling main menu
        if (!active && abilitiesSubmenuPanel != null)
        {
            abilitiesSubmenuPanel.SetActive(false);
        }
    }

    private void ShowMainMenu()
    {
        if (mainActionMenuPanel != null)
        {
            mainActionMenuPanel.SetActive(true);
        }
        if (abilitiesSubmenuPanel != null)
        {
            abilitiesSubmenuPanel.SetActive(false);
        }
    }

    private void ShowAbilitiesSubmenu()
    {
        if (mainActionMenuPanel != null)
        {
            mainActionMenuPanel.SetActive(false);
        }
        if (abilitiesSubmenuPanel != null)
        {
            abilitiesSubmenuPanel.SetActive(true);
        }
    }

    public void ShowBattleLog(string message)
    {
        if (battleLogText != null)
        {
            battleLogText.text = message;
        }
        else
        {
            Debug.Log($"Battle Log: {message}");
        }
    }

    private void OnAbilitiesButtonClicked()
    {
        ShowAbilitiesSubmenu();
    }

    private void OnBackButtonClicked()
    {
        ShowMainMenu();
    }

    private void OnAttackButtonClicked()
    {
        if (battleManager != null)
        {
            battleManager.PlayerAttack();
        }
    }

    private void OnSpecialAttackButtonClicked()
    {
        if (battleManager != null)
        {
            battleManager.PlayerSpecialAttack();
        }
    }

    private void OnDefenseButtonClicked()
    {
        if (battleManager != null)
        {
            battleManager.PlayerDefend();
        }
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        if (abilitiesButton != null)
        {
            abilitiesButton.onClick.RemoveListener(OnAbilitiesButtonClicked);
        }
        if (attackButton != null)
        {
            attackButton.onClick.RemoveListener(OnAttackButtonClicked);
        }
        if (specialAttackButton != null)
        {
            specialAttackButton.onClick.RemoveListener(OnSpecialAttackButtonClicked);
        }
        if (defenseButton != null)
        {
            defenseButton.onClick.RemoveListener(OnDefenseButtonClicked);
        }
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackButtonClicked);
        }
    }
}
