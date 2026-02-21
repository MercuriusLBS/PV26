using UnityEngine;
using System.Collections;

public enum BattleState
{
    PlayerTurn,
    EnemyTurn,
    BattleEnd
}

public class Battlemanager : MonoBehaviour
{
    [Header("Combatants")]
    [SerializeField] private Character player;
    [SerializeField] private Character enemy; // Will be set by BattleEnemySpawner or found automatically

    [Header("Battle Settings")]
    [SerializeField] private float turnDelay = 1f; // Delay between turns for visual feedback

    [Header("Hit Effect")]
    [Tooltip("Prefab to spawn on the player when the enemy's attack impacts (e.g. HitFx_Player).")]
    public GameObject playerHitEffectPrefab;

    private BattleState currentState = BattleState.PlayerTurn;
    private BattleUI battleUI;

    // Special attack cooldown (in player turns)
    private int playerSpecialCooldownTurns = 0;

    // Attack result structure
    private struct AttackResult
    {
        public bool hit;
        public bool criticalHit;
        public int damage;
        public bool evaded;
    }

    public Character Player => player;
    public Character Enemy => enemy;
    public BattleState CurrentState => currentState;

    /// <summary>
    /// Sets the enemy character (called by BattleEnemySpawner)
    /// </summary>
    public void SetEnemy(Character enemyCharacter)
    {
        enemy = enemyCharacter;
        Debug.Log($"[BattleManager] Enemy set to: {(enemy != null ? enemy.CharacterName : "NULL")}");
    }

    private void Awake()
    {
        Debug.Log("[BattleManager] Awake called");
        
        battleUI = FindFirstObjectByType<BattleUI>();
        if (battleUI == null)
        {
            Debug.LogWarning("[BattleManager] BattleUI not found! UI updates will not work.");
        }
        else
        {
            Debug.Log("[BattleManager] BattleUI found");
        }

        // Check for EncounterManager
        if (EncounterManager.Instance == null)
        {
            Debug.LogWarning("[BattleManager] EncounterManager.Instance is NULL!");
        }
        else
        {
            Debug.Log("[BattleManager] EncounterManager.Instance found");
            if (EncounterManager.Instance.CurrentEnemyData == null)
            {
                Debug.LogWarning("[BattleManager] EncounterManager has no CurrentEnemyData!");
            }
            else
            {
                Debug.Log($"[BattleManager] EncounterManager has EnemyData: {EncounterManager.Instance.CurrentEnemyData.enemyName}");
            }
        }
    }

    private void Start()
    {
        Debug.Log("[BattleManager] Start called - configuring enemy and starting battle");
        StartBattle();
    }

    public void StartBattle()
    {
        if (player == null)
        {
            Debug.LogError("[BattleManager] Player not assigned in BattleManager!");
            return;
        }

        // We rely on a single enemy \"slot\" GameObject assigned in the inspector.
        // This GameObject will be configured at runtime based on the EnemyData
        // from EncounterManager (stats, sprite, animations).
        if (enemy == null)
        {
            Debug.LogError("[BattleManager] Enemy is null! Please assign an enemy GameObject (battle enemy) in the BattleManager Inspector.");
            return;
        }

        Debug.Log($"[BattleManager] Starting battle with player: {player.CharacterName} and enemy placeholder: {enemy.CharacterName}");

        // Configure enemy from EncounterManager if available
        if (EncounterManager.Instance != null && EncounterManager.Instance.CurrentEnemyData != null)
        {
            EnemyData enemyData = EncounterManager.Instance.CurrentEnemyData;
            Debug.Log($"[BattleManager] Configuring enemy from EnemyData: {enemyData.enemyName}");

            // Configure stats and visuals on the assigned enemy GameObject
            enemy.ConfigureFromEnemyData(enemyData);
            ConfigureEnemyVisuals(enemy.gameObject, enemyData);

            Debug.Log($"[BattleManager] Enemy configured - Stats: HP={enemy.MaxHealth}, Attack={enemy.Attack}, Name={enemy.CharacterName}");
        }
        else
        {
            Debug.LogWarning("[BattleManager] No EncounterManager or EnemyData found - using default enemy stats and visuals");
        }

        // Disable player movement during battle
        DisablePlayerMovement();

        // Use saved player health across encounters, or full health if first fight
        if (EncounterManager.Instance != null && EncounterManager.Instance.HasSavedPlayerHealth)
            player.SetCurrentHealth(EncounterManager.Instance.SavedPlayerHealth);
        else
            player.ResetHealth();
        enemy.ResetHealth();
        playerSpecialCooldownTurns = 0;

        currentState = BattleState.PlayerTurn;
        
        // Update UI
        if (battleUI != null)
        {
            battleUI.UpdateHealthBars(player, enemy);
            battleUI.SetActionMenuActive(true);
        }

        Debug.Log($"Battle started! {player.CharacterName} vs {enemy.CharacterName}");
    }

