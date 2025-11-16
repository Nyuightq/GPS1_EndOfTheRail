// --------------------------------------------------------------
// Creation Date: 2025-10-20 05:54
// Author: User
// Description: -
// --------------------------------------------------------------
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ItemDragManager : MonoBehaviour, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float moveSpd;

    private Item itemScript;
    private InventoryGridScript inventoryGridScript;

    private RectTransform rectTransform;

    public Vector2 topLeftCellPos { get; private set; }
    private Vector2 dragDir;
    private bool dragging, mouseOnItem;

    //the last equipped position
    private Vector2 equippedPos;
    private bool firstEquip = true;

    InputActionMap playerActionMap;

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
            itemScript.spriteRectTransform.localScale = Vector3.Lerp(itemScript.spriteRectTransform.localScale, new Vector3(0.9f, 0.9f, 1f), 0.3f);
        }
        else
        {
            if(itemScript.spriteRectTransform.localScale != new Vector3(1f,1f,1f)) itemScript.spriteRectTransform.localScale = Vector3.Lerp(itemScript.spriteRectTransform.localScale, new Vector3(1f, 1f, 1f), 0.3f);
        }

        //Debug.Log(dragging);

        switch (itemScript.state)
        {
            case Item.itemState.equipped:
                foreach (GameObject shapeCell in itemScript.shape) shapeCell.SetActive(false);
                break;
            case Item.itemState.unequipped:
                foreach (GameObject shapeCell in itemScript.shape) shapeCell.SetActive(true);
                break;
        }
    }
    #endregion

    #region holding/Dragging handlers
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        mouseOnItem = true;
    }

    public void OnPointerExit (PointerEventData pointerEventData)
    {
        mouseOnItem = false;
    }

    //handles being able to drag stuff
    public void OnDrag(PointerEventData eventData)
    {
        dragDir = eventData.position;
        float moveX = Mathf.Lerp(rectTransform.position.x, dragDir.x, 0.2f);
        float moveY = Mathf.Lerp(rectTransform.position.y, dragDir.y, 0.2f);
        rectTransform.position = new Vector2(moveX, moveY);
    }

    private void LeftClick()
    {
        //Debug.Log(GameStateManager.CurrentPhase);
        if (mouseOnItem && GameStateManager.CurrentPhase != Phase.Combat)
        {
            dragging = true;

            if (topLeftCellPos != null && itemScript.state == Item.itemState.equipped)
            {
                inventoryGridScript.MarkCells(Vector2Int.FloorToInt(topLeftCellPos), itemScript.itemShape, null);
                itemScript.state = Item.itemState.unequipped;
                itemScript.TriggerEffectUnequip();
            }

            if (EngineerManager.Instance != null && EngineerManager.Instance.IsEngineerUIActive)
            {
                EngineerManager.Instance.OnItemDragStarted(gameObject);
            }

            if (EngineerManager.Instance != null && EngineerManager.Instance.IsEngineerUIActive)
            {
                EngineerManager.Instance.OnItemDragStarted(gameObject);
            }
        }
    }

// Only showing the updated LeftRelease() method
// Add this to your existing ItemDragManager.cs

private void LeftRelease()
{
    if (mouseOnItem)
    {
        dragging = false;
        bool sucess = AttachToInventory();

        //equipped pos defaults to 0 and 0 is largely impossible to get to at the start
        if (!sucess && !firstEquip &&  itemScript.itemData.mandatoryItem)
        {
            // ADD THIS LINE - Critical for snap-back functionality
            rectTransform.anchoredPosition = equippedPos;
            AttachToInventory();
        }

        // ADD THIS LINE - Critical for snap-back functionality
        // Notify RewardManager (if active)
        if (RewardManager.Instance != null)
        {
            RewardManager.Instance.OnItemReleased(gameObject);
        }
        
        // Notify TransactionManager (if active)
        if (TransactionManager.Instance != null)
        {
            TransactionManager.Instance.OnItemReleased(gameObject);
        }
    }
}

    private void RightClick()
    {
        if (dragging)
        {
            Debug.Log("Attempting to rotate");

            if (topLeftCellPos != null && itemScript.state == Item.itemState.equipped)
            {
                inventoryGridScript.MarkCells(Vector2Int.FloorToInt(topLeftCellPos), itemScript.itemShape, null);
                itemScript.state = Item.itemState.unequipped;
            }

            itemScript.RotateShape(itemScript.itemShape);
        }
    }
    #endregion

public bool AttachToInventory()
{
    if (itemScript.state == Item.itemState.unequipped)
    {
        foreach (GameObject shapeCell in itemScript.shape)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(inventoryGridScript.inventoryRect, shapeCell.transform.position, null, out Vector2 localPos);

            Vector2 cellPos = inventoryGridScript.GetCellAtPos(localPos);
            InvCellData localGridCellPos = null;
            if (inventoryGridScript.InGrid(cellPos)) localGridCellPos = inventoryGridScript.inventoryGrid[(int)cellPos.x, (int)cellPos.y];

            if (localGridCellPos != null && localGridCellPos.item == null && localGridCellPos.active)
            {
                //Debug.Log(cellPos);
            }
            else
            {
                return false;
            }
        }
        
        Debug.Log("Item Attached!!!");

        int itemWidth = itemScript.itemShape.GetLength(0);
        int itemHeight = itemScript.itemShape.GetLength(1);

        Vector2 screenTopLeft = RectTransformUtility.WorldToScreenPoint(null, rectTransform.TransformPoint(new Vector2(-itemWidth * 16 / 2f, itemHeight * 16 / 2f) + new Vector2(8f, -8f)));

        RectTransformUtility.ScreenPointToLocalPointInRectangle(inventoryGridScript.inventoryRect, screenTopLeft, null, out Vector2 topLeftCell);
        topLeftCellPos = inventoryGridScript.GetCellAtPos(topLeftCell);

        Vector2 itemCellPos = new Vector2(topLeftCellPos.x + itemWidth / 2f - 0.5f, topLeftCellPos.y + itemHeight / 2f - 0.5f);
        Vector2 actualItemCellPos = inventoryGridScript.GetLocalPosGrid(itemCellPos);

        Debug.Log(topLeftCell);
        Debug.Log(topLeftCellPos);

        // ADD THIS: Change parent to inventory BEFORE marking cells
        if (rectTransform.parent != inventoryGridScript.inventoryRect)
        {
            // Store world position
            Vector3 worldPos = rectTransform.position;
            
            // Change parent
            rectTransform.SetParent(inventoryGridScript.inventoryRect);
            
            // Restore world position temporarily
            rectTransform.position = worldPos;
        }

        inventoryGridScript.MarkCells(Vector2Int.FloorToInt(topLeftCellPos), itemScript.itemShape, gameObject);

        rectTransform.anchoredPosition = actualItemCellPos;
        equippedPos = actualItemCellPos;
        itemScript.state = Item.itemState.equipped;
        itemScript.TriggerEffectEquip();

        foreach(GameObject thingy in inventoryGridScript.GetAdjacentComponents(Vector2Int.FloorToInt(topLeftCellPos), itemScript.itemShape,this.gameObject))
            {
                Debug.Log($"<color=red>{gameObject} near {thingy}</color>");
            }    


            firstEquip = false;
            return true;
        }
        return false;
    }
}

