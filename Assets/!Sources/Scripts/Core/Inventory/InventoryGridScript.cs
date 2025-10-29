// --------------------------------------------------------------
// Creation Date: 2025-10-14 02:45
// Author: User
// Description: -
// --------------------------------------------------------------
using NUnit.Framework.Interfaces;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class InvCellData
{
    //public Vector2 arrayPos;
    public GameObject cellObject;
    public GameObject item;

    public InvCellData(GameObject cellObject)
    {
        this.cellObject = cellObject;
    }
};

public class InventoryGridScript : MonoBehaviour
{
    [Header("Inventory Dimensions/Configs")]
    [SerializeField] public int inventoryWidth;
    [SerializeField] public int inventoryHeight;

    [SerializeField] private GameObject inventoryCell;
    [SerializeField] private Canvas inventoryCanvas;
    public RectTransform inventoryRect { get; private set; } 
    [SerializeField] public float cellSize = 16f;
    [SerializeField] private Vector2 margin;

    [SerializeField] private float defaultScale = 0.9f;

    [Header("Item Spawning Configs")]
    [SerializeField] private bool spawnItemInInventory;
    [SerializeField] private Vector2 spawnCentreOffset = Vector2.zero;
    [SerializeField] private int spawnMargin = 5;
    [SerializeField] private GameObject itemSpawn;
    [SerializeField] private List<ItemSO> startingItems;

    //public Dictionary<GameObject, InvCellData> equippedItems = new Dictionary<GameObject, InvCellData>();

    public List<GameObject> equippedItems = new List<GameObject>();

    private float canvasWidth;
    private float canvasHeight;

    private Vector2 mouse;


    public InvCellData[,] inventoryGrid;

    void Awake()
    {
        inventoryRect = inventoryCanvas.GetComponent<RectTransform>();
        inventoryGrid = new InvCellData[inventoryWidth, inventoryHeight];
        
    }

    private IEnumerator Start()
    {
        canvasWidth = inventoryRect.rect.width;
        canvasHeight = inventoryRect.rect.height;

        generateGrid();

        if (spawnItemInInventory)
        {
            foreach (ItemSO startingItem in startingItems)
            {
                addItem(itemSpawn, startingItem);
                yield return null;
            }
        }
        else
        {
            spawnItems(itemSpawn, startingItems, spawnCentreOffset, spawnMargin);
        }
    }



