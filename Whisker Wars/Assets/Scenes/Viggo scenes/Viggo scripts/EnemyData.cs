using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName = "Enemy";
    
    [Header("Stats")]
    public int maxHealth = 100;
    public int attack = 10;
    
    [Header("Combat Stats")]
    [Range(0f, 100f)] public float evasionChance = 20f;
    [Range(0f, 100f)] public float criticalHitChance = 15f;
    public float criticalHitMultiplier = 2f;
    public float damageVariance = 0.2f;
    
    [Header("Special Attack")]
    public float specialAttackMultiplier = 2f;
    [Range(0f, 100f)] public float specialAttackAccuracy = 50f;
    
    [Header("Defense")]
    [Range(0f, 1f)] public float guardDamageReduction = 0.85f;
}
