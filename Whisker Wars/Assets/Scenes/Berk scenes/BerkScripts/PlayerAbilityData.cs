using UnityEngine;

/// <summary>
/// Stores player abilities that persist across scenes
/// </summary>
public class PlayerAbilityData : MonoBehaviour
{
    public static PlayerAbilityData Instance;

    [Header("Abilities")]
    public bool hasConfidence = false; // Attack twice in one round
    public bool hasFleetFootwork = false; // +15% evasion
    public bool hasPiercingStrike = false; // 5-20 HP damage skill
    public bool hasCriticalStrike = false; // +15% crit chance
    public bool hasRush = false; // 1.5x overworld movement speed

    [Header("Items to Receive")]
    public int herbsToReceive = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Resets all abilities and items (useful for testing)
    /// </summary>
    public void ResetData()
    {
        hasConfidence = false;
        hasFleetFootwork = false;
        hasPiercingStrike = false;
        hasCriticalStrike = false;
        hasRush = false;
        herbsToReceive = 0;
    }

    /// <summary>
    /// Grants an ability to the player
    /// </summary>
    public void GrantAbility(string abilityName)
    {
        switch (abilityName)
        {
            case "Confidence":
                hasConfidence = true;
                Debug.Log("[PlayerAbilityData] Granted Confidence ability");
                break;
            case "FleetFootwork":
                hasFleetFootwork = true;
                Debug.Log("[PlayerAbilityData] Granted Fleet Footwork ability");
                break;
            case "PiercingStrike":
                hasPiercingStrike = true;
                Debug.Log("[PlayerAbilityData] Granted Piercing Strike ability");
                break;
            case "CriticalStrike":
                hasCriticalStrike = true;
                Debug.Log("[PlayerAbilityData] Granted Critical Strike ability");
                break;
            case "Rush":
                hasRush = true;
                Debug.Log("[PlayerAbilityData] Granted Rush ability");
                break;
            default:
                Debug.LogWarning($"[PlayerAbilityData] Unknown ability: {abilityName}");
                break;
        }
    }

    /// <summary>
    /// Adds herbs to receive when entering the main game
    /// </summary>
    public void AddHerbs(int amount)
    {
        herbsToReceive += amount;
        Debug.Log($"[PlayerAbilityData] Added {amount} herbs. Total: {herbsToReceive}");
    }
}