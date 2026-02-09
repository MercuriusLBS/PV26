using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyEncounter : MonoBehaviour
{
    [Header("Enemy Data")]
    [SerializeField] private EnemyData enemyData;
    
    [Header("Encounter Settings")]
    [SerializeField] private string enemyID = ""; // Unique ID for this enemy instance (optional)
    [SerializeField] private bool canEncounterMultipleTimes = false; // If false, enemy disappears after defeat
    [SerializeField] private bool isDefeated = false; // Track if this enemy has been defeated

    [Header("Collider Settings")]
    [SerializeField] private bool requirePlayerTag = true;

    private Collider2D encounterCollider;

    private void Awake()
    {
        Debug.Log($"[EnemyEncounter] Awake called on {gameObject.name}");
        
        encounterCollider = GetComponent<Collider2D>();
        
        // Ensure collider is set as trigger
        if (encounterCollider != null)
        {
            encounterCollider.isTrigger = true;
            Debug.Log($"[EnemyEncounter] Collider2D found and set as trigger on {gameObject.name}");
        }
        else
        {
            Debug.LogError($"[EnemyEncounter] {gameObject.name} requires a Collider2D component!");
        }

        // Check if enemy data is assigned
        if (enemyData == null)
        {
            Debug.LogWarning($"[EnemyEncounter] {gameObject.name} has no EnemyData assigned!");
        }
        else
        {
            Debug.Log($"[EnemyEncounter] {gameObject.name} has EnemyData: {enemyData.enemyName}");
        }
    }

    private void Start()
    {
        // Check if this enemy should be marked as defeated (from saved data)
        CheckIfDefeated();
        
        // Hide enemy if already defeated and can't be encountered again
        if (isDefeated && !canEncounterMultipleTimes)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[EnemyEncounter] OnTriggerEnter2D called on {gameObject.name} - Collision with: {collision.gameObject.name} (Tag: {collision.tag})");
        
        // Check if collision is with player
        if (requirePlayerTag && !collision.CompareTag("Player"))
        {
            Debug.Log($"[EnemyEncounter] Collision ignored - not Player tag. Required: Player, Got: {collision.tag}");
            return;
        }

        Debug.Log($"[EnemyEncounter] Player collision detected on {gameObject.name}!");

        // Don't trigger if already defeated and can't be encountered again
        if (isDefeated && !canEncounterMultipleTimes)
        {
            Debug.Log($"[EnemyEncounter] Encounter ignored - {gameObject.name} is already defeated");
            return;
        }

        // Don't trigger if no enemy data
        if (enemyData == null)
        {
            Debug.LogError($"[EnemyEncounter] Cannot start encounter - EnemyData is null on {gameObject.name}");
            return;
        }

        Debug.Log($"[EnemyEncounter] Starting encounter with {enemyData.enemyName} on {gameObject.name}");
        
        // Start encounter
        StartEncounter();
    }

    private void StartEncounter()
    {
        Debug.Log($"[EnemyEncounter] StartEncounter called on {gameObject.name}");
        
        // Get EncounterManager instance
        if (EncounterManager.Instance == null)
        {
            Debug.LogError("[EnemyEncounter] EncounterManager instance not found! Make sure there's an EncounterManager GameObject in the scene.");
            return;
        }

        Debug.Log("[EnemyEncounter] EncounterManager.Instance found!");

        // Use enemyID or generate one from GameObject name if not set
        string idToUse = string.IsNullOrEmpty(enemyID) ? gameObject.name : enemyID;
        Debug.Log($"[EnemyEncounter] Using enemy ID: {idToUse}");

        // Start encounter through EncounterManager
        Debug.Log($"[EnemyEncounter] Calling EncounterManager.StartEncounter with {enemyData.enemyName}");
        EncounterManager.Instance.StartEncounter(enemyData, idToUse);
    }

    /// <summary>
    /// Called when this enemy is defeated (from EncounterManager or external script)
    /// </summary>
    public void MarkAsDefeated()
    {
        isDefeated = true;

        // Hide enemy if it can't be encountered again
        if (!canEncounterMultipleTimes)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Check if this enemy matches any defeated enemy ID
    /// </summary>
    public void CheckIfDefeated()
    {
        if (EncounterManager.Instance != null)
        {
            string thisID = string.IsNullOrEmpty(enemyID) ? gameObject.name : enemyID;

            // Check if this enemy has been defeated (using the collection of all defeated enemies)
            if (EncounterManager.Instance.IsEnemyDefeated(thisID))
            {
                MarkAsDefeated();
            }
        }
    }

    // Called when scene loads - check if this enemy should be marked as defeated
    private void OnEnable()
    {
        if (EncounterManager.Instance != null)
        {
            CheckIfDefeated();
        }
    }
}
