using UnityEngine;

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

    public bool IsAlive => _currentHealth > 0;

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
}
