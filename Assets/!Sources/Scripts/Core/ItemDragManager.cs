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
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //Debug.Log("begin drag" + gameObject);
        if(topLeftCellPos != null)inventoryGridScript.markCells(Vector2Int.FloorToInt(topLeftCellPos), itemScript.itemShape, null);
    }

    public void OnEndDrag(PointerEventData eventData) 
    {
        if(itemScript.state == Item.itemState.unequipped)
        {
            foreach(GameObject shapeCell in itemScript.shape)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(inventoryGridScript.inventoryRect, shapeCell.transform.position, null, out Vector2 localPos);

                Vector2 cellPos = inventoryGridScript.getCellAtPos(localPos);
                if(inventoryGridScript.inGrid(cellPos))
                {
                    Debug.Log("OOOGA BOOGA");
                }
                else
                {
                    return;
                }
        
            }
            Debug.Log("Item Attached!!!");
            RectTransformUtility.ScreenPointToLocalPointInRectangle(inventoryGridScript.inventoryRect, transform.position, null, out Vector2 itemCentre);
            Vector2 itemCellPos = inventoryGridScript.getCellAtPos(itemCentre);

            Vector2 actualItemCellPos = inventoryGridScript.getLocalPosGrid(itemCellPos);
            rectTransform.anchoredPosition = actualItemCellPos;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(inventoryGridScript.inventoryRect, itemScript.shape[0].transform.position, null, out Vector2 topLeftCell);
            topLeftCellPos = inventoryGridScript.getCellAtPos(topLeftCell + new Vector2(-8,16));

            inventoryGridScript.markCells(Vector2Int.FloorToInt(topLeftCellPos), itemScript.itemShape, gameObject);
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
}
