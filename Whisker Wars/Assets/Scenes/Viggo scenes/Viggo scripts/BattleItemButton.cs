using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI helper for a single item entry in the battle Items submenu.
/// </summary>
public class BattleItemButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemQuantityText;
    [SerializeField] private Image itemIconImage;
    [SerializeField] private Button button;

    private Item item;
    private int quantity;
    private Battlemanager battleManager;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.AddListener(OnClicked);
        }
    }

    /// <summary>
    /// Initializes this button with item data and a reference to the Battlemanager.
    /// </summary>
    public void Initialize(Item newItem, int newQuantity, Battlemanager manager)
    {
        item = newItem;
        quantity = newQuantity;
        battleManager = manager;

        if (itemNameText != null && item != null)
        {
            itemNameText.text = item.itemName;
        }

        if (itemQuantityText != null)
        {
            itemQuantityText.text = $"x{quantity}";
        }

        if (itemIconImage != null && item != null)
        {
            itemIconImage.sprite = item.icon;
            itemIconImage.enabled = item.icon != null;
        }
    }

    private void OnClicked()
    {
        if (battleManager != null && item != null)
        {
            battleManager.PlayerUseItem(item);
        }
    }
}