    public void PlayerAttack()
    {
        if (currentState != BattleState.PlayerTurn)
        {
            Debug.LogWarning("Not player's turn!");
            return;
        }

        // Perform attack with all combat mechanics
        AttackResult result = PerformAttack(player, enemy);

        // Handle attack result
        if (result.evaded)
        {
            string message = $"{enemy.CharacterName} evaded the attack!";
            Debug.Log(message);
            if (battleUI != null)
            {
                battleUI.ShowBattleLog(message);
            }
        }
        else
        {
            enemy.TakeDamage(result.damage);
            
            string message = result.criticalHit 
                ? $"{player.CharacterName} lands a CRITICAL HIT for {result.damage} damage!" 
                : $"{player.CharacterName} attacks for {result.damage} damage!";
            
            Debug.Log(message);
            
            if (battleUI != null)
            {
                battleUI.UpdateHealthBars(player, enemy);
                battleUI.ShowBattleLog(message);
            }
        }

        // Check if enemy is defeated
        if (!enemy.IsAlive)
        {
            EndBattle(true); // Player wins
            return;
        }

        // Disable action menu and switch to enemy turn
        if (battleUI != null)
        {
            battleUI.SetActionMenuActive(false);
        }

        StartCoroutine(EnemyTurnCoroutine());
    }

    private IEnumerator EnemyTurnCoroutine()
    {
        currentState = BattleState.EnemyTurn;
        
        // Wait for visual feedback
        yield return new WaitForSeconds(turnDelay);

        // Play enemy attack animation; damage and hit FX are applied at impact time inside the coroutine
        yield return StartCoroutine(PlayEnemyAttackAnimationCoroutine());

        // Check if player is defeated (attack was already resolved at impact)
        if (!player.IsAlive)
        {
            EndBattle(false); // Enemy wins
            yield break;
        }

        // Switch back to player turn
        yield return new WaitForSeconds(turnDelay);
        currentState = BattleState.PlayerTurn;

        // Reduce special attack cooldown at the start of player's turn
        if (playerSpecialCooldownTurns > 0)
        {
            playerSpecialCooldownTurns--;
        }

        if (battleUI != null)
        {
            battleUI.SetActionMenuActive(true);
        }
    }

    public void PlayerSpecialAttack()
    {
        if (currentState != BattleState.PlayerTurn)
        {
            Debug.LogWarning("Not player's turn!");
            return;
        }

        // Check cooldown
        if (playerSpecialCooldownTurns > 0)
        {
            string cooldownMessage = $"{player.CharacterName}'s special attack is on cooldown for {playerSpecialCooldownTurns} more turn(s)!";
            Debug.Log(cooldownMessage);
            if (battleUI != null)
            {
                battleUI.ShowBattleLog(cooldownMessage);
            }
            return;
        }

        // Perform special attack with lower accuracy
        AttackResult result = PerformAttack(player, enemy, isSpecialAttack: true);

        // Handle attack result
        if (result.evaded)
        {
            string message = $"{player.CharacterName}'s special attack missed!";
            Debug.Log(message);
            if (battleUI != null)
            {
                battleUI.ShowBattleLog(message);
            }
        }
        else
        {
            enemy.TakeDamage(result.damage);
            
            string message = result.criticalHit 
                ? $"{player.CharacterName} lands a CRITICAL SPECIAL ATTACK for {result.damage} damage!" 
                : $"{player.CharacterName} uses SPECIAL ATTACK for {result.damage} damage!";
            
            Debug.Log(message);
            
            if (battleUI != null)
            {
                battleUI.UpdateHealthBars(player, enemy);
                battleUI.ShowBattleLog(message);
            }
        }

        // Put special attack on cooldown (3 player turns)
        playerSpecialCooldownTurns = 3;

        // Check if enemy is defeated
        if (!enemy.IsAlive)
        {
            EndBattle(true); // Player wins
            return;
        }

        // Disable action menu and switch to enemy turn
        if (battleUI != null)
        {
            battleUI.SetActionMenuActive(false);
        }

        StartCoroutine(EnemyTurnCoroutine());
    }

