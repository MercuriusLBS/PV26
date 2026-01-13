using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

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

        // Calculate movement direction (only X or Y, never both)
        Vector3 movement = new Vector3(horizontal, vertical, 0f);

        // Move the player at constant speed
        transform.Translate(movement * moveSpeed * Time.deltaTime);
    }
}