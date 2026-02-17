using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class PickableItem : MonoBehaviour
{
    public Item item;
    public int quantity = 1;

    [Header("Interaction Settings")]
    public float pickupRange = 2f;

    [Tooltip("Optional unique ID. If empty, uses scene name + object name so this instance stays collected when returning from combat.")]
    [SerializeField] private string pickableID = "";

    [Header("UI References")]
    public GameObject interactPrompt;
    public TextMeshProUGUI promptText;

    private Transform player;
    private bool isInRange = false;
    private PlayerInput playerInput;
    private InputAction interactAction;

    void Start()
    {
        // If we already collected this pickable this session (e.g. before combat), don't respawn it
        if (EncounterManager.Instance != null && EncounterManager.Instance.IsPickableCollected(GetPickableId()))
        {
            Destroy(gameObject);
            return;
        }

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
            if (EncounterManager.Instance != null)
                EncounterManager.Instance.RegisterCollectedPickable(GetPickableId());
            Destroy(gameObject);
        }
    }

    private string GetPickableId()
    {
        if (!string.IsNullOrEmpty(pickableID)) return pickableID;
        return SceneManager.GetActiveScene().name + "_" + gameObject.name;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}