    public void PlayerDefend()
    {
        if (currentState != BattleState.PlayerTurn)
        {
            Debug.LogWarning("Not player's turn!");
            return;
        }

        // Set player to guard state
        player.SetGuarding(true);
        
        string message = $"{player.CharacterName} takes a defensive stance!";
        Debug.Log(message);
        
        if (battleUI != null)
        {
            battleUI.ShowBattleLog(message);
        }

        // Disable action menu and switch to enemy turn
        if (battleUI != null)
        {
            battleUI.SetActionMenuActive(false);
        }

        StartCoroutine(EnemyTurnCoroutine());
    }

    /// <summary>
    /// Uses an item from the player's inventory during their turn (e.g., a healing item).
    /// </summary>
    public void PlayerUseItem(Item item)
    {
        if (currentState != BattleState.PlayerTurn)
        {
            Debug.LogWarning("[BattleManager] Cannot use item - not player's turn.");
            return;
        }

        if (item == null)
        {
            Debug.LogWarning("[BattleManager] Cannot use item - item is null.");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[BattleManager] Cannot use item - InventoryManager.Instance is null.");
            return;
        }

        if (!item.canUseInBattle)
        {
            string notUsableMessage = $"{item.itemName} cannot be used in battle.";
            Debug.Log(notUsableMessage);
            if (battleUI != null)
            {
                battleUI.ShowBattleLog(notUsableMessage);
            }
            return;
        }

        int currentCount = InventoryManager.Instance.GetItemCount(item);
        if (currentCount <= 0)
        {
            string noItemMessage = $"No more {item.itemName} left!";
            Debug.Log(noItemMessage);
            if (battleUI != null)
            {
                battleUI.ShowBattleLog(noItemMessage);
            }
            return;
        }

        // Apply healing
        int healAmount = Mathf.Max(0, item.healAmount);
        if (healAmount <= 0)
        {
            string zeroHealMessage = $"{item.itemName} has no healing effect.";
            Debug.Log(zeroHealMessage);
            if (battleUI != null)
            {
                battleUI.ShowBattleLog(zeroHealMessage);
            }
            return;
        }

        player.Heal(healAmount);

        // Remove one item from inventory
        InventoryManager.Instance.RemoveItem(item, 1);

        string message = $"{player.CharacterName} uses {item.itemName} and heals {healAmount} HP!";
        Debug.Log(message);

        if (battleUI != null)
        {
            battleUI.UpdateHealthBars(player, enemy);
            battleUI.ShowBattleLog(message);
        }

        // End player's turn after using an item
        if (battleUI != null)
        {
            battleUI.SetActionMenuActive(false);
        }

        StartCoroutine(EnemyTurnCoroutine());
    }

    /// <summary>
    /// Performs an attack from attacker to defender with evasion, critical hits, and random damage
    /// </summary>
    private AttackResult PerformAttack(Character attacker, Character defender, bool isSpecialAttack = false)
    {
        AttackResult result = new AttackResult();
        
        // For special attacks, check accuracy first (before evasion)
        if (isSpecialAttack)
        {
            float accuracyRoll = Random.Range(0f, 100f);
            if (accuracyRoll >= attacker.SpecialAttackAccuracy)
            {
                result.evaded = true;
                result.hit = false;
                return result;
            }
        }
        
        // Check for evasion
        float evasionRoll = Random.Range(0f, 100f);
        if (evasionRoll < defender.EvasionChance)
        {
            result.evaded = true;
            result.hit = false;
            return result;
        }

        result.hit = true;
        result.evaded = false;

        // Calculate base damage with variance
        float baseDamage = attacker.Attack;
        
        // Apply special attack multiplier if it's a special attack
        if (isSpecialAttack)
        {
            baseDamage *= attacker.SpecialAttackMultiplier;
        }
        
        float variance = baseDamage * attacker.DamageVariance;
        float minDamage = baseDamage - variance;
        float maxDamage = baseDamage + variance;
        float damage = Random.Range(minDamage, maxDamage);

        // Check for critical hit
        float critRoll = Random.Range(0f, 100f);
        if (critRoll < attacker.CriticalHitChance)
        {
            result.criticalHit = true;
            damage *= attacker.CriticalHitMultiplier;
        }
        else
        {
            result.criticalHit = false;
        }

        // Round damage to integer
        result.damage = Mathf.RoundToInt(damage);
        
        return result;
    }

