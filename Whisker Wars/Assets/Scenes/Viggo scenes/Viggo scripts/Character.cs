using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private int _attack = 15;
    [SerializeField] private string _characterName = "Character";
    
    [Header("Combat Stats")]
    [SerializeField] [Range(0f, 100f)] private float _evasionChance = 20f; // Percentage chance to evade (0-100)
    [SerializeField] [Range(0f, 100f)] private float _criticalHitChance = 15f; // Percentage chance for critical hit (0-100)
    [SerializeField] private float _criticalHitMultiplier = 2f; // Damage multiplier for critical hits
    [SerializeField] private float _damageVariance = 0.2f; // 20% variance (e.g., 10 attack = 8-12 damage)
    
    [Header("Special Attack")]
    [SerializeField] private float _specialAttackMultiplier = 2f; // Damage multiplier for special attack
    [SerializeField] [Range(0f, 100f)] private float _specialAttackAccuracy = 50f; // Hit chance for special attack (0-100)
    
    [Header("Defense")]
    [SerializeField] [Range(0f, 1f)] private float _guardDamageReduction = 0.85f; // Percentage of damage reduced when guarding (0-1)

    [Header("Hit Effect (for combat)")]
    [Tooltip("Spawn position for hit FX when this character is hit. Assign the HitPoint child on the player.")]
    public Transform hitPoint;

    [Header("Damage Flash")]
    [Tooltip("Red tint applied when taking damage (keeps sprite detail visible).")]
    public Color damageFlashColor = new Color(1f, 0.35f, 0.35f);
    [Tooltip("Duration of the damage flash in seconds.")]
    public float damageFlashDuration = 0.15f;

    private int _currentHealth;
    private bool _isGuarding = false;

    public int MaxHealth => _maxHealth;
    public int CurrentHealth => _currentHealth;
    public int Attack => _attack;
    public string CharacterName => _characterName;
    public float EvasionChance => _evasionChance;
    public float CriticalHitChance => _criticalHitChance;
    public float CriticalHitMultiplier => _criticalHitMultiplier;
    public float DamageVariance => _damageVariance;
    public float SpecialAttackMultiplier => _specialAttackMultiplier;
    public float SpecialAttackAccuracy => _specialAttackAccuracy;
    public float GuardDamageReduction => _guardDamageReduction;
    public bool IsGuarding => _isGuarding;
    /// <summary>World position/transform where hit FX should spawn. Can be null.</summary>
    public Transform HitPoint => hitPoint;

    public bool IsAlive => _currentHealth > 0;

    /// <summary>
    /// Plays a red flash on this character's sprite(s) as a filter so details remain visible. Call when damage is applied (e.g. with hit effect).
    /// </summary>
    public void FlashDamage()
    {
        StartCoroutine(FlashDamageCoroutine());
    }

    private IEnumerator FlashDamageCoroutine()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        if (renderers == null || renderers.Length == 0)
            yield break;

        float half = Mathf.Max(0.01f, damageFlashDuration * 0.5f);
        float elapsed = 0f;

        // Flash to red tint
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            Color c = Color.Lerp(Color.white, damageFlashColor, t);
            foreach (var sr in renderers)
            {
                if (sr != null && sr.enabled)
                    sr.color = c;
            }
            yield return null;
        }

        foreach (var sr in renderers)
        {
            if (sr != null && sr.enabled)
                sr.color = damageFlashColor;
        }

        elapsed = 0f;
        // Back to white
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            Color c = Color.Lerp(damageFlashColor, Color.white, t);
            foreach (var sr in renderers)
            {
                if (sr != null && sr.enabled)
                    sr.color = c;
            }
            yield return null;
        }

        foreach (var sr in renderers)
        {
            if (sr != null && sr.enabled)
                sr.color = Color.white;
        }
    }

    private void Awake()
    {
        _currentHealth = _maxHealth;
    }

    public virtual void TakeDamage(int amount, bool ignoreGuard = false)
    {
        // Apply guard damage reduction if guarding
        if (_isGuarding && !ignoreGuard)
        {
            amount = Mathf.RoundToInt(amount * (1f - _guardDamageReduction));
            _isGuarding = false; // Guard is consumed after one hit
        }
        
        _currentHealth = Mathf.Clamp(_currentHealth - amount, 0, _maxHealth);
        
        if (_currentHealth <= 0)
            {
                Die();
            }
        }

    public virtual void SetGuarding(bool guarding)
    {
        _isGuarding = guarding;
    }

    public virtual void Heal(int amount)
    {
        _currentHealth = Mathf.Clamp(_currentHealth + amount, 0, _maxHealth);
    }

    public virtual void ResetHealth()
    {
        _currentHealth = _maxHealth;
        _isGuarding = false;
    }

    /// <summary>
    /// Sets current health (e.g. when restoring state after combat). Clamped to 0..MaxHealth.
    /// </summary>
    public virtual void SetCurrentHealth(int value)
    {
        _currentHealth = Mathf.Clamp(value, 0, _maxHealth);
    }

    protected virtual void Die()
    {
        // Override in derived classes if needed
        // Don't destroy here - let BattleManager handle it
    }

    // Get health as percentage (0-1) for UI sliders
    public float GetHealthPercentage()
    {
        return (float)_currentHealth / _maxHealth;
    }

    /// <summary>
    /// Configures this Character from EnemyData (used when loading enemy from encounter)
    /// </summary>
    public void ConfigureFromEnemyData(EnemyData data)
    {
        Debug.Log($"[Character] ConfigureFromEnemyData called on {gameObject.name} with data: {(data != null ? data.enemyName : "NULL")}");
        
        if (data == null)
        {
            Debug.LogError("[Character] Cannot configure Character - EnemyData is null!");
            return;
        }

        Debug.Log($"[Character] Configuring {gameObject.name} with stats - HP: {data.maxHealth}, Attack: {data.attack}, Name: {data.enemyName}");

        // Use reflection to set private serialized fields
        var type = typeof(Character);
        
        // Set max health
        var maxHealthField = type.GetField("_maxHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (maxHealthField != null)
        {
            maxHealthField.SetValue(this, data.maxHealth);
            Debug.Log($"[Character] Set _maxHealth to {data.maxHealth}");
        }
        else
        {
            Debug.LogError("[Character] Failed to find _maxHealth field!");
        }

        // Set attack
        var attackField = type.GetField("_attack", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (attackField != null)
        {
            attackField.SetValue(this, data.attack);
            Debug.Log($"[Character] Set _attack to {data.attack}");
        }
        else
        {
            Debug.LogError("[Character] Failed to find _attack field!");
        }

        // Set character name
        var nameField = type.GetField("_characterName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (nameField != null)
        {
            nameField.SetValue(this, data.enemyName);
            Debug.Log($"[Character] Set _characterName to {data.enemyName}");
        }

        // Set combat stats
        var evasionField = type.GetField("_evasionChance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (evasionField != null) evasionField.SetValue(this, data.evasionChance);

        var critChanceField = type.GetField("_criticalHitChance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (critChanceField != null) critChanceField.SetValue(this, data.criticalHitChance);

        var critMultiplierField = type.GetField("_criticalHitMultiplier", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (critMultiplierField != null) critMultiplierField.SetValue(this, data.criticalHitMultiplier);

        var varianceField = type.GetField("_damageVariance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (varianceField != null) varianceField.SetValue(this, data.damageVariance);

        // Set special attack stats
        var specialMultiplierField = type.GetField("_specialAttackMultiplier", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (specialMultiplierField != null) specialMultiplierField.SetValue(this, data.specialAttackMultiplier);

        var specialAccuracyField = type.GetField("_specialAttackAccuracy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (specialAccuracyField != null) specialAccuracyField.SetValue(this, data.specialAttackAccuracy);

        // Set defense stats
        var guardReductionField = type.GetField("_guardDamageReduction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (guardReductionField != null) guardReductionField.SetValue(this, data.guardDamageReduction);

        // Reset health to new max health
        Debug.Log($"[Character] Resetting health to max: {data.maxHealth}");
        ResetHealth();
        
        Debug.Log($"[Character] Configuration complete! Character name: {CharacterName}, MaxHealth: {MaxHealth}, Attack: {Attack}");
    }
}
