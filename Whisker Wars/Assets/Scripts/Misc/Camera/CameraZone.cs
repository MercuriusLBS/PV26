using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class CameraZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        CinemachineConfiner2D confiner =
            FindFirstObjectByType<CinemachineConfiner2D>();

        if (confiner == null)
            return;

        confiner.BoundingShape2D = GetComponent<Collider2D>();
        confiner.InvalidateBoundingShapeCache();
    }
}