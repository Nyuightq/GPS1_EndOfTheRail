// --------------------------------------------------------------
// Creation Date: 2025-10-14 02:45
// Author: User
// Description: -
// --------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InvCellData
{
    public bool active;
    public GameObject cellObject;
    public GameObject item;

    public InvCellData(GameObject cellObject, bool active)
    {
        this.cellObject = cellObject;
        this.active = active;
    }
};

public class InventoryGridScript : MonoBehaviour
{
    public enum InventoryState
    {
        normal,
        adding,
        locked
    }

    [Header("Inventory Dimensions/Configs")]
    [SerializeField] public int inventoryWidth;
    [SerializeField] public int inventoryHeight;
    [SerializeField] private int inventoryWidthMax;
    [SerializeField] private int inventoryHeightMax;

    [SerializeField] private GameObject inventoryCell;
    [SerializeField] private Canvas inventoryCanvas;
    [SerializeField] private Vector2 margin;
    [SerializeField] private float defaultScale = 0.9f;

    [Header("Inventory Expansion stuff")]
    [SerializeField] private int baseCost;
    [SerializeField] private float costMultiplier;
    [SerializeField] private TextMeshProUGUI costText;

    [Header("Item Spawning Configs")]
    [SerializeField] private bool spawnItemInInventory;
    [SerializeField] private Vector2 spawnCentreOffset = Vector2.zero;
    [SerializeField] private int spawnMargin = 5;
    [SerializeField] private GameObject itemSpawn;
    [SerializeField] private List<ItemSO> startingItems;

    [Header("Do Not Change")]
    [SerializeField] private InventoryState currentInventoryState;
    [SerializeField] private int currentCost;

    public RectTransform inventoryRect { get; private set; }
    public InvCellData[,] inventoryGrid;

    private List<GameObject> previewCells = new List<GameObject>();

    [SerializeField] public List<GameObject> equippedItems = new List<GameObject>();

    private float cellSize = GameManager.cellSize;
    private float canvasWidth;
    private float canvasHeight;
    private OnPausePlay pauseState;

    
    


    public InventoryState CurrentInventoryState
    {
        get
        {
           return currentInventoryState;
        }
        set
        {
            currentInventoryState = value;

            if(currentInventoryState == InventoryState.adding)
            {
                if (inventoryGrid != null)
                {
                    GenerateGrid();
                    GenerateUpgradePreview();
                }
            }
            else
            {
                ClearUpgradePreview();
            }
        }
    }

    public void OnToggleInventoryState(bool close = false)
    {
        if (close)
        {
            CurrentInventoryState = InventoryState.normal;
            return;
        }

        if (GameStateManager.CurrentPhase != Phase.Plan) return;
        if(CurrentInventoryState != InventoryState.adding) 
            CurrentInventoryState = InventoryState.adding; 
        else 
            CurrentInventoryState = InventoryState.normal;
    }

    #region Unity Lifecycle
    void Awake()
    {
        inventoryRect = inventoryCanvas.GetComponent<RectTransform>();
        inventoryGrid = new InvCellData[inventoryWidth, inventoryHeight];
    }

    private void OnEnable()
    {
        currentCost = baseCost;
        InputManager.OnLeftClick += LeftClick;       
        UpdateCurrentCostText(); 
    }

    //slight delay for start so i can properly generate the grid
    private IEnumerator Start()
    {
        pauseState = FindFirstObjectByType<OnPausePlay>();
        canvasWidth = inventoryRect.rect.width;
        canvasHeight = inventoryRect.rect.height;
        CurrentInventoryState = InventoryState.normal;

        for(int x =  0; x < inventoryWidth; x++)
        {
            for(int y = 0; y < inventoryHeight; y++)
            {
                inventoryGrid[x, y] = new InvCellData(null, true);
            }
        }

        GenerateGrid();
        InputManager.OnLeftClick += LeftClick;

        if (spawnItemInInventory)
        {
            foreach (ItemSO startingItem in startingItems)
            {
                AddItem(itemSpawn, startingItem);
                yield return null;
            }
        }
        else
        {
            SpawnItems(itemSpawn, startingItems, spawnCentreOffset, spawnMargin);
        }
    }


