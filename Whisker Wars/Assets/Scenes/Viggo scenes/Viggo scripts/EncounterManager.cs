using UnityEngine;
using UnityEngine.SceneManagement;

public class EncounterManager : MonoBehaviour
{
    public static EncounterManager Instance;

    [Header("Scene Names")]
    [SerializeField] private string battleSceneName = "Viggo turnbasedscene";
    [SerializeField] private string overworldSceneName = "MainViggo";

    // Current encounter data
    public EnemyData CurrentEnemyData { get; private set; }
    public bool LastBattleWon { get; private set; }
    public string LastDefeatedEnemyID { get; private set; } // For tracking which enemy was defeated

    private void Awake()
    {
        Debug.Log("[EncounterManager] Awake called");
        
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[EncounterManager] Instance created and set to DontDestroyOnLoad");
        }
        else
        {
            Debug.Log("[EncounterManager] Duplicate instance detected - destroying");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log($"[EncounterManager] Start called - Battle Scene: {battleSceneName}, Overworld Scene: {overworldSceneName}");
        Debug.Log($"[EncounterManager] Current Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
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
    /// Called when battle ends - stores result and returns to overworld
    /// </summary>
    public void EndEncounter(bool playerWon)
    {
        Debug.Log($"[EncounterManager] EndEncounter called - Player won: {playerWon}");
        
        LastBattleWon = playerWon;

        Debug.Log($"[EncounterManager] Encounter ended. Player won: {playerWon}, Defeated Enemy ID: {LastDefeatedEnemyID}");

        // Return to overworld after a short delay
        StartCoroutine(ReturnToOverworldCoroutine());
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
        }
        else
        {
            Debug.LogError("[EncounterManager] Overworld scene name is not set in EncounterManager!");
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
}
