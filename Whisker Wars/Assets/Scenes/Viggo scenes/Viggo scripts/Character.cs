using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private int _attack = 10;
    [SerializeField] private string _characterName = "Character";

    private int _currentHealth;

    public int MaxHealth => _maxHealth;
    public int CurrentHealth => _currentHealth;
    public int Attack => _attack;
    public string CharacterName => _characterName;

    public bool IsAlive => _currentHealth > 0;

    private void Awake()
    {
        _currentHealth = _maxHealth;
    }

    public virtual void TakeDamage(int amount)
    {
        _currentHealth = Mathf.Clamp(_currentHealth - amount, 0, _maxHealth);
        
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Heal(int amount)
    {
        _currentHealth = Mathf.Clamp(_currentHealth + amount, 0, _maxHealth);
    }

    public virtual void ResetHealth()
    {
        _currentHealth = _maxHealth;
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
