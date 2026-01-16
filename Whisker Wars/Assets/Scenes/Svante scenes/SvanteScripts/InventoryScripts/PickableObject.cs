using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PickableItem : MonoBehaviour
{
    public Item item;
    public int quantity = 1;

    [Header("Interaction Settings")]
    public float pickupRange = 2f;

    [Header("UI References")]
    public GameObject interactPrompt;
    public TextMeshProUGUI promptText;

    private Transform player;
    private bool isInRange = false;
    private PlayerInput playerInput;
    private InputAction interactAction;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Get the PlayerInput component from the player
        if (player != null)
        {
            playerInput = player.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                interactAction = playerInput.actions["Interact"];
            }
        }

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }

        if (promptText != null)
        {
            promptText.text = $"Press E to pick up {item.itemName}";
        }
    }

    void Update()
    {
        Debug.Log($"Icon set: {item.icon.name}");
        float dist = Vector2.Distance(transform.position, player.position);
        isInRange = dist <= pickupRange;

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(isInRange);
        }

        // Use new Input System if available, fallback to old system
        if (isInRange)
        {
            if (interactAction != null && interactAction.triggered)
            {
                PickUp();
            }
            else if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                PickUp();
            }
        }
    }

    void PickUp()
    {
        bool wasPickedUp = InventoryManager.Instance.AddItem(item, quantity);

        if (wasPickedUp)
        {
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}