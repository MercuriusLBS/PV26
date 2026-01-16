using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedAspectRatioCamera : MonoBehaviour
{
    [Header("Target Resolution (pixels)")]
    [Tooltip("Target pixel resolution width (e.g. 256 for a 256x160 game)")]
    [SerializeField] private int targetWidth = 256;
    
    [Tooltip("Target pixel resolution height (e.g. 160 for a 256x160 game)")]
    [SerializeField] private int targetHeight = 160;
    
    [Header("Pixel Perfect Settings")]
    [Tooltip("Pixels per unit in your game (e.g., 32 for 32px sprites)")]
    [SerializeField] private int pixelsPerUnit = 32;

    private Camera cam;
    private float targetAspect;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        CalculateTargetAspect();
        ApplyAspect();
    }

    private void OnValidate()
    {
        // Ensures correct behavior in Editor when resizing Game view
        if (cam == null)
            cam = GetComponent<Camera>();

        CalculateTargetAspect();
        ApplyAspect();
    }

    private void Start()
    {
        // Handle screen resolution changes at runtime
        ApplyAspect();
    }

    private void CalculateTargetAspect()
    {
        if (targetWidth > 0 && targetHeight > 0)
        {
            targetAspect = (float)targetWidth / targetHeight;
        }
        else
        {
            // Fallback to 16:10 (8:5) if invalid values
            targetAspect = 256f / 160f;
        }
    }

    private void ApplyAspect()
    {
        if (cam == null) return;

        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Rect rect = cam.rect;

        if (scaleHeight < 1.0f)
        {
            // Add letterbox (black bars top and bottom)
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        }
        else
        {
            // Add pillarbox (black bars left and right)
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
        }

        cam.rect = rect;
    }

    /// <summary>
    /// Calculates the recommended orthographic size for pixel-perfect rendering.
    /// Formula: (targetHeight / pixelsPerUnit) / 2
    /// </summary>
    public float GetRecommendedOrthographicSize()
    {
        return (targetHeight / (float)pixelsPerUnit) / 2f;
    }
}
