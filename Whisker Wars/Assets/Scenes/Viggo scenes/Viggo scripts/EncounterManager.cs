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
    
    // Collection of all defeated enemy IDs
    private HashSet<string> defeatedEnemyIDs = new HashSet<string>();
    private const string DEFEATED_ENEMIES_KEY = "DefeatedEnemyIDs";
    
    // Player position saving
    private Vector3 savedPlayerPosition;
    private bool hasSavedPlayerPosition = false;

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
        
        // Load defeated enemies from PlayerPrefs
        LoadDefeatedEnemies();
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
    /// Saves the player's current position before battle
    /// </summary>
    private void SavePlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            savedPlayerPosition = player.transform.position;
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

        // If player won, add the defeated enemy to the collection and save it
        if (playerWon && !string.IsNullOrEmpty(LastDefeatedEnemyID))
        {
            defeatedEnemyIDs.Add(LastDefeatedEnemyID);
            SaveDefeatedEnemies();
            Debug.Log($"[EncounterManager] Added defeated enemy '{LastDefeatedEnemyID}' to collection. Total defeated: {defeatedEnemyIDs.Count}");
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

        // Clear saved position since we're going to game over (not returning to overworld)
        hasSavedPlayerPosition = false;

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
            hasSavedPlayerPosition = false; // Clear the saved position
            Debug.Log($"[EncounterManager] Restored player position to: {savedPlayerPosition}");
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
    /// Saves the list of defeated enemy IDs to PlayerPrefs
    /// </summary>
    private void SaveDefeatedEnemies()
    {
        // Convert HashSet to comma-separated string
        string defeatedEnemiesString = string.Join(",", defeatedEnemyIDs.ToArray());
        PlayerPrefs.SetString(DEFEATED_ENEMIES_KEY, defeatedEnemiesString);
        PlayerPrefs.Save();
        Debug.Log($"[EncounterManager] Saved {defeatedEnemyIDs.Count} defeated enemies to PlayerPrefs");
    }

    /// <summary>
    /// Loads the list of defeated enemy IDs from PlayerPrefs
    /// </summary>
    private void LoadDefeatedEnemies()
    {
        defeatedEnemyIDs.Clear();
        
        if (PlayerPrefs.HasKey(DEFEATED_ENEMIES_KEY))
        {
            string defeatedEnemiesString = PlayerPrefs.GetString(DEFEATED_ENEMIES_KEY, "");
            if (!string.IsNullOrEmpty(defeatedEnemiesString))
            {
                // Split comma-separated string and add to HashSet
                string[] enemyIDs = defeatedEnemiesString.Split(',');
                foreach (string id in enemyIDs)
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        defeatedEnemyIDs.Add(id);
                    }
                }
                Debug.Log($"[EncounterManager] Loaded {defeatedEnemyIDs.Count} defeated enemies from PlayerPrefs");
            }
        }
        else
        {
            Debug.Log("[EncounterManager] No defeated enemies found in PlayerPrefs - starting fresh");
        }
    }

    /// <summary>
    /// Clears all defeated enemies (useful for testing or resetting)
    /// </summary>
    public void ClearDefeatedEnemies()
    {
        defeatedEnemyIDs.Clear();
        PlayerPrefs.DeleteKey(DEFEATED_ENEMIES_KEY);
        PlayerPrefs.Save();
        Debug.Log("[EncounterManager] Cleared all defeated enemies");
    }
}
