using UnityEngine;
using System.Reflection;

/// <summary>
/// Manages player abilities and applies them to the Character component
/// Attach this to the Player GameObject in the main game scene
/// </summary>
public class AbilityManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Character playerCharacter;
    [SerializeField] private GameObject herbPrefab; // Assign the herb pickup prefab
    [SerializeField] private Transform[] herbSpawnPoints; // Where to spawn herbs

    [Header("Movement Settings")]
    [SerializeField] private float baseMovementSpeed = 5f;
    private float movementSpeedMultiplier = 1f;

    private PlayerAbilityData abilityData;
    private bool abilitiesApplied = false;

    void Start()
    {
        // Get references
        if (playerCharacter == null)
            playerCharacter = GetComponent<Character>();

        abilityData = PlayerAbilityData.Instance;

        if (abilityData != null && !abilitiesApplied)
        {
            ApplyAbilities();
            SpawnHerbs();
            abilitiesApplied = true;
        }
    }

    void ApplyAbilities()
    {
        if (abilityData == null || playerCharacter == null)
        {
            Debug.LogWarning("[AbilityManager] Missing references, cannot apply abilities");
            return;
        }

        // Apply Fleet Footwork (+15% evasion)
        if (abilityData.hasFleetFootwork)
        {
            ModifyEvasion(15f);
            Debug.Log("[AbilityManager] Applied Fleet Footwork: +15% evasion");
        }

        // Apply Critical Strike (+15% crit chance)
        if (abilityData.hasCriticalStrike)
        {
            ModifyCriticalChance(15f);
            Debug.Log("[AbilityManager] Applied Critical Strike: +15% crit chance");
        }

        // Apply Rush (1.5x movement speed)
        if (abilityData.hasRush)
        {
            movementSpeedMultiplier = 1.5f;
            ApplyMovementSpeed();
            Debug.Log("[AbilityManager] Applied Rush: 1.5x movement speed");
        }

        // Confidence and Piercing Strike are checked during combat
        if (abilityData.hasConfidence)
        {
            Debug.Log("[AbilityManager] Player has Confidence ability (attack twice)");
        }

        if (abilityData.hasPiercingStrike)
        {
            Debug.Log("[AbilityManager] Player has Piercing Strike ability");
        }
    }

    void ModifyEvasion(float amount)
    {
        var type = typeof(Character);
        var evasionField = type.GetField("_evasionChance", BindingFlags.NonPublic | BindingFlags.Instance);

        if (evasionField != null)
        {
            float currentEvasion = (float)evasionField.GetValue(playerCharacter);
            float newEvasion = Mathf.Clamp(currentEvasion + amount, 0f, 100f);
            evasionField.SetValue(playerCharacter, newEvasion);
            Debug.Log($"[AbilityManager] Evasion changed from {currentEvasion}% to {newEvasion}%");
        }
    }

    void ModifyCriticalChance(float amount)
    {
        var type = typeof(Character);
        var critField = type.GetField("_criticalHitChance", BindingFlags.NonPublic | BindingFlags.Instance);

        if (critField != null)
        {
            float currentCrit = (float)critField.GetValue(playerCharacter);
            float newCrit = Mathf.Clamp(currentCrit + amount, 0f, 100f);
            critField.SetValue(playerCharacter, newCrit);
            Debug.Log($"[AbilityManager] Critical chance changed from {currentCrit}% to {newCrit}%");
        }
    }

    void ApplyMovementSpeed()
    {
        // This modifies movement speed - adjust based on your movement system
        // If you're using a PlayerMovement script, you'll need to modify it there
        // For now, this stores the multiplier which can be accessed by other scripts
        Debug.Log($"[AbilityManager] Movement speed multiplier set to {movementSpeedMultiplier}");
    }

    void SpawnHerbs()
    {
        if (abilityData.herbsToReceive > 0 && herbPrefab != null && herbSpawnPoints.Length > 0)
        {
            int herbsToSpawn = Mathf.Min(abilityData.herbsToReceive, herbSpawnPoints.Length);

            for (int i = 0; i < herbsToSpawn; i++)
            {
                if (herbSpawnPoints[i] != null)
                {
                    Instantiate(herbPrefab, herbSpawnPoints[i].position, Quaternion.identity);
                    Debug.Log($"[AbilityManager] Spawned herb at position {herbSpawnPoints[i].position}");
                }
            }

            // Reset herbs count
            abilityData.herbsToReceive = 0;
        }
    }

    // Public methods for combat system to check abilities
    public bool HasConfidence()
    {
        return abilityData != null && abilityData.hasConfidence;
    }

    public bool HasPiercingStrike()
    {
        return abilityData != null && abilityData.hasPiercingStrike;
    }

    public float GetMovementSpeedMultiplier()
    {
        return movementSpeedMultiplier;
    }

    /// <summary>
    /// Calculates damage for Piercing Strike ability (5-20 HP)
    /// </summary>
    public int CalculatePiercingStrikeDamage()
    {
        return Random.Range(5, 21); // 5-20 inclusive
    }
}