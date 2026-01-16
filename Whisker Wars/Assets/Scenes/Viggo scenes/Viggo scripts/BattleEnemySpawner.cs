using UnityEngine;

public class BattleEnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform enemySpawnPoint; // Where to spawn the enemy (optional - uses transform.position if null)
    [SerializeField] private GameObject defaultEnemyPrefab; // Fallback enemy if no EnemyData prefab is found

    [Header("References")]
    [SerializeField] private Battlemanager battleManager; // Reference to BattleManager to set spawned enemy

    private GameObject currentSpawnedEnemy;

    private void Awake()
    {
        Debug.Log("[BattleEnemySpawner] Awake called");

        // Find BattleManager if not assigned
        if (battleManager == null)
        {
            battleManager = FindFirstObjectByType<Battlemanager>();
            if (battleManager == null)
            {
                Debug.LogError("[BattleEnemySpawner] BattleManager not found! Enemy spawning will not work.");
            }
        }

        // Set spawn point to this transform if not assigned
        if (enemySpawnPoint == null)
        {
            enemySpawnPoint = transform;
        }
    }

    private void Start()
    {
        Debug.Log("[BattleEnemySpawner] Start called - spawning enemy");
        SpawnEnemy();
    }

    /// <summary>
    /// Spawns the enemy prefab based on EncounterManager's EnemyData
    /// NOTE: This spawner is deprecated. Enemies are now configured directly in BattleManager.
    /// You can disable or remove this component from the scene.
    /// </summary>
    public void SpawnEnemy()
    {
        Debug.LogWarning("[BattleEnemySpawner] BattleEnemySpawner is deprecated and no longer used. Enemies are configured directly in BattleManager. You can remove this component.");
        // Spawner is no longer used - enemies are configured directly in BattleManager
        return;
    }

    /// <summary>
    /// Spawns the default enemy prefab as fallback
    /// </summary>
    private void SpawnDefaultEnemy()
    {
        if (defaultEnemyPrefab == null)
        {
            Debug.LogError("[BattleEnemySpawner] No default enemy prefab assigned and no EnemyData prefab available!");
            return;
        }

        Debug.Log("[BattleEnemySpawner] Spawning default enemy prefab");

        // Destroy any existing enemy
        if (currentSpawnedEnemy != null)
        {
            Destroy(currentSpawnedEnemy);
        }

        // Spawn default enemy
        Vector3 spawnPosition = enemySpawnPoint != null ? enemySpawnPoint.position : transform.position;
        currentSpawnedEnemy = Instantiate(defaultEnemyPrefab, spawnPosition, Quaternion.identity);
        currentSpawnedEnemy.name = "DefaultEnemy";

        // Get Character component
        Character enemyCharacter = currentSpawnedEnemy.GetComponent<Character>();
        if (enemyCharacter != null && battleManager != null)
        {
            // Set enemy in BattleManager
            battleManager.SetEnemy(enemyCharacter);
            Debug.Log("[BattleEnemySpawner] Set default enemy in BattleManager");
        }
    }

    /// <summary>
    /// Cleans up spawned enemy (called when battle ends or scene unloads)
    /// </summary>
    public void CleanupEnemy()
    {
        if (currentSpawnedEnemy != null)
        {
            Debug.Log("[BattleEnemySpawner] Cleaning up spawned enemy");
            Destroy(currentSpawnedEnemy);
            currentSpawnedEnemy = null;
        }
    }

    private void OnDestroy()
    {
        CleanupEnemy();
    }
}
