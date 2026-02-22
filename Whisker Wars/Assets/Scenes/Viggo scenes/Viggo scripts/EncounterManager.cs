using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class EncounterManager : MonoBehaviour
{
    public static EncounterManager Instance;

    [Header("Scene Names")]
    [SerializeField] private string battleSceneName = "Viggo turnbasedscene";
    [SerializeField] private string overworldSceneName = "MainViggo";
    [SerializeField] private string gameOverSceneName = "GameOverScreen";

    // Current encounter data
    public EnemyData CurrentEnemyData { get; private set; }
    public bool LastBattleWon { get; private set; }
    public string LastDefeatedEnemyID { get; private set; } // For tracking which enemy was defeated (kept for backward compatibility)
    
    // Collection of all defeated enemy IDs (per session only - not persisted between runs)
    private HashSet<string> defeatedEnemyIDs = new HashSet<string>();

    // Collection of collected pickable IDs (per session only - so they don't respawn after returning from combat)
    private HashSet<string> collectedPickableIDs = new HashSet<string>();
    
    // Player position and health saving (for returning from combat)
    private Vector3 savedPlayerPosition;
    private bool hasSavedPlayerPosition = false;
    private int savedPlayerHealth;
    private int savedPlayerMaxHealth;
    private bool hasSavedPlayerHealth = false;

    // Popup: show "first defeat - whisker info" when returning to overworld after first victory
    private bool pendingFirstDefeatPopup = false;

    private void Awake()
    {
        Debug.Log("[EncounterManager] Awake called");
        
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[EncounterManager] Instance created and set to DontDestroyOnLoad");
            
            // Subscribe to scene loaded event
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Debug.Log("[EncounterManager] Duplicate instance detected - destroying");
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Called when a scene finishes loading
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[EncounterManager] Scene loaded: {scene.name}");

        // Hide inventory in battle (and other non-overworld) scenes, show it again in overworld
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.SetInventoryVisible(scene.name == overworldSceneName);
        
        // If we're loading the overworld scene and have a saved position, restore it
        if (scene.name == overworldSceneName && hasSavedPlayerPosition)
        {
            Debug.Log("[EncounterManager] Overworld scene loaded - restoring player position");
            StartCoroutine(RestorePlayerPositionCoroutine());
        }
    }

    private void Start()
    {
        Debug.Log($"[EncounterManager] Start called - Battle Scene: {battleSceneName}, Overworld Scene: {overworldSceneName}");
        Debug.Log($"[EncounterManager] Current Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        
        // We no longer load defeated enemies from PlayerPrefs here.
        // Defeated enemies are now tracked only for the current game session.
    }

    /// <summary>
    /// Sets up encounter data and loads battle scene
    /// </summary>
    public void StartEncounter(EnemyData enemyData, string enemyID = "")
    {
        Debug.Log($"[EncounterManager] StartEncounter called with enemyData: {(enemyData != null ? enemyData.enemyName : "NULL")}, enemyID: {enemyID}");
        
        if (enemyData == null)
        {
            Debug.LogError("[EncounterManager] EnemyData is null! Cannot start encounter.");
            return;
        }

        // Save player position before loading battle scene
        SavePlayerPosition();

        // Hide inventory as we switch to battle (it will be shown again when returning to overworld)
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.SetInventoryVisible(false);

        CurrentEnemyData = enemyData;
        LastDefeatedEnemyID = enemyID;
        LastBattleWon = false;

        Debug.Log($"[EncounterManager] Starting encounter with {enemyData.enemyName} (ID: {enemyID})");
        Debug.Log($"[EncounterManager] Current Scene: {SceneManager.GetActiveScene().name}");
        Debug.Log($"[EncounterManager] Attempting to load battle scene: {battleSceneName}");

        // Load battle scene
        if (!string.IsNullOrEmpty(battleSceneName))
        {
            Debug.Log($"[EncounterManager] Loading scene: {battleSceneName}");
            SceneManager.LoadScene(battleSceneName);
        }
        else
        {
            Debug.LogError("[EncounterManager] Battle scene name is not set in EncounterManager!");
        }
    }

    /// <summary>
    /// Saves the player's current position before battle.
    /// Health persistence is handled when the battle ends (in EndEncounter).
    /// </summary>
    private void SavePlayerPosition()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        
        if (playerObj != null)
        {
            savedPlayerPosition = playerObj.transform.position;
            hasSavedPlayerPosition = true;
            Debug.Log($"[EncounterManager] Saved player position: {savedPlayerPosition}");
        }
        else
        {
            Debug.LogWarning("[EncounterManager] Could not find player GameObject with 'Player' tag to save position!");
            hasSavedPlayerPosition = false;
        }
    }

    /// <summary>
    /// Called when battle ends - stores result and returns to overworld or game over
    /// </summary>
    public void EndEncounter(bool playerWon)
    {
        Debug.Log($"[EncounterManager] EndEncounter called - Player won: {playerWon}");
        
        LastBattleWon = playerWon;

        // If player won, save their current health from battle so we can restore it in overworld
        if (playerWon)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                Character playerChar = playerObj.GetComponent<Character>();
                if (playerChar != null)
                {
                    savedPlayerHealth = playerChar.CurrentHealth;
                    savedPlayerMaxHealth = playerChar.MaxHealth;
                    hasSavedPlayerHealth = true;
                    Debug.Log($"[EncounterManager] Saved player health after battle: {savedPlayerHealth}/{savedPlayerMaxHealth}");
                }
            }
        }

        // If player won, add the defeated enemy to the in-memory collection
        if (playerWon && !string.IsNullOrEmpty(LastDefeatedEnemyID))
        {
            defeatedEnemyIDs.Add(LastDefeatedEnemyID);
            Debug.Log($"[EncounterManager] Added defeated enemy '{LastDefeatedEnemyID}' to collection. Total defeated: {defeatedEnemyIDs.Count}");
            // First victory ever — show whisker info popup when we're back in overworld
            if (defeatedEnemyIDs.Count == 1)
                pendingFirstDefeatPopup = true;
        }

        Debug.Log($"[EncounterManager] Encounter ended. Player won: {playerWon}, Defeated Enemy ID: {LastDefeatedEnemyID}");

        if (playerWon)
        {
            // Player won - return to overworld after a short delay
            StartCoroutine(ReturnToOverworldCoroutine());
        }
        else
        {
            // Player lost - go to game over screen
            StartCoroutine(GoToGameOverCoroutine());
        }
    }

    private System.Collections.IEnumerator ReturnToOverworldCoroutine()
    {
        Debug.Log("[EncounterManager] ReturnToOverworldCoroutine started - waiting 2 seconds");
        
        // Wait a moment for battle end message to be seen
        yield return new WaitForSeconds(2f);

        Debug.Log($"[EncounterManager] Loading overworld scene: {overworldSceneName}");
        Debug.Log($"[EncounterManager] Current Scene: {SceneManager.GetActiveScene().name}");

        // Load overworld scene
        if (!string.IsNullOrEmpty(overworldSceneName))
        {
            SceneManager.LoadScene(overworldSceneName);
            // Position restoration will happen in OnSceneLoaded callback
        }
        else
        {
            Debug.LogError("[EncounterManager] Overworld scene name is not set in EncounterManager!");
        }
    }

    private System.Collections.IEnumerator GoToGameOverCoroutine()
    {
        Debug.Log("[EncounterManager] GoToGameOverCoroutine started - waiting 2 seconds");
        
        // Wait a moment for battle end message to be seen
        yield return new WaitForSeconds(2f);

        Debug.Log($"[EncounterManager] Loading game over scene: {gameOverSceneName}");
        Debug.Log($"[EncounterManager] Current Scene: {SceneManager.GetActiveScene().name}");

        // Clear saved position and health since we're going to game over (not returning to overworld)
        hasSavedPlayerPosition = false;
        hasSavedPlayerHealth = false;

        // Load game over scene
        if (!string.IsNullOrEmpty(gameOverSceneName))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
        else
        {
            Debug.LogError("[EncounterManager] Game over scene name is not set in EncounterManager!");
        }
    }

    private System.Collections.IEnumerator RestorePlayerPositionCoroutine()
    {
        // Wait a frame to ensure scene is loaded
        yield return null;
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            player.transform.position = savedPlayerPosition;
            hasSavedPlayerPosition = false;
            Debug.Log($"[EncounterManager] Restored player position to: {savedPlayerPosition}");

            if (hasSavedPlayerHealth)
            {
                Character playerChar = player.GetComponent<Character>();
                if (playerChar != null)
                {
                    playerChar.SetCurrentHealth(savedPlayerHealth);
                    Debug.Log($"[EncounterManager] Restored player health to: {savedPlayerHealth}/{savedPlayerMaxHealth}");
                }
            }

            // Show first-defeat whisker info popup (only once per session)
            if (pendingFirstDefeatPopup && PopupManager.Instance != null)
            {
                pendingFirstDefeatPopup = false;
                PopupManager.Instance.TryShowFirstDefeatWhiskerInfo();
            }
        }
        else
        {
            Debug.LogWarning("[EncounterManager] Could not find player GameObject with 'Player' tag to restore position!");
        }
    }

    /// <summary>
    /// Clears encounter data (useful for testing or resetting)
    /// </summary>
    public void ClearEncounterData()
    {
        CurrentEnemyData = null;
        LastBattleWon = false;
        LastDefeatedEnemyID = string.Empty;
    }

    /// <summary>
    /// Whether we have saved player health to apply at start of battle (from overworld or previous battle).
    /// </summary>
    public bool HasSavedPlayerHealth => hasSavedPlayerHealth;

    public int SavedPlayerHealth => savedPlayerHealth;
    public int SavedPlayerMaxHealth => savedPlayerMaxHealth;

    /// <summary>
    /// Checks if an enemy with the given ID has been defeated
    /// </summary>
    public bool IsEnemyDefeated(string enemyID)
    {
        if (string.IsNullOrEmpty(enemyID))
        {
            return false;
        }
        return defeatedEnemyIDs.Contains(enemyID);
    }

    /// <summary>
    /// Registers a pickable as collected so it won't respawn when returning from combat (session only).
    /// </summary>
    public void RegisterCollectedPickable(string pickableID)
    {
        if (string.IsNullOrEmpty(pickableID)) return;
        collectedPickableIDs.Add(pickableID);
    }

    /// <summary>
    /// Returns true if this pickable was already collected this session (e.g. before going to combat).
    /// </summary>
    public bool IsPickableCollected(string pickableID)
    {
        if (string.IsNullOrEmpty(pickableID)) return false;
        return collectedPickableIDs.Contains(pickableID);
    }

    // Note: previously we saved / loaded defeated enemies using PlayerPrefs so that
    // defeated enemies persisted between game sessions. To avoid issues where
    // old data from previous playthroughs caused all enemies to appear defeated
    // after a single encounter, that persistence has been removed. The
    // defeatedEnemyIDs HashSet now only lives for the current run of the game.
}