    private void Update()
    {
        Vector2 mouseBottomLeft = getMousePosGrid();

        //Debug.Log($"Local mouse: {mouse}, success={success}");

        if (inventoryGrid != null)
        {
            Vector2Int cellPos = getCellAtPos(mouseBottomLeft);
            EnlargeOnHover(cellPos);

            //debug shit remove later
            foreach(InvCellData cell in inventoryGrid)
            {
                if(cell.item != null)
                {
                    RectTransform cellRect = cell.cellObject.GetComponent<RectTransform>();
                    cell.cellObject.transform.localScale = Vector3.Lerp(cellRect.localScale, new Vector3(1.1f, 1.1f, 1f), 0.3f);
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            Instantiate(itemSpawn,inventoryRect);
        }
    }

    private void generateGrid()
    {
        for (int x = 0; x < inventoryWidth; x++)
        {
            for (int y = 0; y < inventoryHeight; y++)
            {
                GameObject newCell = Instantiate(inventoryCell, inventoryRect);
                RectTransform cellRect = newCell.GetComponent<RectTransform>();

                InvCellData data = new InvCellData(newCell);
                inventoryGrid[x, y] = data;

                newCell.GetComponent<DebugInventorySlot>().setInfo((int)x, (int)y);

                // Anchor to top-right corner of canvas
                cellRect.anchorMin = new Vector2(0.5f, 0.5f);
                cellRect.anchorMax = new Vector2(0.5f, 0.5f);
                cellRect.pivot = new Vector2(0.5f, 0.5f);

                // Position each cell from left to right, up to bottom using the top right of the screen as a pivot
                //Vector2 gridSize = new Vector2(inventoryWidth * cellSize, inventoryHeight * cellSize);

                //Vector2 canvasTopRight = new Vector2(canvasWidth*0.5f - gridSize.x - margin.x, canvasHeight*0.5f - cellSize - margin.y);

                Vector2 canvasTopLeft = getTopLeft();

                Vector2 cellPos = canvasTopLeft + new Vector2(x * cellSize + cellSize/2, -y * cellSize + cellSize/2);
                cellRect.anchoredPosition = cellPos;

                cellRect.SetAsFirstSibling();
            }
        }
    }

    

    //returns the actual position in local space that the cell position takes up
    public Vector2 getLocalPosGrid(Vector2 cellPos)
    {
        //Vector2 gridSize = new Vector2(inventoryWidth * cellSize, inventoryHeight * cellSize);
        Vector2 canvasTopLeft = getTopLeft();

        float posX = canvasTopLeft.x + (cellPos.x * cellSize) + (cellSize * 0.5f);
        float posY = canvasTopLeft.y - (cellPos.y * cellSize) + (cellSize * 0.5f);
        return new Vector2(posX, posY);
    }

    public Vector2Int getCellAtPos(Vector2 pos)
    {
        Vector2 canvasTopLeft = getTopLeft();

        Vector2 mouseGrid = new Vector2(pos.x - canvasTopLeft.x, canvasTopLeft.y - pos.y + cellSize);
        int cellX = Mathf.FloorToInt(mouseGrid.x / cellSize);
        int cellY = Mathf.FloorToInt(mouseGrid.y / cellSize);

        return new Vector2Int(cellX, cellY);
    }

    private Vector2 getTopLeft()
    {
        Vector2 gridSize = new Vector2(inventoryWidth * cellSize, inventoryHeight * cellSize);
        return new Vector2(canvasWidth * 0.5f - gridSize.x - margin.x, canvasHeight * 0.5f - cellSize - margin.y);
    }

    public bool inGrid(Vector2 cellPos)
    {
        return (cellPos.x >= 0 && cellPos.x < inventoryWidth && cellPos.y >= 0 && cellPos.y < inventoryHeight);
    }

    public Vector2 getMousePosGrid()
    {
        Vector2 mousePos;
        //converting mouse pos from screenspace to local canvas space
        bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            inventoryRect,              // your canvas rect
            Mouse.current.position.ReadValue(), // screen mouse poss
            null,                       // no camera in overlay mode
            out mousePos
        );

        //applying offset so mouse is positioned based off of bottm left (0,0) instead of canvas pivot (0.5,0.5)
        return mousePos;// + new Vector2(canvasWidth / 2, canvasHeight / 2);
    }

    public void markCells(Vector2Int startingCell, ItemShapeCell[,] itemShape, GameObject itemId)
    {
        for (int x = 0; x < itemShape.GetLength(0); x++)
        {
            for (int y = 0; y < itemShape.GetLength(1); y++)
            {
                if (itemShape[x,y].filled)
                {
                    inventoryGrid[startingCell.x+x,startingCell.y+y].item = itemId;
                }
            }
        }
        checkItems();
    }

    //Debug Function for fun~
    private void EnlargeOnHover(Vector2Int cellPos)
    {
        GameObject selectedCell;
        if (cellPos.x >= 0 && cellPos.x < inventoryWidth && cellPos.y >= 0 && cellPos.y < inventoryHeight)
        {
            //Debug.Log($"on cell:{cellPos.x},{cellPos.y},{inventoryGrid[cellPos.x,cellPos.y].item}");
            selectedCell = inventoryGrid[cellPos.x, cellPos.y].cellObject;
            RectTransform cellRect = selectedCell.GetComponent<RectTransform>();
            cellRect.localScale = Vector3.Lerp(cellRect.localScale, new Vector3(1.2f, 1.2f, 1f), 0.3f);
            resetCellScale(selectedCell);

        }
        else
        {
            selectedCell = null;
            resetCellScale(selectedCell);
        }
    }

    private void resetCellScale(GameObject selectedCell)
    {
        foreach (InvCellData cellData in inventoryGrid)
        {
            if (cellData.cellObject != selectedCell && cellData.item == null )
            {
                RectTransform cellRect = cellData.cellObject.GetComponent<RectTransform>();
                while (cellRect != null && cellRect.localScale != new Vector3(defaultScale, defaultScale, 1f))
                {
                    cellRect.localScale = Vector3.Lerp(cellRect.localScale, new Vector3(defaultScale, defaultScale, 1f), 0.3f);
                }
            }
        }
    }

    public void checkItems()
    {
        Debug.Log("equipped Items :");
        equippedItems.Clear();
        foreach(InvCellData cellData in inventoryGrid)
        {
            if (cellData.item != null && !equippedItems.Contains(cellData.item))
            {
                equippedItems.Add(cellData.item);
            }
        }

        //debug
        foreach(GameObject huhu in equippedItems)
        {
            Debug.Log("item: "+huhu);
        }
    }

    public void addItem(GameObject item, ItemSO itemData)
    {
        Vector2 canvasTopLeft = getTopLeft();
        
        int itemWidth = itemData.itemWidth;
        int itemHeight = itemData.itemHeight;


        ItemShapeCell[,] itemShape = itemData.getShapeGrid();

        Debug.Log("starting placement search");
        for (int x = 0; x<inventoryWidth; x++)
        {
            for (int y = 0; y < inventoryHeight; y++) 
            {
                bool canPlace = true;
                for (int cellX = 0; cellX < itemWidth; cellX++) 
                {
                    for (int cellY = 0; cellY < itemHeight; cellY++)
                    {
                        int checkX = x + cellX;
                        int checkY = y + cellY;

                        if (checkX >= inventoryWidth || checkY >= inventoryHeight)
                        {
                            canPlace = false;
                            break;
                        }

                        if (itemShape[cellX,cellY].filled && inventoryGrid[cellX + x, cellY + y].item != null)
                        {
                            canPlace = false;
                            break;
                        }
                    }
                    if (!canPlace) break;
                }

                if (canPlace) 
                {
                    Debug.Log("valid spot found!");
                    Vector2 origin = getLocalPosGrid(new Vector2(x,y));
                    Vector2 itemCentre = origin + new Vector2(itemWidth * 8f - 8f, -itemHeight * 8f + 8f);

                    GameObject newItem = Instantiate(item, inventoryRect);
                    Item itemScript = newItem.GetComponent<Item>();
                    itemScript.itemData = itemData;

                    //itemScript.refreshShape(itemData.getShapeGrid());

                    RectTransform itemRect = newItem.GetComponent<RectTransform>();
                    itemRect.anchoredPosition = itemCentre;
                    ItemDragManager dragScript = newItem.GetComponent<ItemDragManager>();

                    StartCoroutine(attachNextFrame(dragScript));
                    //dragScript.attachToInventory();
                    Debug.Log("placed!");
                    return;
                }
            }
        }
    }

    private IEnumerator attachNextFrame(ItemDragManager drag)
    {
        yield return null; // wait 1 frame
        drag.attachToInventory();
    }

    private void spawnItems(GameObject itemPrefab, List<ItemSO> itemList, Vector2 pos, float spawnMarginX)
    {
        float offsetX = pos.x;
        float totalWidth = spawnMarginX * (itemList.Count - 1);
        foreach (ItemSO startingItem in itemList) totalWidth += startingItem.itemWidth * cellSize;

        for (int i = 0; i < itemList.Count; i++)
        {
            GameObject newItem = Instantiate(itemPrefab, inventoryRect);
            Item itemScript = newItem.GetComponent<Item>();
            itemScript.itemData = itemList[i];

            RectTransform newItemRect = newItem.GetComponent<RectTransform>();

            newItemRect.anchoredPosition = new Vector2(offsetX - totalWidth * 0.5f, pos.y);
            offsetX += itemList[i].itemWidth * cellSize + spawnMarginX;
        }
    }

}