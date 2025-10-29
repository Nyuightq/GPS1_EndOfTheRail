// --------------------------------------------------------------
// Creation Date: 2025-10-20 05:54
// Author: User
// Description: -
// --------------------------------------------------------------
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ItemDragManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float moveSpd;

    private Item itemScript;
    private InventoryGridScript inventoryGridScript;

    private RectTransform rectTransform;

    private Vector2 topLeftCellPos;
    private Vector2 dragDir;
    private bool dragging, mouseOnItem;

    #region Unity LifeCycle
    private void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        itemScript = GetComponent<Item>();
        inventoryGridScript = FindFirstObjectByType<InventoryGridScript>().GetComponent<InventoryGridScript>();
    }

    public void Update()
    {
        //FIX TS, ENd drag has false positives, Find if we use any new input systems
        if(mouseOnItem)
        {
            if (Input.GetMouseButtonDown(0)) dragging = true;
            if (Input.GetMouseButtonUp(0)) dragging = false;
        }

        if(dragging)
        {
            itemScript.spriteRectTransform.localScale = Vector3.Lerp(itemScript.spriteRectTransform.localScale, new Vector3(0.9f, 0.9f, 1f), 0.3f);

            if (Input.GetMouseButtonDown(1))
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
        else
        {
            if(itemScript.spriteRectTransform.localScale != new Vector3(1f,1f,1f)) itemScript.spriteRectTransform.localScale = Vector3.Lerp(itemScript.spriteRectTransform.localScale, new Vector3(1f, 1f, 1f), 0.3f);
        }

        Debug.Log(dragging);
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging = true;

        if (topLeftCellPos != null && itemScript.state == Item.itemState.equipped)
        {
            inventoryGridScript.MarkCells(Vector2Int.FloorToInt(topLeftCellPos), itemScript.itemShape, null);
            itemScript.state = Item.itemState.unequipped;
        }
    }

    public void OnEndDrag(PointerEventData eventData) 
    {
        dragging = false;
        AttachToInventory();
    }

    //handles being able to drag stuff
    public void OnDrag(PointerEventData eventData)
    {
        dragDir = eventData.position;
        float moveX = Mathf.Lerp(rectTransform.position.x, dragDir.x, 0.2f);
        float moveY = Mathf.Lerp(rectTransform.position.y, dragDir.y, 0.2f);
        rectTransform.position = new Vector2(moveX, moveY);
    }
    #endregion

    public void AttachToInventory()
    {
        if (itemScript.state == Item.itemState.unequipped)
        {
            foreach (GameObject shapeCell in itemScript.shape)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(inventoryGridScript.inventoryRect, shapeCell.transform.position, null, out Vector2 localPos);

                Vector2 cellPos = inventoryGridScript.GetCellAtPos(localPos);
                if (inventoryGridScript.InGrid(cellPos) && inventoryGridScript.inventoryGrid[(int)cellPos.x, (int)cellPos.y].item == null)
                {
                    //Debug.Log(cellPos);
                }
                else
                {
                    return;
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

            inventoryGridScript.MarkCells(Vector2Int.FloorToInt(topLeftCellPos), itemScript.itemShape, gameObject);

            rectTransform.anchoredPosition = actualItemCellPos;
            itemScript.state = Item.itemState.equipped;
        }
    }
}
