// --------------------------------------------------------------
// Creation Date: 2025-10-20 05:54
// Author: User
// Description: Cleaned up with proper encapsulation - Refactored for Singleton Tooltip
// --------------------------------------------------------------
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ItemDragManager : MonoBehaviour, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Item itemScript;
    private InventoryGridScript inventoryGridScript;
    private RectTransform rectTransform;
    private ItemTooltip tooltip;

    public Vector2 topLeftCellPos { get; private set; }
    private Vector2 dragDir;
    public bool canDrag;
    public bool dragging { get; private set; }
    private bool mouseOnItem;
    public Vector2 TopLeftCellPos => topLeftCellPos;
    public Vector2 EquippedPos => equippedPos;
    public bool IsHovered => mouseOnItem;

    //the last equipped position
    private Vector2 equippedPos;
    private bool firstEquip = true;

    #region Unity LifeCycle
    private void Start()
    {
        // Get tooltip instance after it's initialized
        tooltip = ItemTooltip.Instance;
    }

    private void OnEnable()
    {

        rectTransform = GetComponent<RectTransform>();
        itemScript = GetComponent<Item>();
        inventoryGridScript = FindFirstObjectByType<InventoryGridScript>().GetComponent<InventoryGridScript>();

        InputManager.OnLeftClick += LeftClick;
        InputManager.OnLeftRelease += LeftRelease;
        InputManager.OnRightClick += RightClick;
    }

    private void OnDisable()
    {
        InputManager.OnLeftClick -= LeftClick;
        InputManager.OnLeftRelease -= LeftRelease;
        InputManager.OnRightClick -= RightClick;
    }

    public void OnDestroy()
    {
        tooltip.Hide();
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
                itemScript.ShowPreview(true);
                break;
            case Item.itemState.unequipped:
                if (!mouseOnItem || !dragging) itemScript.ShowPreview(false); else itemScript.ShowPreview(true);
                break;
        }
    }
    #endregion

    #region Pointer & Drag handlers
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        mouseOnItem = true;

        if (tooltip != null && !dragging)
        {
            tooltip.Show(itemScript);
            SoundManager.Instance.PlaySFX("SFX_Component_OnHover");
        }
    }

    public void OnPointerExit (PointerEventData pointerEventData)
    {
        mouseOnItem = false;

        if (tooltip != null)
        {
            tooltip.Hide();
        }
    }

    //handles being able to drag stuff
    public void OnDrag(PointerEventData eventData)
    {
        if (dragging)
        {
            dragDir = eventData.position;
            float moveX = Mathf.Lerp(rectTransform.position.x, dragDir.x, 0.2f);
            float moveY = Mathf.Lerp(rectTransform.position.y, dragDir.y, 0.2f);
            rectTransform.position = new Vector2(moveX, moveY);

            // Hide tooltip when dragging starts
            if (tooltip != null)
            {
                tooltip.Hide();
            }
        }
    }

    private void LeftClick()
    {
        if (mouseOnItem && GameStateManager.CurrentPhase != Phase.Combat && !itemScript.flaggedForDeletion)
        {
            canDrag = true;
            SoundManager.Instance.PlaySFX("SFX_Component_OnDrag");
        }
        else
        {
            canDrag = false;
        }

        if (canDrag)
        {
            dragging = true;

            if (topLeftCellPos != null && itemScript.state == Item.itemState.equipped)
            {
                inventoryGridScript.MarkCells(
                    Vector2Int.FloorToInt(topLeftCellPos),
                    itemScript.itemShape,
                    null
                );
                itemScript.state = Item.itemState.unequipped;
                itemScript.TriggerEffectUnequip();
            }

            if (EngineerManager.Instance != null && EngineerManager.Instance.IsEngineerUIActive)
            {
                EngineerManager.Instance.OnItemDragStarted(gameObject);
            }
        }
    }
    //

    // Only showing the updated LeftRelease() method
    // Add this to your existing ItemDragManager.cs

    private void LeftRelease()
    {
        if (!dragging) return;
        
        dragging = false;

        // if (tooltip != null)
        // {
        //     tooltip.Show(itemScript);
        // }

        bool sucess = AttachToInventory();
        if ( sucess )
        {
            SoundManager.Instance.PlaySFX("SFX_Component_OnRegistered");
        }
        else
        {
            SoundManager.Instance.PlaySFX("SFX_Component_OnRelease");
        }
        //equipped pos defaults to 0 and 0 is largely impossible to get to at the start
        if (!sucess && !firstEquip &&  itemScript.itemData.mandatoryItem)
        {
            // ADD THIS LINE - Critical for snap-back functionality
            rectTransform.anchoredPosition = equippedPos;
            AttachToInventory();
        }

        // Check Engineer first (higher priority when active)
        if (EngineerManager.Instance != null && EngineerManager.Instance.IsEngineerUIActive)
        {
            EngineerManager.Instance.OnItemReleased(gameObject);
            RemoveTemporaryCanvas();
        }
        else
        {
            AttachToInventory();

            RewardManager.Instance?.OnItemReleased(gameObject);
            TransactionManager.Instance?.OnItemReleased(gameObject);
        }
    }

    private void RightClick()
    {
        if (!dragging) return;
        
        if (itemScript.state == Item.itemState.equipped)
        {
            Debug.Log("Attempting to rotate");

            if (topLeftCellPos != null && itemScript.state == Item.itemState.equipped)
            {
                inventoryGridScript.MarkCells(Vector2Int.FloorToInt(topLeftCellPos), itemScript.itemShape, null);
                itemScript.state = Item.itemState.unequipped;
            }

            itemScript.RotateShape(itemScript.itemShape);
            itemScript.TriggerEffectAdjacentEquip();
            foreach (GameObject adjacent in inventoryGridScript.GetAdjacentComponents(Vector2Int.FloorToInt(topLeftCellPos), itemScript.itemShape, gameObject))
            {
                adjacent.GetComponent<Item>()?.TriggerEffectAdjacentEquip();
            }
        }

        itemScript.RotateShape(itemScript.itemShape);
        SoundManager.Instance.PlaySFX("SFX_Component_OnRotate");
    }
    #endregion

    #region Inventory Attachment
    public bool AttachToInventory()
    {
        if (itemScript.state == Item.itemState.unequipped)
        {
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
                {
                    return false;
                }

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

            Vector2 itemCellPos = new Vector2(
                topLeftCellPos.x + itemWidth / 2f - 0.5f, 
                topLeftCellPos.y + itemHeight / 2f - 0.5f
            );
            Vector2 actualItemCellPos = inventoryGridScript.GetLocalPosGrid(itemCellPos);

            // Ensure item is parented to inventory before marking cells
            if (rectTransform.parent != inventoryGridScript.inventoryRect)
            {
                Vector3 worldPos = rectTransform.position;
            
                rectTransform.SetParent(inventoryGridScript.inventoryRect);
                rectTransform.position = worldPos; // Maintain visual position temporarily
            }

            inventoryGridScript.MarkCells(
                Vector2Int.FloorToInt(topLeftCellPos), 
                itemScript.itemShape, 
                gameObject
            );

            rectTransform.anchoredPosition = actualItemCellPos;
            equippedPos = actualItemCellPos; // Store the equipped position
            itemScript.state = Item.itemState.equipped;
            itemScript.TriggerEffectEquip();
            itemScript.TriggerEffectAdjacentEquip();

            foreach (GameObject adjacentItems in inventoryGridScript.GetAdjacentComponents(Vector2Int.FloorToInt(topLeftCellPos), itemScript.itemShape, gameObject))
            {
                Debug.Log($"<color=green>{gameObject} near {adjacentItems}</color>");
                adjacentItems.GetComponent<Item>().TriggerEffectAdjacentEquip();
            }

            firstEquip = false;
            return true;
        }
        return false;
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