    private void EndBattle(bool playerWon)
    {
        Debug.Log($"[BattleManager] EndBattle called - Player won: {playerWon}");
        
        currentState = BattleState.BattleEnd;

        if (battleUI != null)
        {
            battleUI.SetActionMenuActive(false);
        }

        if (playerWon)
        {
            Debug.Log($"[BattleManager] {player.CharacterName} wins the battle!");
            if (battleUI != null)
            {
                battleUI.ShowBattleLog($"{player.CharacterName} wins!");
            }
        }
        else
        {
            Debug.Log($"[BattleManager] {enemy.CharacterName} wins the battle!");
            if (battleUI != null)
            {
                battleUI.ShowBattleLog($"{enemy.CharacterName} wins!");
            }
        }

        // Notify EncounterManager about battle result and return to overworld
        Debug.Log("[BattleManager] Checking for EncounterManager to end encounter...");
        
        if (EncounterManager.Instance != null)
        {
            Debug.Log("[BattleManager] EncounterManager.Instance found - calling EndEncounter");
            EncounterManager.Instance.EndEncounter(playerWon);
        }
        else
        {
            Debug.LogError("[BattleManager] EncounterManager not found - cannot return to overworld automatically");
        }
    }

    /// <summary>
    /// Configures the enemy GameObject's visuals (sprite and animator) based on EnemyData
    /// </summary>
    private void ConfigureEnemyVisuals(GameObject enemyObject, EnemyData enemyData)
    {
        if (enemyObject == null || enemyData == null)
        {
            Debug.LogWarning("[BattleManager] Cannot configure enemy visuals - enemyObject or enemyData is null");
            return;
        }

        Debug.Log($"[BattleManager] Configuring visuals for enemy GameObject: {enemyObject.name}");
        
        // Ensure enemy GameObject is active and visible
        if (!enemyObject.activeSelf)
        {
            Debug.Log($"[BattleManager] Enemy GameObject was inactive - activating it");
            enemyObject.SetActive(true);
        }

        // Configure sprite
        SpriteRenderer spriteRenderer = enemyObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            // Try to get it from children
            spriteRenderer = enemyObject.GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            if (enemyData.enemySprite != null)
            {
                spriteRenderer.sprite = enemyData.enemySprite;
                spriteRenderer.enabled = true; // Ensure sprite renderer is enabled
                Debug.Log($"[BattleManager] Set enemy sprite to: {enemyData.enemySprite.name} on {spriteRenderer.gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"[BattleManager] EnemyData {enemyData.enemyName} has no sprite assigned!");
            }
        }
        else
        {
            Debug.LogError($"[BattleManager] Enemy GameObject {enemyObject.name} has no SpriteRenderer component! Please add one.");
        }

        // Configure animator controller
        Animator animator = enemyObject.GetComponent<Animator>();
        if (animator == null)
        {
            // Try to get it from children
            animator = enemyObject.GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            if (enemyData.enemyAnimatorController != null)
            {
                animator.runtimeAnimatorController = enemyData.enemyAnimatorController;
                animator.enabled = true; // Ensure animator is enabled
                Debug.Log($"[BattleManager] Set enemy animator controller to: {enemyData.enemyAnimatorController.name} on {animator.gameObject.name}");
            }
            else
            {
                Debug.Log($"[BattleManager] EnemyData {enemyData.enemyName} has no animator controller assigned - using default");
            }
        }
        else
        {
            Debug.Log($"[BattleManager] Enemy GameObject {enemyObject.name} has no Animator component - skipping animator setup (this is okay if not using animations)");
        }

        // Log enemy position for debugging
        Debug.Log($"[BattleManager] Enemy position: {enemyObject.transform.position}, Active: {enemyObject.activeSelf}, Visible: {(spriteRenderer != null ? spriteRenderer.enabled : "N/A")}");
    }

