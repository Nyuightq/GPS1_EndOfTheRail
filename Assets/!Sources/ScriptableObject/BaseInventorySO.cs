// --------------------------------------------------------------
// Creation Date: 2025-12-02 12:47
// Author: User
// Description: -
// --------------------------------------------------------------
using UnityEngine;
using static Item;

[System.Serializable]
public class BaseInventoryCell
{
    public bool filled;
}


[CreateAssetMenu(fileName = "InventoryTemplate", menuName = "Inventory Style")]
public class BaseInventorySO : ScriptableObject
{
    public int width = 5;
    public int height = 5;

    public BaseInventoryCell[] inventoryShape;

    private void OnValidate()
    {
        int targetSize = width * height;

        if (inventoryShape == null || inventoryShape.Length != targetSize)
        {
            BaseInventoryCell[] newArray = new BaseInventoryCell[targetSize];

            for (int i = 0; i < targetSize; i++)
            {
                if (inventoryShape != null && i < inventoryShape.Length)
                    newArray[i] = inventoryShape[i];
                else
                    newArray[i] = new BaseInventoryCell();
            }

            inventoryShape = newArray;
        }
    }

    public BaseInventoryCell[,] getInventoryShapeGrid()
    {
        int totalCells = width * height;
        BaseInventoryCell[,] trueGrid = new BaseInventoryCell[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (index < inventoryShape.Length)
                    trueGrid[x, y] = inventoryShape[index];
            }
        }
        return trueGrid;
    }
}
