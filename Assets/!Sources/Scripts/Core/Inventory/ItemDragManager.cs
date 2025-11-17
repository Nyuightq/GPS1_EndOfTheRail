// --------------------------------------------------------------
// Creation Date: 2025-10-20 05:54
// Author: User
// Description: Cleaned up with proper encapsulation + Tooltip support + Adjacent triggers
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ItemDragManager : MonoBehaviour, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Item itemScript;
    private InventoryGridScript inventoryGridScript;
    private RectTransform rectTransform;
    private ItemTooltip tooltip;

    // Public read-only properties for external access
    public Vector2 TopLeftCellPos => topLeftCellPos;
    public Vector2 EquippedPos => equippedPos;
    public bool IsHovered => mouseOnItem;

    // Private backing fields
    private Vector2 topLeftCellPos;
    private Vector2 equippedPos;
    
    private Vector2 dragDir;
    private bool dragging;
    private bool mouseOnItem;
    private bool firstEquip = true; // Kept from Ysaac's version

    #region Unity LifeCycle
    private void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        itemScript = GetComponent<Item>();
        inventoryGridScript = FindFirstObjectByType<InventoryGridScript>().GetComponent<InventoryGridScript>();

        InputManager.OnLeftClick += LeftClick;
        InputManager.OnLeftRelease += LeftRelease;
        InputManager.OnRightClick += RightClick;
    }

    private void Start()
    {
        // Get tooltip instance after it's initialized
        tooltip = ItemTooltip.Instance;
    }

    private void OnDisable()
    {
        InputManager.OnLeftClick -= LeftClick;
        InputManager.OnLeftRelease -= LeftRelease;
        InputManager.OnRightClick -= RightClick;
    }

    public void Update()
    {
        if(dragging)
        {
            itemScript.spriteRectTransform.localScale = Vector3.Lerp(
                itemScript.spriteRectTransform.localScale, 
                new Vector3(0.9f, 0.9f, 1f), 
                0.3f
            );
        }
        else
        {
            if(itemScript.spriteRectTransform.localScale != Vector3.one)
            {
                itemScript.spriteRectTransform.localScale = Vector3.Lerp(
                    itemScript.spriteRectTransform.localScale, 
                    Vector3.one, 
                    0.3f
                );
            }
        }

        // Toggle shape visibility based on item state
        switch (itemScript.state)
        {
            case Item.itemState.equipped:
                foreach (GameObject shapeCell in itemScript.shape) 
                    shapeCell.SetActive(false);
                break;
            case Item.itemState.unequipped:
                foreach (GameObject shapeCell in itemScript.shape) 
                    shapeCell.SetActive(true);
                break;
        }
    }
    #endregion

    #region Pointer & Drag Handlers
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        mouseOnItem = true;
        
        // Show tooltip when hovering
        if (tooltip != null)
        {
            tooltip.Show(itemScript);
        }
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        mouseOnItem = false;
        
        // Hide tooltip when not hovering
        if (tooltip != null)
        {
            tooltip.Hide();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        dragDir = eventData.position;
        float moveX = Mathf.Lerp(rectTransform.position.x, dragDir.x, 0.2f);
        float moveY = Mathf.Lerp(rectTransform.position.y, dragDir.y, 0.2f);
        rectTransform.position = new Vector2(moveX, moveY);
    }
    #endregion

    #region Input Handlers
    private void LeftClick()
    {
        if (mouseOnItem && GameStateManager.CurrentPhase != Phase.Combat)
        {
            dragging = true;

            // Hide tooltip when dragging starts
            if (tooltip != null)
            {
                tooltip.Hide();
            }

            // Unequip from inventory if currently equipped
            if (itemScript.state == Item.itemState.equipped)
            {
                inventoryGridScript.MarkCells(
                    Vector2Int.FloorToInt(topLeftCellPos), 
                    itemScript.itemShape, 
                    null
                );
                itemScript.state = Item.itemState.unequipped;
                itemScript.TriggerEffectUnequip();
            }

            // Notify EngineerManager if active
            if (EngineerManager.Instance != null && EngineerManager.Instance.IsEngineerUIActive)
            {
                EngineerManager.Instance.OnItemDragStarted(gameObject);
            }
        }
    }

    private void LeftRelease()
    {
        if (!mouseOnItem) return;

        dragging = false;

        // Show tooltip again after releasing if still hovering
        if (mouseOnItem && tooltip != null)
        {
            tooltip.Show(itemScript);
        }

        // Check Engineer first (higher priority when active)
        if (EngineerManager.Instance != null && EngineerManager.Instance.IsEngineerUIActive)
        {
            EngineerManager.Instance.OnItemReleased(gameObject);
            
            // Remove temporary canvas components if they were added
            RemoveTemporaryCanvas();
        }
        else
        {
            // Normal inventory attachment
            AttachToInventory();

            // Notify other managers (if active)
            RewardManager.Instance?.OnItemReleased(gameObject);
            TransactionManager.Instance?.OnItemReleased(gameObject);
        }
    }

    private void RightClick()
    {
        if (!dragging) return;

        Debug.Log("Attempting to rotate");

        // Unequip before rotating
        if (itemScript.state == Item.itemState.equipped)
        {
            inventoryGridScript.MarkCells(
                Vector2Int.FloorToInt(topLeftCellPos), 
                itemScript.itemShape, 
                null
            );
            itemScript.state = Item.itemState.unequipped;
        }

        itemScript.RotateShape(itemScript.itemShape);
    }
    #endregion

    #region Inventory Attachment
    public bool AttachToInventory()
    {
        if (itemScript.state != Item.itemState.unequipped)
            return false;

        // Validate all shape cells can be placed
        foreach (GameObject shapeCell in itemScript.shape)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                inventoryGridScript.inventoryRect, 
                shapeCell.transform.position, 
                null, 
                out Vector2 localPos
            );

            Vector2 cellPos = inventoryGridScript.GetCellAtPos(localPos);
            
            if (!inventoryGridScript.InGrid(cellPos))
                return false;

            InvCellData gridCell = inventoryGridScript.inventoryGrid[(int)cellPos.x, (int)cellPos.y];
            
            if (gridCell == null || !gridCell.active || gridCell.item != null)
                return false;
        }

        Debug.Log("Item Attached!!!");

        int itemWidth = itemScript.itemShape.GetLength(0);
        int itemHeight = itemScript.itemShape.GetLength(1);

        // Calculate top-left cell position
        Vector2 screenTopLeft = RectTransformUtility.WorldToScreenPoint(
            null, 
            rectTransform.TransformPoint(
                new Vector2(-itemWidth * 16 / 2f, itemHeight * 16 / 2f) + new Vector2(8f, -8f)
            )
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            inventoryGridScript.inventoryRect, 
            screenTopLeft, 
            null, 
            out Vector2 topLeftCell
        );
        
        topLeftCellPos = inventoryGridScript.GetCellAtPos(topLeftCell);

        // Calculate center position for the item
        Vector2 itemCellPos = new Vector2(
            topLeftCellPos.x + itemWidth / 2f - 0.5f, 
            topLeftCellPos.y + itemHeight / 2f - 0.5f
        );
        Vector2 actualItemCellPos = inventoryGridScript.GetLocalPosGrid(itemCellPos);

        Debug.Log($"TopLeftCell: {topLeftCell}, TopLeftCellPos: {topLeftCellPos}");

        // Ensure item is parented to inventory before marking cells
        if (rectTransform.parent != inventoryGridScript.inventoryRect)
        {
            Vector3 worldPos = rectTransform.position;
            rectTransform.SetParent(inventoryGridScript.inventoryRect);
            rectTransform.position = worldPos; // Maintain visual position temporarily
        }

        // Mark cells and set final position
        inventoryGridScript.MarkCells(
            Vector2Int.FloorToInt(topLeftCellPos), 
            itemScript.itemShape, 
            gameObject
        );

        rectTransform.anchoredPosition = actualItemCellPos;
        equippedPos = actualItemCellPos; // Store the equipped position
        
        itemScript.state = Item.itemState.equipped;
        itemScript.TriggerEffectEquip();

        // Trigger adjacent item effects (Ysaac's feature)
        foreach(GameObject adjacentItem in inventoryGridScript.GetAdjacentComponents(
            Vector2Int.FloorToInt(topLeftCellPos), 
            itemScript.itemShape, 
            gameObject))
        {
            Debug.Log($"<color=green>{gameObject.name} near {adjacentItem.name}</color>");
            
            Item adjacentItemScript = adjacentItem.GetComponent<Item>();
            if (adjacentItemScript != null)
            {
                adjacentItemScript.TriggerEffectAdjacentEquip();
            }
        }

        firstEquip = false;
        return true;
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Removes the temporary canvas components added during engineer UI dragging
    /// </summary>
    private void RemoveTemporaryCanvas()
    {
        Canvas itemCanvas = GetComponent<Canvas>();
        if (itemCanvas != null && itemCanvas.overrideSorting && itemCanvas.sortingOrder == 1000)
        {
            // Remove GraphicRaycaster first
            UnityEngine.UI.GraphicRaycaster raycaster = GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (raycaster != null)
            {
                Destroy(raycaster);
            }
            
            // Then remove Canvas
            Destroy(itemCanvas);
        }
    }
    #endregion
}