// --------------------------------------------------------------
// Creation Date: 2025-10-14 02:45
// Author: User
// Description: -
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

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
    [SerializeField] private int inventoryWidth;
    [SerializeField] private int inventoryHeight;

    [SerializeField] private GameObject inventoryCell;
    [SerializeField] private Canvas inventoryCanvas;
    public RectTransform inventoryRect { get; private set; } 
    [SerializeField] public float cellSize = 16f;
    [SerializeField] private Vector2 margin;

    [SerializeField] private float defaultScale = 0.9f;

    [SerializeField] GameObject itemSpawn;


    private float canvasWidth;
    private float canvasHeight;

    private Vector2 mouse;


    public InvCellData[,] inventoryGrid;

    void Awake()
    {
        inventoryRect = inventoryCanvas.GetComponent<RectTransform>();
        inventoryGrid = new InvCellData[inventoryWidth, inventoryHeight];
        
    }

    private void Start()
    {
        canvasWidth = inventoryRect.rect.width;
        canvasHeight = inventoryRect.rect.height;

        generateGrid();
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
                    cell.cellObject.transform.localScale = Vector3.Lerp(cellRect.localScale, new Vector3(1f, 1f, 1f), 0.3f);
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
                Vector2 gridSize = new Vector2(inventoryWidth * cellSize, inventoryHeight * cellSize);

                Vector2 canvasTopRight = new Vector2(canvasWidth*0.5f - gridSize.x - margin.x, canvasHeight*0.5f - cellSize - margin.y);

                Vector2 cellPos = canvasTopRight + new Vector2(x * cellSize + cellSize/2, -y * cellSize + cellSize/2);
                cellRect.anchoredPosition = cellPos;

                cellRect.SetAsFirstSibling();
            }
        }
    }

    //returns the actual position in local space that the cell position takes up
    public Vector2 getLocalPosGrid(Vector2 cellPos)
    {
        Vector2 gridSize = new Vector2(inventoryWidth * cellSize, inventoryHeight * cellSize);
        Vector2 canvasTopRight = new Vector2(canvasWidth * 0.5f - gridSize.x - margin.x, canvasHeight * 0.5f - margin.y);

        float posX = canvasTopRight.x + (cellPos.x * cellSize) + (cellSize * 0.5f);
        float posY = canvasTopRight.y - (cellPos.y * cellSize) - (cellSize * 0.5f);
        return new Vector2(posX, posY);
    }

    public Vector2Int getCellAtPos(Vector2 pos)
    {
        Vector2 gridSize = new Vector2(inventoryWidth * cellSize, inventoryHeight * cellSize);
        Vector2 canvasTopRight = new Vector2(canvasWidth * 0.5f - gridSize.x - margin.x, canvasHeight * 0.5f - margin.y);

        Vector2 mouseGrid = new Vector2(pos.x - canvasTopRight.x, canvasTopRight.y - pos.y);
        int cellX = Mathf.FloorToInt(mouseGrid.x / cellSize);
        int cellY = Mathf.FloorToInt(mouseGrid.y / cellSize);

        return new Vector2Int(cellX, cellY);
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
    }

    //Debug Function for fun~
    private void EnlargeOnHover(Vector2Int cellPos)
    {
        GameObject selectedCell;
        if (cellPos.x >= 0 && cellPos.x < inventoryWidth && cellPos.y >= 0 && cellPos.y < inventoryHeight)
        {
            //Debug.Log($"on cell:{cellPos.x},{cellPos.y}");
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
}