    /// <summary>
    /// Coroutine to play the enemy attack animation. Damage and hit FX are applied at the impact time for the current enemy.
    /// </summary>
    private IEnumerator PlayEnemyAttackAnimationCoroutine()
    {
        if (enemy == null)
        {
            Debug.LogWarning("[BattleManager] Cannot play enemy attack animation - enemy is null");
            yield break;
        }

        Animator animator = enemy.GetComponent<Animator>();
        if (animator == null)
            animator = enemy.GetComponentInChildren<Animator>();

        if (animator == null)
        {
            Debug.LogWarning($"[BattleManager] Enemy {enemy.gameObject.name} has no Animator component - resolving attack immediately");
            ResolveEnemyAttackAtImpact();
            yield break;
        }

        if (!animator.enabled)
        {
            Debug.LogWarning($"[BattleManager] Animator on {enemy.gameObject.name} is disabled! Enabling it...");
            animator.enabled = true;
        }

        int enemyNumber = GetEnemyNumber();
        if (enemyNumber <= 0)
        {
            Debug.LogWarning($"[BattleManager] Could not determine enemy number - resolving attack after short delay");
            ResolveEnemyAttackAtImpact();
            yield break;
        }

        // Use state names from EnemyData if set, otherwise fallback to "Enemy X Attack" / "Enemy X Idle"
        string attackAnimationName = GetEnemyAttackStateName(enemyNumber);
        string idleAnimationName = GetEnemyIdleStateName(enemyNumber);
        float impactTime = GetEnemyImpactTime(enemyNumber);

        Debug.Log($"[BattleManager] Playing attack animation: {attackAnimationName}, impact at {impactTime}s");
        animator.Play(attackAnimationName, 0, 0f);
        yield return null;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;
        if (animationLength <= 0.01f)
        {
            Debug.LogWarning($"[BattleManager] Animation '{attackAnimationName}' might not exist. Using default wait.");
            animationLength = 0.5f;
        }

        // Wait until impact frame
        yield return new WaitForSeconds(impactTime);

        // At impact: resolve attack (damage, evasion, guard) and spawn hit FX on player if hit
        ResolveEnemyAttackAtImpact();

        // Wait for the rest of the attack animation
        float remaining = Mathf.Max(0f, animationLength - impactTime);
        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);

