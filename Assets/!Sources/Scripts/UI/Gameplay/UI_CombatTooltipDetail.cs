// --------------------------------------------------------------
// Creation Date: 2025-10-06 21:09
// Author: nyuig
// Description: Tooltip that displays combat entity details,
//              automatically adapts its size, and is clamped to the screen edge.
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System;

public class UI_CombatTooltipDetail : MonoBehaviour
{
    public static UI_CombatTooltipDetail Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private RectTransform tooltipRectTransform; // Tooltip background/container
    [SerializeField] private TextMeshProUGUI entityNameText;
    [SerializeField] private TextMeshProUGUI infoText;

    [Header("Positioning & Clamping")]
    [SerializeField] private Vector3 offset = new Vector3(20, -20, 0); // Offset from the mouse cursor
    [SerializeField] private float padding = 5f;                         // Padding from the screen edges

    private CombatEntity _entity;
    private Canvas canvas; // Reference to the parent canvas

    private void Awake()
    {
        // Enforce Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Get necessary references
        canvas = GetComponentInParent<Canvas>();
        if (!tooltipRectTransform)
        {
            tooltipRectTransform = GetComponent<RectTransform>();
        }
        
        Hide();
    }

    private void Update()
    {
        if (tooltipRectTransform.gameObject.activeSelf)
        {
            // 1. Update dynamic stats (HP)
            // Note: Rebuilding the entire text every frame is costly. 
            // For production, consider using a separate component/listener for HP if required.
            // For this implementation, we re-show the data to update the HP text in real-time.
            if (_entity.IsDead && !_entity.IsComponent)
            {
                Hide();
                _entity = null;
            }
            
            if (_entity != null)
            {
                UpdateStatsText();
            }
           

            // 2. Clamp position to follow the mouse and stay within screen bounds
            FollowMouseAndClamp();
        }
    }

    /// <summary>
    /// Updates the infoText with the current entity stats. Called on Show and in Update for real-time HP.
    /// </summary>
    private void UpdateStatsText()
    {
        if (_entity == null) return;

        // Use StringBuilder to format the entire content
        StringBuilder infoBuilder = new StringBuilder();

        // 1. Entity Name (Larger size)
        entityNameText.text = $"<size=110%>{_entity.entityName}</size>\n";

        // 2. Current Stats
        // HP is updated every frame
        if (IsValidAttr(_entity.CurrentHp) && IsValidAttr(_entity.MaxHp))
            infoBuilder.Append($"HP: <color=#77FF77>{_entity.CurrentHp}</color> / {_entity.MaxHp}\n");
        
        // Other stats
        if (IsValidAttr(_entity.Defense))
            infoBuilder.Append($"DEFENSE: <color=#77FF77>{_entity.Defense}</color>\n");
        if (IsValidAttr(_entity.AttackDamage))
        {
            if (IsValidAttr(_entity.AttackDamageVariance))
            {
                int maxAtk = _entity.AttackDamage + _entity.AttackDamageVariance;
                int minAtk = Math.Max(_entity.AttackDamage - _entity.AttackDamageVariance, 1);

                infoBuilder.Append($"POWER: <color=#C23753>{minAtk}~{maxAtk}</color>\n");
            }
            else
            {
                infoBuilder.Append($"POWER: <color=#C23753>{_entity.AttackDamage}</color>\n");
            }
        }
        if (IsValidAttr(_entity.AttackSpeed))
        {
            var colorCode = "FFFFFF";
            if (_entity.AttackSpeed <= 2) colorCode = "C23753";
            if (_entity.AttackSpeed >= 6) colorCode = "EBB85B";
            infoBuilder.Append($"SPEED: <color=#{colorCode}>{_entity.AttackSpeed}</color>\n"); // Format speed if it's a float
        }

        infoText.text = infoBuilder.ToString();
    }
    
    private bool IsValidAttr(int value)
    {
        if (value <= 0) 
            return false;

        return true;
    }

    public void Show(CombatEntity entity)
    {
        if (entity == null)
        {
            Hide();
            return;
        }

        _entity = entity;
        UpdateStatsText();

        // Activate the object
        gameObject.SetActive(true);

        // Crucial step: Force the layout group (if present) and the RectTransform 
        // to update immediately based on the new text content. This ensures the 
        // bounding box (rect.width/height) is correct for clamping calculations.
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRectTransform);

        // Set initial position and clamp
        FollowMouseAndClamp();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        _entity = null;
    }

    /// <summary>
    /// Moves the tooltip to follow the mouse and adjusts its position to prevent it from going off-screen.
    /// </summary>
    private void FollowMouseAndClamp()
    {
        // We need the width and height of the tooltip in screen pixels.
        // rect.width/height is in Canvas Local Space. We must multiply by the Canvas scale factor.
        float width = tooltipRectTransform.rect.width * canvas.scaleFactor;
        float height = tooltipRectTransform.rect.height * canvas.scaleFactor;

        // 1. Calculate the starting position in screen space (Input.mousePosition is in pixels)
        Vector3 newPos = Input.mousePosition + offset * canvas.scaleFactor;
        newPos.x += width * 0.5f;
        newPos.y -= height * 0.5f;
        newPos.z = 0f;

        // --- Clamping Logic (Adapting the example's approach) ---
        
        // 2. Right Edge Clamping
        // Right edge of tooltip = newPos.x + width / 2 (assuming pivot x=0.5)
        // Since tooltips are often pivoted at (0, 1) or (0, 0), let's calculate based on the current pivot and anchor.
        // If we treat newPos as the pivot point (center of the tooltip):
        float rightEdgeToScreenEdgeDistance = Screen.width - (newPos.x + width * (1f - tooltipRectTransform.pivot.x)) - padding;
        if (rightEdgeToScreenEdgeDistance < 0)
        {
            newPos.x += rightEdgeToScreenEdgeDistance;
        }

        // 3. Left Edge Clamping
        // Left edge of tooltip = newPos.x - width * tooltipRectTransform.pivot.x
        float leftEdgeToScreenEdgeDistance = 0 - (newPos.x - width * tooltipRectTransform.pivot.x) + padding;
        if (leftEdgeToScreenEdgeDistance > 0)
        {
            newPos.x += leftEdgeToScreenEdgeDistance;
        }
        
        // 4. Top Edge Clamping
        // Top edge of tooltip = newPos.y + height * (1f - tooltipRectTransform.pivot.y)
        float topEdgeToScreenEdgeDistance = Screen.height - (newPos.y + height * (1f - tooltipRectTransform.pivot.y)) - padding;
        if (topEdgeToScreenEdgeDistance < 0)
        {
            newPos.y += topEdgeToScreenEdgeDistance;
        }
        
        // 5. Bottom Edge Clamping (Optional, usually less important for mouse-following tooltips)
        // Bottom edge of tooltip = newPos.y - height * tooltipRectTransform.pivot.y
        float bottomEdgeToScreenEdgeDistance = (newPos.y - height * tooltipRectTransform.pivot.y) - padding;
        if (bottomEdgeToScreenEdgeDistance < 0)
        {
            newPos.y -= bottomEdgeToScreenEdgeDistance;
        }

        // 6. Apply the clamped position in screen space
        tooltipRectTransform.transform.position = newPos;
    }
}