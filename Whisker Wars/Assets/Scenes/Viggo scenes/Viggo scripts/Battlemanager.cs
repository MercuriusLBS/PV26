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
    [SerializeField] private Character enemy;

    [Header("Battle Settings")]
    [SerializeField] private float turnDelay = 1f; // Delay between turns for visual feedback

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

    private void Awake()
    {
        battleUI = FindFirstObjectByType<BattleUI>();
        if (battleUI == null)
        {
            Debug.LogWarning("BattleUI not found! UI updates will not work.");
        }
    }

    private void Start()
    {
        StartBattle();
    }

    public void StartBattle()
    {
        if (player == null || enemy == null)
        {
            Debug.LogError("Player or Enemy not assigned in BattleManager!");
            return;
        }

        // Reset health and cooldowns if needed
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

        // Perform attack with all combat mechanics
        AttackResult result = PerformAttack(enemy, player);

        // Handle attack result
        if (result.evaded)
        {
            string message = $"{player.CharacterName} evaded the attack!";
            Debug.Log(message);
            if (battleUI != null)
            {
                battleUI.ShowBattleLog(message);
            }
        }
        else
        {
            // Check if player was guarding and calculate actual damage
            bool wasGuarding = player.IsGuarding;
            int originalDamage = result.damage;
            int actualDamage = wasGuarding 
                ? Mathf.RoundToInt(originalDamage * (1f - player.GuardDamageReduction)) 
                : originalDamage;
            
            player.TakeDamage(originalDamage);
            
            string message;
            if (wasGuarding)
            {
                // Guard reduced damage
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

        // Check if player is defeated
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
        currentState = BattleState.BattleEnd;

        if (battleUI != null)
        {
            battleUI.SetActionMenuActive(false);
        }

        if (playerWon)
        {
            Debug.Log($"{player.CharacterName} wins the battle!");
            if (battleUI != null)
            {
                battleUI.ShowBattleLog($"{player.CharacterName} wins!");
            }
        }
        else
        {
            Debug.Log($"{enemy.CharacterName} wins the battle!");
            if (battleUI != null)
            {
                battleUI.ShowBattleLog($"{enemy.CharacterName} wins!");
            }
        }

        // Here you can add: return to overworld, give rewards, etc.
    }

    // Public method to restart battle (useful for testing)
    public void RestartBattle()
    {
        StartBattle();
    }
}
