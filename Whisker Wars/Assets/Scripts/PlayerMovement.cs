using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    // Update is called once per frame
    void Update()
    {
        // Get input from arrow keys or WASD
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Calculate movement direction
        Vector3 movement = new Vector3(horizontal, vertical, 0f);

        // Move the player at constant speed
        transform.Translate(movement * moveSpeed * Time.deltaTime);
    }
}
