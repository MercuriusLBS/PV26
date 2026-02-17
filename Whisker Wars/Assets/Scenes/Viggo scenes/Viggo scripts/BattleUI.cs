using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUI : MonoBehaviour
{
    [Header("Player UI")]
    // Filled Image inside the player health bar border
    [SerializeField] private Image playerHealthFill;
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private TextMeshProUGUI playerNameText;

    [Header("Enemy UI")]
    // Filled Image inside the enemy health bar border
    [SerializeField] private Image enemyHealthFill;
    [SerializeField] private TextMeshProUGUI enemyHealthText;
    [SerializeField] private TextMeshProUGUI enemyNameText;

    [Header("Main Action Menu")]
    [SerializeField] private GameObject mainActionMenuPanel;
    [SerializeField] private Button abilitiesButton;
    [SerializeField] private Button itemsButton;

    [Header("Items Submenu")]
    [SerializeField] private GameObject itemsSubmenuPanel;
    [SerializeField] private Transform itemsListParent;
    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private Button itemsBackButton;

    [Header("Abilities Submenu")]
    [SerializeField] private GameObject abilitiesSubmenuPanel;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button specialAttackButton;
    [SerializeField] private Button defenseButton;
    [SerializeField] private Button backButton; // Button to go back to main menu

    [Header("Battle Log")]
    [SerializeField] private TextMeshProUGUI battleLogText;
    [SerializeField] private float battleLogDisplayTime = 2f; // Seconds before log is cleared

    private Battlemanager battleManager;
    private Coroutine clearLogCoroutine;

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
        // Connect main menu buttons
        if (abilitiesButton != null)
        {
            abilitiesButton.onClick.AddListener(OnAbilitiesButtonClicked);
        }
        else
        {
            Debug.LogWarning("Abilities button not assigned in BattleUI!");
        }

        if (itemsButton != null)
        {
            itemsButton.onClick.AddListener(OnItemsButtonClicked);
        }
        else
        {
            Debug.LogWarning("Items button not assigned in BattleUI!");
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

        if (itemsBackButton != null)
        {
            itemsBackButton.onClick.AddListener(OnBackFromItemsButtonClicked);
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

        if (itemsSubmenuPanel != null)
        {
            itemsSubmenuPanel.SetActive(false);
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
        if (playerHealthFill != null && player != null)
        {
            // Assumes GetHealthPercentage returns a value between 0 and 1
            playerHealthFill.fillAmount = player.GetHealthPercentage();
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
        if (enemyHealthFill != null && enemy != null)
        {
            // Assumes GetHealthPercentage returns a value between 0 and 1
            enemyHealthFill.fillAmount = enemy.GetHealthPercentage();
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

        if (itemsButton != null)
        {
            itemsButton.interactable = active;
        }

        // Hide submenus when disabling main menu
        if (!active && abilitiesSubmenuPanel != null)
        {
            abilitiesSubmenuPanel.SetActive(false);
        }

        if (!active && itemsSubmenuPanel != null)
        {
            itemsSubmenuPanel.SetActive(false);
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

        if (itemsSubmenuPanel != null)
        {
            itemsSubmenuPanel.SetActive(false);
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

        if (itemsSubmenuPanel != null)
        {
            itemsSubmenuPanel.SetActive(false);
        }
    }

    private void ShowItemsSubmenu()
    {
        if (mainActionMenuPanel != null)
        {
            mainActionMenuPanel.SetActive(false);
        }

        if (abilitiesSubmenuPanel != null)
        {
            abilitiesSubmenuPanel.SetActive(false);
        }

        if (itemsSubmenuPanel != null)
        {
            itemsSubmenuPanel.SetActive(true);
        }

        PopulateItemsList();
    }

    public void ShowBattleLog(string message)
    {
        if (battleLogText != null)
        {
            battleLogText.text = message;

            // Restart auto-clear timer
            if (clearLogCoroutine != null)
            {
                StopCoroutine(clearLogCoroutine);
            }
            clearLogCoroutine = StartCoroutine(ClearBattleLogAfterDelay());
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

    private void OnItemsButtonClicked()
    {
        ShowItemsSubmenu();
    }

    private void OnBackButtonClicked()
    {
        ShowMainMenu();
    }

    private void OnBackFromItemsButtonClicked()
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

    /// <summary>
    /// Populates the items submenu with buttons for each battle-usable item.
    /// </summary>
    private void PopulateItemsList()
    {
        if (itemsListParent == null || itemButtonPrefab == null)
        {
            Debug.LogWarning("Items list parent or item button prefab is not assigned in BattleUI.");
            return;
        }

        // Clear existing children
        for (int i = itemsListParent.childCount - 1; i >= 0; i--)
        {
            Destroy(itemsListParent.GetChild(i).gameObject);
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("InventoryManager.Instance is null - cannot populate items list.");
            return;
        }

        var battleItems = InventoryManager.Instance.GetBattleUsableItems();

        if (battleItems == null || battleItems.Count == 0)
        {
            Debug.Log("No battle-usable items found in inventory.");
            return;
        }

        foreach (var stack in battleItems)
        {
            GameObject buttonObj = Instantiate(itemButtonPrefab, itemsListParent);
            BattleItemButton itemButton = buttonObj.GetComponent<BattleItemButton>();

            if (itemButton != null)
            {
                itemButton.Initialize(stack.item, stack.quantity, battleManager);
            }
            else
            {
                Debug.LogWarning("Item button prefab does not have a BattleItemButton component.");
            }
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

        if (itemsButton != null)
        {
            itemsButton.onClick.RemoveListener(OnItemsButtonClicked);
        }

        if (itemsBackButton != null)
        {
            itemsBackButton.onClick.RemoveListener(OnBackFromItemsButtonClicked);
        }

        if (clearLogCoroutine != null)
        {
            StopCoroutine(clearLogCoroutine);
        }
    }

    private System.Collections.IEnumerator ClearBattleLogAfterDelay()
    {
        yield return new WaitForSeconds(battleLogDisplayTime);

        if (battleLogText != null)
        {
            battleLogText.text = string.Empty;
        }

        clearLogCoroutine = null;
    }
}
