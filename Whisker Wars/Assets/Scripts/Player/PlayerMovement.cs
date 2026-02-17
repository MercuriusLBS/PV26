using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Animator animator;
    private Rigidbody2D rb;
    private Vector2 movement;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // Get input from arrow keys or WASD using new Input System
        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                horizontal -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                horizontal += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                vertical -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                vertical += 1f;
        }

        // Prevent diagonal movement - prioritize horizontal over vertical
        if (horizontal != 0f)
        {
            vertical = 0f;
        }

        // Set movement vector for animator & physics
        movement.x = horizontal;
        movement.y = vertical;

        // Check if player is moving
        bool isMoving = movement.sqrMagnitude > 0;
        animator.SetBool("IsMoving", isMoving);

        // Set animator parameters for movement direction
        if (isMoving)
        {
            animator.SetFloat("MoveX", movement.x);
            animator.SetFloat("MoveY", movement.y);
        }
    }

    void FixedUpdate()
    {
        // Use Rigidbody2D movement so collisions work correctly
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}