    private void Update()
    {
        
        Vector2 mouseBottomLeft = getMousePosGrid();
        
        //Debug.Log($"Local mouse: {mouse}, success={success}");

        if (inventoryGrid != null && pauseState.IsOnSettings == false)
        {
            Vector2Int cellPos = GetCellAtPos(mouseBottomLeft);
            EnlargeOnHover(cellPos);

            //debug shit remove later
            foreach(InvCellData cell in inventoryGrid)
            {
                if(cell.active && cell.item != null)
                {
                    RectTransform cellRect = cell.cellObject.GetComponent<RectTransform>();
                    cell.cellObject.transform.localScale = Vector3.Lerp(cellRect.localScale, new Vector3(1.1f, 1.1f, 1f), 0.3f);
                }
            }
        }

        //for if stuff needs to be done for different inventory states
        switch(currentInventoryState)
        {
            case InventoryState.normal:

                break;
            case InventoryState.locked:

                break;
            case InventoryState.adding:
                break;
        }
    }
    #endregion

    #region input handling
    private void LeftClick()
    {
        Debug.Log("InventoryGridScript: LeftClick()");
        if(currentInventoryState == InventoryState.adding)
        {
            foreach (Vector2 pos in GetExpendableCells())
            {
                Debug.Log(GetCellAtPos(getMousePosGrid()) == pos);
                if (GetCellAtPos(getMousePosGrid()) == pos && GameManager.instance.playerStatus.Scraps >= currentCost)
                {
                    GameManager.instance.playerStatus.ConsumeScraps(currentCost);
                    currentCost = (int)(currentCost * costMultiplier);
                    UpdateCurrentCostText();
                    ExpandInventory(pos);
                    return;
                }
            }
        }    
    }
    #endregion

    private void GenerateGrid()
    {
        foreach(InvCellData cell in inventoryGrid) Destroy(cell.cellObject);

        for (int x = 0; x < inventoryWidth; x++)
        {
            for (int y = 0; y < inventoryHeight; y++)
            {
                if (inventoryGrid[x, y].active == true)
                {
                    GameObject newCell = Instantiate(inventoryCell, inventoryRect);
                    RectTransform cellRect = newCell.GetComponent<RectTransform>();

                    //Debug.Log("NEW INVENTORY CELL CREATED");

                    inventoryGrid[x, y].cellObject = newCell;

                    newCell.GetComponent<DebugInventorySlot>().setInfo((int)x, (int)y);

                    // Anchor to top-right corner of canvas
                    cellRect.anchorMin = new Vector2(0.5f, 0.5f);
                    cellRect.anchorMax = new Vector2(0.5f, 0.5f);
                    cellRect.pivot = new Vector2(0.5f, 0.5f);

                    // Position each cell from left to right, up to bottom using the top right of the screen as a pivot
                    Vector2 canvasTopLeft = GetTopLeftGrid();

                    Vector2 cellPos = canvasTopLeft + new Vector2(x * cellSize + cellSize / 2, -y * cellSize + cellSize / 2);
                    cellRect.anchoredPosition = cellPos;

                    cellRect.SetAsFirstSibling();
                }
            }
        }
    }

    /// <summary>
    /// mark the cells within the shape in the grid with the item's id
    /// </summary>
    /// <param name="startingCell"></param>
    /// <param name="itemShape"></param>
    /// <param name="itemId"></param>
    public void MarkCells(Vector2Int startingCell, ItemShapeCell[,] itemShape, GameObject itemId)
    {
        for (int x = 0; x < itemShape.GetLength(0); x++)
        {
            for (int y = 0; y < itemShape.GetLength(1); y++)
            {
                if (itemShape[x,y].filled)
                {
                    InvCellData targetCell = inventoryGrid[startingCell.x + x, startingCell.y + y];
                    if(targetCell.active) targetCell.item = itemId;
                }
            }
        }
        CheckItems();
    }

