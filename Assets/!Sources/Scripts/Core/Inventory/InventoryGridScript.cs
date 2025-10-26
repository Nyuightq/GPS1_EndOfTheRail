// --------------------------------------------------------------
// Creation Date: 2025-10-14 02:45
// Author: User
// Description: -
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.InputSystem;

public class InvCellData
{
    //public Vector2 arrayPos;
    public GameObject cellObject;
    public int itemId;

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
    RectTransform inventoryRect;
    [SerializeField] private float cellSize = 16f;
    [SerializeField] private Vector2 margin;

    [SerializeField] private float defaultScale = 0.9f;


    private float canvasWidth;
    private float canvasHeight;

    private Vector2 mouse;


    private InvCellData[,] inventoryGrid;

    void Start()
    {
        inventoryRect = inventoryCanvas.GetComponent<RectTransform>();
        inventoryGrid = new InvCellData[inventoryWidth, inventoryHeight];
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
                cellRect.anchorMin = new Vector2(0.0f, 0.0f);
                cellRect.anchorMax = new Vector2(0.0f, 0.0f);
                cellRect.pivot = new Vector2(0.5f, 0.5f);

                // Position each cell from left to right, up to bottom using the top right of the screen as a pivot
                Vector2 gridSize = new Vector2(inventoryWidth * cellSize, inventoryHeight * cellSize);

                Vector2 canvasTopRight = new Vector2(canvasWidth - gridSize.x - margin.x, canvasHeight - cellSize - margin.y);

                Vector2 cellPos = canvasTopRight + new Vector2(x * cellSize + cellSize/2, -y * cellSize + cellSize/2);
                cellRect.anchoredPosition = cellPos;
            }
        }
    }

    public Vector2Int getCellAtPos(Vector2 pos)
    {
        Vector2 gridSize = new Vector2(inventoryWidth * cellSize, inventoryHeight * cellSize);
        Vector2 canvasTopRight = new Vector2(canvasWidth - gridSize.x - margin.x, canvasHeight - margin.y);

        Vector2 mouseGrid = new Vector2(pos.x - canvasTopRight.x, canvasTopRight.y - pos.y);
        int cellX = Mathf.FloorToInt(mouseGrid.x / cellSize);
        int cellY = Mathf.FloorToInt(mouseGrid.y / cellSize);

        return new Vector2Int(cellX, cellY);
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
        return mousePos + new Vector2(canvasWidth / 2, canvasHeight / 2);
    }

    //Debug Function for fun~
    private void EnlargeOnHover(Vector2Int cellPos)
    {
        GameObject selectedCell;
        if (cellPos.x >= 0 && cellPos.x < inventoryWidth && cellPos.y >= 0 && cellPos.y < inventoryHeight)
        {
            Debug.Log($"on cell:{cellPos.x},{cellPos.y}");
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
            if (cellData.cellObject != selectedCell)
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