using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory Settings")]
    public int inventorySize = 20;
    public bool toggleInventoryWithKey = true;

    [Header("UI References")]
    public GameObject inventoryPanel;
    public Transform slotsParent;
    public GameObject slotPrefab;

    [Header("Item Details Panel")]
    public GameObject detailsPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public UnityEngine.UI.Image itemIconLarge;

    [Header("Notification")]
    public TextMeshProUGUI notificationText;
    public float notificationDuration = 2f;

    private List<InventorySlot> slots = new List<InventorySlot>();
    private float notificationTimer = 0f;
    private InputAction toggleInventoryAction;

    // Helper class used to expose item stacks (item + total quantity)
    public class InventoryItemStack
    {
        public Item item;
        public int quantity;

        public InventoryItemStack(Item item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Persist inventory across scene loads so items are available in battle
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeInventory();

        if (detailsPanel != null)
        {
            detailsPanel.SetActive(false);
        }

        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }

        // Try to get PlayerInput component for toggle action
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerInput playerInput = player.GetComponent<PlayerInput>();
            if (playerInput != null && playerInput.actions != null)
            {
                toggleInventoryAction = playerInput.actions.FindAction("ToggleInventory");
            }
        }
    }

    void Update()
    {
        // Handle notification timer
        if (notificationTimer > 0)
        {
            notificationTimer -= Time.deltaTime;
            if (notificationTimer <= 0 && notificationText != null)
            {
                notificationText.gameObject.SetActive(false);
            }
        }

        // Toggle inventory with key (works with both input systems)
        if (toggleInventoryWithKey && inventoryPanel != null)
        {
            bool togglePressed = false;

            // New Input System
            if (toggleInventoryAction != null && toggleInventoryAction.triggered)
            {
                togglePressed = true;
            }
            // Fallback to Keyboard.current
            else if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
            {
                togglePressed = true;
            }

            if (togglePressed)
            {
                inventoryPanel.SetActive(!inventoryPanel.activeSelf);
            }
        }
    }

    void InitializeInventory()
    {
        for (int i = 0; i < inventorySize; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsParent);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            slots.Add(slot);
            slot.ClearSlot();
        }
    }

    public bool AddItem(Item item, int quantity)
    {
        // Check if item already exists and can stack
        if (item.maxStack > 1)
        {
            foreach (InventorySlot slot in slots)
            {
                if (slot.item == item && slot.quantity < item.maxStack)
                {
                    int spaceLeft = item.maxStack - slot.quantity;
                    int amountToAdd = Mathf.Min(spaceLeft, quantity);

                    slot.AddQuantity(amountToAdd);
                    quantity -= amountToAdd;

                    if (quantity <= 0)
                    {
                        ShowNotification($"Picked up {item.itemName}");
                        return true;
                    }
                }
            }
        }

        // Add to empty slots
        while (quantity > 0)
        {
            InventorySlot emptySlot = FindEmptySlot();

            if (emptySlot != null)
            {
                int amountToAdd = Mathf.Min(item.maxStack, quantity);
                emptySlot.SetItem(item, amountToAdd);
                quantity -= amountToAdd;
            }
            else
            {
                ShowNotification("Inventory is full!");
                return false;
            }
        }

        ShowNotification($"Picked up {item.itemName}");
        return true;
    }

    public bool RemoveItem(Item item, int quantity)
    {
        int remaining = quantity;

        for (int i = slots.Count - 1; i >= 0; i--)
        {
            if (slots[i].item == item)
            {
                if (slots[i].quantity >= remaining)
                {
                    slots[i].AddQuantity(-remaining);
                    if (slots[i].quantity <= 0)
                    {
                        slots[i].ClearSlot();
                    }
                    return true;
                }
                else
                {
                    remaining -= slots[i].quantity;
                    slots[i].ClearSlot();
                }
            }
        }

        return remaining <= 0;
    }

    InventorySlot FindEmptySlot()
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.item == null)
            {
                return slot;
            }
        }
        return null;
    }

    public void ShowItemDetails(Item item, int quantity)
    {
        if (detailsPanel == null) return;

        detailsPanel.SetActive(true);
        itemNameText.text = item.itemName;
        itemDescriptionText.text = item.description;
        itemIconLarge.sprite = item.icon;
    }

    public void HideItemDetails()
    {
        if (detailsPanel != null)
        {
            detailsPanel.SetActive(false);
        }
    }

    void ShowNotification(string message)
    {
        if (notificationText != null)
        {
            notificationText.text = message;
            notificationText.gameObject.SetActive(true);
            notificationTimer = notificationDuration;
        }
    }

    public int GetItemCount(Item item)
    {
        int count = 0;
        foreach (InventorySlot slot in slots)
        {
            if (slot.item == item)
            {
                count += slot.quantity;
            }
        }
        return count;
    }

    /// <summary>
    /// Returns a list of all battle-usable items (grouped by item type with total quantity).
    /// Used by the turn-based combat UI to populate the Items menu.
    /// </summary>
    public List<InventoryItemStack> GetBattleUsableItems()
    {
        List<InventoryItemStack> result = new List<InventoryItemStack>();
        Dictionary<Item, int> counts = new Dictionary<Item, int>();

        foreach (InventorySlot slot in slots)
        {
            if (slot.item == null)
            {
                continue;
            }

            Item item = slot.item;

            // Only include items that are marked as usable in battle
            if (!item.canUseInBattle)
            {
                continue;
            }

            if (!counts.ContainsKey(item))
            {
                counts[item] = 0;
            }

            counts[item] += slot.quantity;
        }

        foreach (KeyValuePair<Item, int> kvp in counts)
        {
            if (kvp.Value > 0)
            {
                result.Add(new InventoryItemStack(kvp.Key, kvp.Value));
            }
        }

        return result;
    }
}