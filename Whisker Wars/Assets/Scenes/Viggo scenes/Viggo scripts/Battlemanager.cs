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

        // Reset health if needed
        player.ResetHealth();
        enemy.ResetHealth();

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

        // Calculate damage
        int damage = player.Attack;
        enemy.TakeDamage(damage);

        Debug.Log($"{player.CharacterName} attacks {enemy.CharacterName} for {damage} damage!");
        
        // Update UI
        if (battleUI != null)
        {
            battleUI.UpdateHealthBars(player, enemy);
            battleUI.ShowBattleLog($"{player.CharacterName} attacks for {damage} damage!");
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

        // Enemy attacks
        int damage = enemy.Attack;
        player.TakeDamage(damage);

        Debug.Log($"{enemy.CharacterName} attacks {player.CharacterName} for {damage} damage!");

        // Update UI
        if (battleUI != null)
        {
            battleUI.UpdateHealthBars(player, enemy);
            battleUI.ShowBattleLog($"{enemy.CharacterName} attacks for {damage} damage!");
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
        
        if (battleUI != null)
        {
            battleUI.SetActionMenuActive(true);
        }
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