    public List<GameObject> GetAdjacentComponents(Vector2Int startingCell, ItemShapeCell[,] itemShape, GameObject itemId)
    {
        List<GameObject> adjacentComponents = new List<GameObject>();
        for (int x = 0; x < itemShape.GetLength(0); x++)
        {
            for (int y = 0; y < itemShape.GetLength(1); y++)
            {
                if (itemShape[x, y].filled)
                {
                    Vector2 targetCellPos = new Vector2(startingCell.x + x, startingCell.y + y);
                    InvCellData targetCell = inventoryGrid[(int)targetCellPos.x,(int)targetCellPos.y];
                    if (targetCell.active && targetCell.item == itemId)
                    {
                        foreach(Vector2 adjacentCell in GetAdjacentCell(targetCellPos))
                        {
                            if (InGrid(adjacentCell))
                            {
                                InvCellData adjacentItemCell = inventoryGrid[(int)adjacentCell.x, (int)adjacentCell.y];
                                if (adjacentCell != null && adjacentItemCell.item != itemId && adjacentItemCell.item != null && !adjacentComponents.Contains(adjacentItemCell.item))
                                {
                                    adjacentComponents.Add(adjacentItemCell.item);
                                }
                            }
                        }
                    }
                }
            }
        }
        return adjacentComponents;
    }

    //Debug Function for fun~
    private void EnlargeOnHover(Vector2Int cellPos)
    {
        GameObject selectedCell;
        if (cellPos.x >= 0 && cellPos.x < inventoryWidth && cellPos.y >= 0 && cellPos.y < inventoryHeight)
        {
            if (inventoryGrid[cellPos.x, cellPos.y].active)
            {
                //Debug.Log($"on cell:{cellPos.x},{cellPos.y},{inventoryGrid[cellPos.x,cellPos.y].item}");
                selectedCell = inventoryGrid[cellPos.x, cellPos.y].cellObject;
                RectTransform cellRect = selectedCell.GetComponent<RectTransform>();
                cellRect.localScale = Vector3.Lerp(cellRect.localScale, new Vector3(1.2f, 1.2f, 1f), 0.3f);
                ResetCellScale(selectedCell);
            }
        }
        else
        {
            selectedCell = null;
            ResetCellScale(selectedCell);
        }
    }

    private void ResetCellScale(GameObject selectedCell)
    {
        foreach (InvCellData cellData in inventoryGrid)
        {
            if (cellData.active && cellData.cellObject != selectedCell && cellData.item == null )
            {
                RectTransform cellRect = cellData.cellObject.GetComponent<RectTransform>();
                while (cellRect != null && cellRect.localScale != new Vector3(defaultScale, defaultScale, 1f))
                {
                    cellRect.localScale = Vector3.Lerp(cellRect.localScale, new Vector3(defaultScale, defaultScale, 1f), 0.3f);
                }
            }
        }
    }

