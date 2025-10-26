// --------------------------------------------------------------
// Creation Date: 2025-10-20 01:24
// Author: User
// Description: -
// --------------------------------------------------------------
using System;
using UnityEngine;

[System.Serializable]
public class ItemShapeCell
{
    public bool filled;
}

[CreateAssetMenu(fileName = "ItemTemplate", menuName = "Item")]
public class ItemSO : ScriptableObject
{
    public Sprite itemSprite;
    public string itemName;
    public int itemWidth, itemHeight;

    [SerializeField] private ItemShapeCell[] itemShape;

    private void OnEnable()
    {
        if (itemShape == null || itemShape.Length != itemWidth * itemHeight)
        {
            resizeShape();
        }
    }

    public ItemShapeCell GetCell(int x, int y)
    {
        if (itemShape == null || x < 0 || y < 0 || x >= itemWidth || y >= itemHeight)
            return null;

        int index = y * itemWidth + x;
        if (index >= itemShape.Length)
            resizeShape();

        return itemShape[index];
    }

    public void resizeShape()
    {
        int newSize = itemWidth * itemHeight;
        ItemShapeCell[] newShape = new ItemShapeCell[newSize];

        if (itemShape != null)
        {
            int copyLength = Mathf.Min(itemShape.Length, newSize);
            Array.Copy(itemShape, newShape, copyLength);
        }

        for (int i = 0; i < newSize; i++)
        {
            if (newShape[i] == null)
                newShape[i] = new ItemShapeCell();
        }

        itemShape = newShape;
    }

    public ItemShapeCell[,] getShapeGrid()
    {
        int totalCells = itemWidth * itemHeight;

        if (itemShape == null || itemShape.Length != totalCells)
        {
            resizeShape(); // auto-fix
        }

        ItemShapeCell[,] trueGrid = new ItemShapeCell[itemWidth, itemHeight];

        for (int y = 0; y < itemHeight; y++)
        {
            for (int x = 0; x < itemWidth; x++)
            {
                int index = y * itemWidth + x;
                if (index < itemShape.Length)
                    trueGrid[x, y] = itemShape[index];
            }
        }
        return trueGrid;
    }
}
