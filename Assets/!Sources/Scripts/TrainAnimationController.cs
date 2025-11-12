// --------------------------------------------------------------
// Creation Date: 2025-11-09 22:40
// Author: ZQlie
// Description: -
// --------------------------------------------------------------

using UnityEngine;

public class TrainAnimationController : MonoBehaviour
{
    [Header("Train Sprites")]
    [SerializeField] private Sprite trainRight; // Train facing right
    [SerializeField] private Sprite trainDown;  // Train facing down
    
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private Vector2 currentDirection = Vector2.right;
    
    private void Awake()
    {
        // Get sprite renderer if not assigned
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("SpriteRenderer not found on train!");
            }
        }
    }
    
    /// <summary>
    /// Updates the train sprite based on movement direction
    /// </summary>
    /// <param name="direction">The direction the train is moving (normalized Vector2)</param>
    public void UpdateDirection(Vector2 direction)
    {
        if (direction == Vector2.zero || spriteRenderer == null)
            return;
        
        currentDirection = direction;
        
        // Determine which sprite to use and how to flip it
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Moving horizontally - use right sprite
            spriteRenderer.sprite = trainRight;
            
            if (direction.x > 0)
            {
                // Moving right - no flip
                spriteRenderer.flipX = false;
                spriteRenderer.flipY = false;
            }
            else
            {
                // Moving left - flip horizontally
                spriteRenderer.flipX = true;
                spriteRenderer.flipY = false;
            }
        }
        else
        {
            // Moving vertically - use down sprite
            spriteRenderer.sprite = trainDown;
            
            if (direction.y < 0)
            {
                // Moving down - no flip
                spriteRenderer.flipX = false;
                spriteRenderer.flipY = false;
            }
            else
            {
                // Moving up - flip vertically
                spriteRenderer.flipX = false;
                spriteRenderer.flipY = true;
            }
        }
    }
    
    /// <summary>
    /// Gets the current movement direction
    /// </summary>
    public Vector2 GetCurrentDirection()
    {
        return currentDirection;
    }
}