    /// <summary>
    /// adds the marked cell items into a list
    /// </summary>
    public void CheckItems()
    {
        Debug.Log("equipped Items :");
        equippedItems.Clear();
        foreach(InvCellData cellData in inventoryGrid)
        {
            if (cellData.active && cellData.item != null && !equippedItems.Contains(cellData.item))
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

    public void AddItem(GameObject itemPrefab, ItemSO itemData)
    {
        Debug.Log("starting placement search");

        Vector2Int? foundPos = FindFreeSpace(itemData);

        if (foundPos == null)
        {
            Debug.Log("No valid placement found!");
            return;
        }

        Vector2Int pos = (Vector2Int)foundPos;
        Debug.Log("valid spot found!");

        // Calculate anchor position of item centre
        int itemWidth = itemData.itemWidth;
        int itemHeight = itemData.itemHeight;
        Vector2 origin = GetLocalPosGrid(new Vector2(pos.x, pos.y));
        Vector2 itemCentre = origin + new Vector2(itemWidth * 8f - 8f, -itemHeight * 8f + 8f);

        // Spawn new item
        GameObject newItem = Instantiate(itemPrefab, inventoryRect);
        Item itemScript = newItem.GetComponent<Item>();
        itemScript.itemData = itemData;

        RectTransform itemRect = newItem.GetComponent<RectTransform>();
        itemRect.anchoredPosition = itemCentre;

        ItemDragManager dragScript = newItem.GetComponent<ItemDragManager>();
        StartCoroutine(AttachNextFrame(dragScript));

        Debug.Log("placed!");
    }


    public void DeleteItems ()
    {
        
    }
    
    private IEnumerator AttachNextFrame(ItemDragManager drag)
    {
        yield return null; // wait 1 frame
        drag.AttachToInventory();
    }

    private void SpawnItems(GameObject itemPrefab, List<ItemSO> itemList, Vector2 pos, float spawnMarginX)
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

    private void GenerateUpgradePreview()
    {
        if(previewCells.Count > 0)
        {
            ClearUpgradePreview();
        }

        List<Vector2> outerBounds = GetExpendableCells();
        foreach(Vector2 cell in outerBounds)
        {
            //if the preview cell is 
            if ((inventoryWidth >= inventoryWidthMax && (cell.x < 0 || cell.x >= inventoryWidth)) || 
               (inventoryHeight >= inventoryHeightMax && (cell.y < 0 || cell.y >= inventoryHeight)))
            {
                continue;
            }

            GameObject previewCell = Instantiate(inventoryCell, inventoryRect);
            RectTransform previewRect = previewCell.GetComponent<RectTransform>();

            // Anchor to top-right corner of canvas
            previewRect.anchorMin = new Vector2(0.5f, 0.5f);
            previewRect.anchorMax = new Vector2(0.5f, 0.5f);
            previewRect.pivot = new Vector2(0.5f, 0.5f);

            previewRect.anchoredPosition = GetLocalPosGrid(cell);

            Image previewImage = previewCell.GetComponent<Image>();
            previewImage.color = new Color(previewImage.color.r, previewImage.color.g, previewImage.color.b, 0.3f);

            previewRect.SetAsFirstSibling();

            previewCells.Add(previewCell);

            Debug.Log("SKADOOSH!");
        }
    }

    /// <summary>
    /// expends the inventory in the given position
    /// </summary>
    /// <param name="cellPos"></param>
    private void ExpandInventory(Vector2 cellPos)
    {
        InvCellData[,] newInventory;

        if (InGrid(cellPos))
        {
            inventoryGrid[(int)cellPos.x, (int)cellPos.y].active = true;
            foreach (InvCellData cell in inventoryGrid)
            {
                Destroy(cell.cellObject);
            }
        }
        else
        {
            if(!InGrid( new Vector2 (cellPos.x,0)) && inventoryWidth < inventoryWidthMax)
            {
                inventoryWidth += 1;
            }
            if(!InGrid(new Vector2 (0,cellPos.y)) && inventoryHeight < inventoryHeightMax)
            {
                inventoryHeight += 1;
            }

            newInventory = new InvCellData[inventoryWidth, inventoryHeight];

            int oldInventoryWidth = inventoryGrid.GetLength(0);
            int oldInventoryHeight = inventoryGrid.GetLength(1);

            int expandLeft = (Mathf.Sign(cellPos.x) == -1f) ? 1 : 0;
            int expandDown = ((Mathf.Sign(cellPos.y) == -1f)) ? 1 : 0;
            int expandRight = (cellPos.x >= oldInventoryWidth) ? 1 : 0;

            for (int x=0;x<inventoryWidth;x++)
            {
                for(int y=0;y<inventoryHeight;y++)
                {
                    if (x < oldInventoryWidth && y < oldInventoryHeight)
                    {
                        newInventory[x + expandLeft, y + expandDown] = inventoryGrid[x, y];
                    }
                    else
                    {
                        int newCellsPosX = x - (x * expandLeft);
                        int newCellsPosY = y - (y * expandDown);

                        newInventory[x - (x * expandLeft), y - (y * expandDown)] = new InvCellData(null, false);
                        if (newCellsPosX == (int)cellPos.x + expandLeft && newCellsPosY == (int)cellPos.y + expandDown) 
                        {
                            newInventory[newCellsPosX, newCellsPosY] = new InvCellData(null,true);
                        }
                        
                    }
                }
            }

            foreach(GameObject item in equippedItems)
            {
                if (item != null)
                {
                    item.GetComponent<RectTransform>().anchoredPosition += new Vector2(-GameManager.cellSize * expandRight, -GameManager.cellSize * expandDown);
                }
            }

            foreach (InvCellData cell in inventoryGrid){cell.item = null;}

            inventoryGrid = newInventory;
        }
        CurrentInventoryState = InventoryState.adding;
    }

    #region Helper Functions

    /// <summary>
    /// Searches the inventory grid for a valid placement location for an item.
    /// Returns the top-left cell position, or null if no valid spot is found.
    /// </summary>
    public Vector2Int? FindFreeSpace(ItemSO itemData)
    {
        int itemWidth = itemData.itemWidth;
        int itemHeight = itemData.itemHeight;
        ItemShapeCell[,] itemShape = itemData.GetShapeGrid();

        for (int x = 0; x < inventoryWidth; x++)
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

                        // Out of bounds
                        if (checkX >= inventoryWidth || checkY >= inventoryHeight)
                        {
                            canPlace = false;
                            break;
                        }

                        // Filled cell must be empty in inventory
                        if (itemShape[cellX, cellY].filled &&
                            inventoryGrid[checkX, checkY].item != null)
                        {
                            canPlace = false;
                            break;
                        }
                    }
                    if (!canPlace) break;
                }

                if (canPlace)
                    return new Vector2Int(x, y);
            }
        }

        return null; // no valid placement found
    }



    /// <summary>
    /// Gets the Local Coordinate position of the grid cell (ex: 230,100) based on array coordinates
    /// </summary>
    public Vector2 GetLocalPosGrid(Vector2 cellPos)
    {
        Vector2 canvasTopLeft = GetTopLeftGrid();

        float posX = canvasTopLeft.x + (cellPos.x * cellSize) + (cellSize * 0.5f);
        float posY = canvasTopLeft.y - (cellPos.y * cellSize) + (cellSize * 0.5f);
        return new Vector2(posX, posY);
    }

    /// <summary>
    /// Gets the Array Coordinate position of the grid cell (ex: [0,1]) based on local coordinates
    /// </summary>
    public Vector2Int GetCellAtPos(Vector2 pos)
    {
        Vector2 canvasTopLeft = GetTopLeftGrid();

        Vector2 mouseGrid = new Vector2(pos.x - canvasTopLeft.x, canvasTopLeft.y - pos.y + cellSize);
        int cellX = Mathf.FloorToInt(mouseGrid.x / cellSize);
        int cellY = Mathf.FloorToInt(mouseGrid.y / cellSize);

        return new Vector2Int(cellX, cellY);
    }

    private Vector2 GetTopLeftGrid()
    {
        Vector2 gridSize = new Vector2(inventoryWidth * cellSize, inventoryHeight * cellSize);
        return new Vector2(canvasWidth * 0.5f - gridSize.x - margin.x, canvasHeight * 0.5f - cellSize - margin.y);
    }

    /// <summary>
    /// returns true if the 2D array cell position is within the grid bounds 
    /// </summary>
    /// <param name="cellPos">2D array coordinates</param>
    /// <returns></returns>
    public bool InGrid(Vector2 cellPos)
    {
        return (cellPos.x >= 0 && cellPos.x < inventoryWidth && cellPos.y >= 0 && cellPos.y < inventoryHeight);
    }
    
    public Vector2 getMousePosGrid()
    {
        Vector2 mousePos;
        //converting mouse pos from screenspace to local canvas space
        bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(inventoryRect, Mouse.current.position.ReadValue(), null, out mousePos);
        return mousePos;
    }

    /// <summary>
    /// Get expendable cells (adjacent empty cells)
    /// </summary>
    /// <returns></returns>
    private List<Vector2> GetExpendableCells()
    {
        List<Vector2> outerBounds = new List<Vector2>();
        //Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        for (int x = 0; x < inventoryWidth; x++)
        {
            for (int y = 0; y < inventoryHeight; y++)
            {
                foreach (Vector2 adjacentCell in GetAdjacentCell(new Vector2(x,y)))
                {
                    if (inventoryGrid[x,y].active)
                    { 
                        //Vector2Int adjacentCell = Vector2Int.FloorToInt(new Vector2(x, y) + direction);
                        if (!InGrid(adjacentCell) || inventoryGrid[(int)adjacentCell.x, (int)adjacentCell.y].active == false)
                        {
                            outerBounds.Add(adjacentCell);
                        }
                    }
                }
            }
        }
        return outerBounds;
    }


    public List<Vector2> GetAdjacentCell(Vector2 gridPos)
    {
        List<Vector2> adjacentList = new List<Vector2>();
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        foreach(Vector2 direction in directions)
        {
            adjacentList.Add(gridPos + direction);
        }
        return adjacentList;
    }

    private void ClearUpgradePreview()
    {
        foreach (GameObject previewCell in previewCells) Destroy(previewCell);
        previewCells.Clear();
    }

    private void UpdateCurrentCostText()
    {
        if (costText != null) costText.text = currentCost.ToString();
    }
    #endregion

    /// <summary>
    /// Public getter for item spawn prefab
    /// Used by reward system to spawn items
    /// </summary>
    public GameObject ItemSpawnPrefab 
    { 
        get { return itemSpawn; } 
    }
}