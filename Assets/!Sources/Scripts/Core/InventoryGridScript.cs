// --------------------------------------------------------------
// Creation Date: 2025-10-14 02:45
// Author: User
// Description: -
// --------------------------------------------------------------
using UnityEngine;

public class InventoryGridScript : MonoBehaviour
{
    [SerializeField] private int inventoryWidth;
    [SerializeField] private int inventoryHeight;

    [SerializeField] private GameObject inventoryCell;
    [SerializeField] private Canvas inventoryCanvas;
    [SerializeField] private int margin;

    private RectTransform inventoryRect;
    private float canvasWidth;
    private float canvasHeight;

    private float cellSize = 16f;

    private int[,] inventoryGrid;

    void Awake()
    {
        inventoryRect = inventoryCanvas.GetComponent<RectTransform>();
        inventoryGrid = new int[inventoryWidth , inventoryHeight];
        canvasWidth = inventoryRect.rect.width;
        canvasHeight = inventoryRect.rect.height;
        generateGrid();
    }

    void generateGrid()
    {
        for (int x = 0; x < inventoryWidth; x++)
        {
            for (int y = 0; y < inventoryHeight; y++)
            {
                GameObject newCell = Instantiate(inventoryCell, inventoryRect);
                RectTransform cellRect = newCell.GetComponent<RectTransform>();

                // Anchor to top-right corner of canvas
                cellRect.anchorMin = new Vector2(1, 1);
                cellRect.anchorMax = new Vector2(1, 1);
                cellRect.pivot = new Vector2(1, 1);

                // Position each cell downward and leftward from the top-right
                Vector2 cellPos = new Vector2((-x * cellSize)-margin, (-y * cellSize)-margin);
                cellRect.anchoredPosition = cellPos;
            }
        }
    }
}
