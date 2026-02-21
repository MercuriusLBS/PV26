using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName = "Enemy";
    
    [Header("Visuals")]
    [Tooltip("Sprite for this enemy type")]
    public Sprite enemySprite;
    [Tooltip("Animator Controller for this enemy type (optional)")]
    public RuntimeAnimatorController enemyAnimatorController;
    [Tooltip("Exact state name in the Animator for attack (e.g. Attack or Enemy 1 Attack). If empty, BattleManager uses \"Enemy X Attack\" from enemy number.")]
    public string attackStateName = "";
    [Tooltip("Exact state name in the Animator for idle (e.g. Idle or Enemy 1 Idle). If empty, BattleManager uses \"Enemy X Idle\" from enemy number.")]
    public string idleStateName = "";
    
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
