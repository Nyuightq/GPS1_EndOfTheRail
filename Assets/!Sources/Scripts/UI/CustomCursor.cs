// --------------------------------------------------------------
// Creation Date: 2025-11-27 17:56
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    [Header("Assign one (Texture2D or Sprite)")]
    public Texture2D cursorTexture;
    public Sprite cursorSprite;

    [Header("Behavior")]
    public bool forceSoftware = true;   // use ForceSoftware to avoid hardware-size issues
    public bool centerHotspot = true;   // center hotspot by default
    public Vector2 hotspot = Vector2.zero;

    void Start()
    {
        // Choose texture from either field
        Texture2D tex = cursorTexture;

        // If Sprite provided, extract its rect pixels (handles sprites inside atlases)
        if (tex == null && cursorSprite != null)
        {
            Rect r = cursorSprite.textureRect;
            try
            {
                tex = new Texture2D((int)r.width, (int)r.height, TextureFormat.RGBA32, false);
                Color[] pixels = cursorSprite.texture.GetPixels(
                    (int)r.x, (int)r.y, (int)r.width, (int)r.height
                );
                tex.SetPixels(pixels);
                tex.Apply();
            }
            catch (UnityException e)
            {
                Debug.LogWarning("Failed to read sprite texture pixels. Make sure the source texture has Read/Write enabled. Exception: " + e.Message);
            }
        }

        if (tex == null)
        {
            Debug.LogWarning("ReplaceSystemCursor: No cursor texture assigned. Please attach a Texture2D or Sprite in the inspector.");
            return;
        }

        // Compute hotspot
        Vector2 hs = centerHotspot ? new Vector2(tex.width / 2f, tex.height / 2f) : hotspot;

        // Ensure cursor is visible and unlocked for testing
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        CursorMode mode = forceSoftware ? CursorMode.ForceSoftware : CursorMode.Auto;

        Cursor.SetCursor(tex, hs, mode);

        Debug.Log($"ReplaceSystemCursor: Set cursor -> name:{tex.name} size:{tex.width}x{tex.height} hotspot:{hs} mode:{mode}");
    }

    void OnDisable()
    {
        // Reset cursor when script disabled
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Cursor.visible = true;
    }
}
