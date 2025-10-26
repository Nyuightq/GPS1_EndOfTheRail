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
public class ItemDragManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private float moveSpd;

    private Item itemScript;
    private InventoryGridScript inventoryGridScript;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    Vector2 itemCellPos;
    Vector2 topLeftCellPos;

    private Vector2 dragDir;

    private bool dragging;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        itemScript = GetComponent<Item>();
        
    }

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        inventoryGridScript = FindFirstObjectByType<InventoryGridScript>().GetComponent<InventoryGridScript>();
    }

    public void Update()
    {
        if(dragging)
        {
            if (Input.GetMouseButtonDown(1))
            {
                Debug.Log("Attempting to rotate");
                itemScript.rotateShape(itemScript.itemShape);

            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging = true;
        //Debug.Log("begin drag" + gameObject);
        if (topLeftCellPos != null && itemScript.state == Item.itemState.equipped)
        {
            inventoryGridScript.markCells(Vector2Int.FloorToInt(topLeftCellPos), itemScript.itemShape, null);
            itemScript.state = Item.itemState.unequipped;
        }
    }

    public void OnEndDrag(PointerEventData eventData) 
    {
        dragging = false;

        if(itemScript.state == Item.itemState.unequipped)
        {
            foreach(GameObject shapeCell in itemScript.shape)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(inventoryGridScript.inventoryRect, shapeCell.transform.position, null, out Vector2 localPos);

                Vector2 cellPos = inventoryGridScript.getCellAtPos(localPos);
                if(inventoryGridScript.inGrid(cellPos) && inventoryGridScript.inventoryGrid[(int)cellPos.x,(int)cellPos.y].item == null)
                {
                    //Debug.Log("OOOGA BOOGA");
                    Debug.Log(cellPos);
                }
                else
                {
                    return;
                }
        
            }
            Debug.Log("Item Attached!!!");

            int itemWidth = itemScript.itemShape.GetLength(0);
            int itemHeight = itemScript.itemShape.GetLength(1);

            //look into this
            Vector2 screenTopLeft = RectTransformUtility.WorldToScreenPoint(
            null,
            rectTransform.TransformPoint(new Vector2(-itemWidth * 16 / 2f, itemHeight * 16 / 2f) + new Vector2(8f,-8f))
            );


            RectTransformUtility.ScreenPointToLocalPointInRectangle(inventoryGridScript.inventoryRect, screenTopLeft, null, out Vector2 topLeftCell);
            topLeftCellPos = inventoryGridScript.getCellAtPos(topLeftCell/*+new Vector2(0,16)*/ /*new Vector2(-8,8)*/);
            Debug.Log(topLeftCell);
            Debug.Log(topLeftCellPos);

            inventoryGridScript.markCells(Vector2Int.FloorToInt(topLeftCellPos), itemScript.itemShape, gameObject);


            Vector2 itemCellPos = new Vector2(topLeftCellPos.x + itemWidth / 2f - 0.5f, topLeftCellPos.y + itemHeight / 2f - 0.5f);

            Vector2 actualItemCellPos = inventoryGridScript.getLocalPosGrid(itemCellPos);


            
            rectTransform.anchoredPosition = actualItemCellPos;

            itemScript.state = Item.itemState.equipped;

            
        }
        //Debug.Log("end drag" + gameObject);
    }

    //hanldes being able to drag stuff
    public void OnDrag(PointerEventData eventData)
    {
        //gameObject.transform.position = eventData.position;
        dragDir = eventData.position;
        float moveX = Mathf.Lerp(rectTransform.position.x, dragDir.x, 0.2f);
        float moveY = Mathf.Lerp(rectTransform.position.y, dragDir.y, 0.2f);
        rectTransform.position = new Vector2(moveX, moveY);
        //Debug.Log("dragging " + gameObject + " to " + eventData);
    }

    //private ItemShapeCell[,] RotateShape(ItemShapeCell[,] itemShape)
    //{

    //}

}