        animator.Play(idleAnimationName, 0, 0f);
        Debug.Log($"[BattleManager] Attack animation completed: {attackAnimationName}");
    }

    /// <summary>
    /// Called at the impact moment of the enemy attack: applies damage (or evasion), spawns hit FX on player if not evaded.
    /// </summary>
    private void ResolveEnemyAttackAtImpact()
    {
        AttackResult result = PerformAttack(enemy, player);
        string message;

        if (result.evaded)
        {
            message = $"{player.CharacterName} evaded the attack!";
            Debug.Log(message);
            if (battleUI != null)
                battleUI.ShowBattleLog(message);
            return;
        }

        bool wasGuarding = player.IsGuarding;
        int originalDamage = result.damage;
        int actualDamage = wasGuarding
            ? Mathf.RoundToInt(originalDamage * (1f - player.GuardDamageReduction))
            : originalDamage;

        player.TakeDamage(originalDamage);

        // Spawn hit effect on player at HitPoint
        if (playerHitEffectPrefab != null && player != null)
        {
            Transform spawnPoint = player.HitPoint != null ? player.HitPoint : player.transform;
            GameObject fx = Instantiate(playerHitEffectPrefab, spawnPoint.position, Quaternion.identity);
            fx.transform.SetParent(spawnPoint);
        }

        if (wasGuarding)
        {
            message = result.criticalHit
                ? $"{enemy.CharacterName} lands a CRITICAL HIT! {player.CharacterName} guards and takes {actualDamage} damage!"
                : $"{enemy.CharacterName} attacks! {player.CharacterName} guards and takes {actualDamage} damage!";
        }
        else
        {
            message = result.criticalHit
                ? $"{enemy.CharacterName} lands a CRITICAL HIT for {result.damage} damage!"
                : $"{enemy.CharacterName} attacks for {result.damage} damage!";
        }
        Debug.Log(message);

        if (battleUI != null)
        {
            battleUI.UpdateHealthBars(player, enemy);
            battleUI.ShowBattleLog(message);
        }
    }

    /// <summary>
    /// Extracts the enemy number from the enemy's name or EnemyData (e.g., "Enemy 1" -> 1, "Enemy2" -> 2, "Enemy 3" -> 3)
    /// </summary>
    private int GetEnemyNumber()
    {
        // First, try to get from EnemyData
        if (EncounterManager.Instance != null && EncounterManager.Instance.CurrentEnemyData != null)
        {
            string enemyName = EncounterManager.Instance.CurrentEnemyData.enemyName;
            int number = ExtractNumberFromName(enemyName);
            if (number > 0)
            {
                return number;
            }
        }

        // Fallback: try to extract from enemy Character name
        if (enemy != null)
        {
            return ExtractNumberFromName(enemy.CharacterName);
        }

        // Last fallback: try to extract from GameObject name
        if (enemy != null && enemy.gameObject != null)
        {
            return ExtractNumberFromName(enemy.gameObject.name);
        }

        return 0;
    }

    /// <summary>
    /// Returns the Animator state name for this enemy's attack. Uses EnemyData.attackStateName if set, else "Enemy X Attack".
    /// </summary>
    private string GetEnemyAttackStateName(int enemyNumber)
    {
        if (EncounterManager.Instance != null && EncounterManager.Instance.CurrentEnemyData != null)
        {
            string name = EncounterManager.Instance.CurrentEnemyData.attackStateName;
            if (!string.IsNullOrWhiteSpace(name))
                return name.Trim();
        }
        return $"Enemy {enemyNumber} Attack";
    }

    /// <summary>
    /// Returns the Animator state name for this enemy's idle. Uses EnemyData.idleStateName if set, else "Enemy X Idle".
    /// </summary>
    private string GetEnemyIdleStateName(int enemyNumber)
    {
        if (EncounterManager.Instance != null && EncounterManager.Instance.CurrentEnemyData != null)
        {
            string name = EncounterManager.Instance.CurrentEnemyData.idleStateName;
            if (!string.IsNullOrWhiteSpace(name))
                return name.Trim();
        }
        return $"Enemy {enemyNumber} Idle";
    }

    /// <summary>
    /// Returns the time in seconds when the enemy's attack animation hits (for syncing hit FX and damage).
    /// Enemy 1: 0.27s, Enemy 2: 0.21s, Enemy 3: 0.39s. Add more entries as needed.
    /// </summary>
    private float GetEnemyImpactTime(int enemyNumber)
    {
        switch (enemyNumber)
        {
            case 1: return 0.27f;
            case 2: return 0.21f;
            case 3: return 0.39f;
            default: return 0.25f;
        }
    }

    /// <summary>
    /// Extracts a number from a string (e.g., "Enemy 1" -> 1, "Enemy2" -> 2, "Enemy 3" -> 3)
    /// </summary>
    private int ExtractNumberFromName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return 0;
        }

        // Try to find a number in the name
        string numberString = "";
        foreach (char c in name)
        {
            if (char.IsDigit(c))
            {
                numberString += c;
            }
        }

        if (int.TryParse(numberString, out int number))
        {
            return number;
        }

        return 0;
    }

    /// <summary>
    /// Disables player movement during battle
    /// </summary>
    private void DisablePlayerMovement()
    {
        if (player == null || player.gameObject == null)
        {
            return;
        }

        // Disable PlayerMovement component
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            Debug.Log("[BattleManager] Disabled PlayerMovement component during battle");
        }
        else
        {
            Debug.LogWarning("[BattleManager] PlayerMovement component not found on player GameObject");
        }

        // Also disable Rigidbody2D physics if present (freeze movement)
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
            Debug.Log("[BattleManager] Disabled Rigidbody2D physics during battle");
        }
    }

    // Public method to restart battle (useful for testing)
    public void RestartBattle()
    {
        StartBattle();
    }
}
