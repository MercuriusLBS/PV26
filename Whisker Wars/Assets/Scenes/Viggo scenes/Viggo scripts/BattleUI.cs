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

    [Header("Action Menu")]
    [SerializeField] private GameObject actionMenuPanel;
    [SerializeField] private Button attackButton;

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
        // Connect attack button to BattleManager
        if (attackButton != null)
        {
            attackButton.onClick.AddListener(OnAttackButtonClicked);
        }
        else
        {
            Debug.LogWarning("Attack button not assigned in BattleUI!");
        }

        // Initialize UI
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
        if (actionMenuPanel != null)
        {
            actionMenuPanel.SetActive(active);
        }

        if (attackButton != null)
        {
            attackButton.interactable = active;
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

    private void OnAttackButtonClicked()
    {
        if (battleManager != null)
        {
            battleManager.PlayerAttack();
        }
    }

    private void OnDestroy()
    {
        // Clean up button listener
        if (attackButton != null)
        {
            attackButton.onClick.RemoveListener(OnAttackButtonClicked);
        }
    }
}
