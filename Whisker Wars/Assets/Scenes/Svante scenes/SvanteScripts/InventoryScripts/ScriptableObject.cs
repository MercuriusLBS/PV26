using UnityEngine;

public enum ItemType
{
    Generic,
    Healing
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public Sprite icon;
    [TextArea]
    public string description;
    public int maxStack = 1;

    [Header("Item Type")]
    public ItemType itemType = ItemType.Generic;

    [Header("Healing Settings")]
    [Tooltip("Amount of HP this item heals when used in battle")]
    public int healAmount = 0;

    [Tooltip("Can this item be used during turn-based combat?")]
    public bool canUseInBattle = false;
}