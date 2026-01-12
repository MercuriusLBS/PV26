using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedAspectRatioCamera : MonoBehaviour
{
    // Target aspect ratio (16:10)
    private const float TARGET_ASPECT = 16f / 10f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        ApplyAspect();
    }

    private void OnValidate()
    {
        // Ensures correct behavior in Editor when resizing Game view
        if (cam == null)
            cam = GetComponent<Camera>();

        ApplyAspect();
    }

    private void ApplyAspect()
    {
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / TARGET_ASPECT;

        Rect rect = cam.rect;

        if (scaleHeight < 1.0f)
        {
            // Add letterbox (top and bottom)
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        }
        else
        {
            // Add pillarbox (left and right)
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
        }

        cam.rect = rect;
    }
}
