using UnityEngine;

/// <summary>
/// Attach this to the same GameObject as Battlemanager.
/// When the player wins a battle, it adds a "Whisker" item to the InventoryManager.
/// 
/// SETUP REQUIRED:
/// 1. Assign a "Whisker" Item ScriptableObject to the whiskerItem field in the Inspector.
/// 2. Attach this component to the same GameObject as Battlemanager.
/// </summary>
public class BattleLootDropper : MonoBehaviour
{
    [Header("Loot Settings")]
    [Tooltip("Drag your Whisker Item ScriptableObject here")]
    [SerializeField] private Item whiskerItem;

    [Tooltip("How many Whiskers to award per enemy defeated")]
    [SerializeField] private int whiskerQuantity = 1;

    private Battlemanager battleManager;
    private bool lootAlreadyAwarded = false;

    private void Awake()
    {
        battleManager = GetComponent<Battlemanager>();

        if (battleManager == null)
        {
            Debug.LogError("[BattleLootDropper] Battlemanager not found on this GameObject! " +
                           "Attach BattleLootDropper to the same GameObject as Battlemanager.");
        }

        if (whiskerItem == null)
        {
            Debug.LogWarning("[BattleLootDropper] No Whisker item assigned in the Inspector! " +
                             "Create a Whisker Item ScriptableObject and assign it.");
        }
    }

    private void Start()
    {
        // Subscribe to the EncounterManager's scene-load callback indirectly
        // by polling EndEncounter result. We hook in via Update to watch BattleState.
        lootAlreadyAwarded = false;
    }

    private void Update()
    {
        // Watch for the battle ending in a player victory
        if (battleManager == null) return;
        if (lootAlreadyAwarded) return;

        if (battleManager.CurrentState == BattleState.BattleEnd)
        {
            // EncounterManager.LastBattleWon is set just before EndBattle fires,
            // but we detect the win condition by checking if the enemy is no longer alive.
            // We also check EncounterManager.LastBattleWon for safety.
            bool playerWon = false;

            if (EncounterManager.Instance != null)
            {
                playerWon = EncounterManager.Instance.LastBattleWon;
            }
            else
            {
                // Fallback: check if enemy is dead and player is alive
                if (battleManager.Enemy != null && battleManager.Player != null)
                {
                    playerWon = !battleManager.Enemy.IsAlive && battleManager.Player.IsAlive;
                }
            }

            if (playerWon)
            {
                AwardWhisker();
            }

            // Mark as handled so we only run once per battle
            lootAlreadyAwarded = true;
        }
    }

    /// <summary>
    /// Adds the Whisker item to the player's inventory.
    /// </summary>
    private void AwardWhisker()
    {
        if (whiskerItem == null)
        {
            Debug.LogError("[BattleLootDropper] Cannot award Whisker - whiskerItem is not assigned in the Inspector!");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[BattleLootDropper] Cannot award Whisker - InventoryManager.Instance is null!");
            return;
        }

        InventoryManager.Instance.AddItem(whiskerItem, whiskerQuantity);

        Debug.Log($"[BattleLootDropper] Awarded {whiskerQuantity}x '{whiskerItem.itemName}' to inventory!");

        // First time reaching 10+ whiskers — show milestone popup (only once per session)
        if (PopupManager.Instance != null && InventoryManager.Instance.GetItemCount(whiskerItem) >= 10)
            PopupManager.Instance.TryShowTenWhiskersMilestone();
    }

    /// <summary>
    /// Call this externally if you want to reset the dropper for a new battle
    /// (useful if battles can restart without a scene reload).
    /// </summary>
    public void ResetForNewBattle()
    {
        lootAlreadyAwarded = false;
    }
}