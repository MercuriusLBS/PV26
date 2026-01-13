using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public Item item;
    public int quantity;

    [Header("UI References")]
    public Image icon;
    public TextMeshProUGUI quantityText;
    public Button slotButton;

    void Start()
    {
        if (slotButton != null)
        {
            slotButton.onClick.AddListener(OnSlotClicked);
        }
    }

    public void SetItem(Item newItem, int newQuantity)
    {
        item = newItem;
        quantity = newQuantity;

        if (item != null)
        {
            icon.sprite = item.icon;
            icon.enabled = true;

            if (quantity > 1)
            {
                quantityText.text = quantity.ToString();
                quantityText.enabled = true;
            }
            else
            {
                quantityText.enabled = false;
            }
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        item = null;
        quantity = 0;
        icon.sprite = null;
        icon.enabled = false;
        quantityText.enabled = false;
    }

    public void AddQuantity(int amount)
    {
        quantity += amount;
        quantityText.text = quantity.ToString();

        if (quantity <= 1)
        {
            quantityText.enabled = false;
        }
        else
        {
            quantityText.enabled = true;
        }
    }

    void OnSlotClicked()
    {
        if (item != null)
        {
            InventoryManager.Instance.ShowItemDetails(item, quantity);
        }
    }
}