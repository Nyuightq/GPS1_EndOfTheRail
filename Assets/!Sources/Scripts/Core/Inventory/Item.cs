// --------------------------------------------------------------
// Creation Date: 2025-10-20 03:26
// Author: User
// Description: -
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Item : MonoBehaviour
{
    public enum itemState
    {
        equipped,
        unequipped
    }

    public itemState state = itemState.unequipped;

    [SerializeField] private Image sprite;
    [SerializeField] public ItemSO itemData;
    public ItemEffect itemEffect;
    [SerializeField] private GameObject inventoryCellPrefab;
    [SerializeField] private float shapePreviewAlpha = 0.5f;
    public ItemShapeCell[,] itemShape { get; private set; }
    public RectTransform spriteRectTransform, itemRect;
    public List<GameObject> shape = new List<GameObject>();
    int shapeLocalHeight,shapeLocalWidth;

    int cellSize = GameManager.cellSize;


    #region Unity LifeCycle
    private void OnEnable()
    {
        spriteRectTransform = sprite.GetComponent<RectTransform>();
    }

    private void Start()
    {
        //Get the item Shape 2D Array 
        itemShape = itemData.GetShapeGrid();

        if (itemData != null && itemData.itemSprite != null)
        {
            sprite.sprite = itemData.itemSprite;
        }

        if (itemEffect == null && itemData.itemEffectPrefab != null)
        {
            var effectObj = Instantiate(itemData.itemEffectPrefab, transform);
            itemEffect = effectObj.GetComponent<ItemEffect>();
        }

        //Ensure that the sprite child and the actual object size is equal
        sprite.SetNativeSize();
        itemRect = GetComponent<RectTransform>();
        itemRect.sizeDelta = spriteRectTransform.sizeDelta;

        GeneratePreview();
        spriteRectTransform.SetAsLastSibling();
        GetComponent<RectTransform>().SetAsLastSibling();
    }
    #endregion

    private void GeneratePreview()
    {
        shapeLocalHeight = itemShape.GetLength(1) * cellSize;
        shapeLocalWidth = itemShape.GetLength(0) * cellSize;

        for (int x = 0; x < shapeLocalWidth / cellSize; x++)
        {
            for (int y = 0; y < shapeLocalHeight / cellSize; y++)
            {
                if (itemShape[x, y].filled)
                {
                    GameObject newCell = Instantiate(inventoryCellPrefab, GetComponent<RectTransform>());

                    newCell.GetComponent<DebugInventorySlot>().setInfo((int)x, (int)y);

                    RectTransform shapeRect = newCell.GetComponent<RectTransform>();

                    shapeRect.localScale = Vector3.one;
                    Image shapeImage = newCell.GetComponent<Image>();
                    shapeImage.color = new Color(shapeImage.color.r,shapeImage.color.g,shapeImage.color.b,shapePreviewAlpha);

                    shapeRect.anchorMin = new Vector2(0f, 1f);
                    shapeRect.anchorMax = new Vector2(0f, 1f);
                    shapeRect.pivot = new Vector2(0.5f, 0.5f);

                    //get the position and offset to the centre of that cell
                    Vector2 cellPos = new Vector2(x * cellSize + cellSize * 0.5f, -y * cellSize - cellSize * 0.5f);
                    shapeRect.anchoredPosition = cellPos;

                    shape.Add(newCell);
                    //Debug.Log("added new cell");
                }
            }
        }
        //Make the Sprite of the item appear infront
        spriteRectTransform.SetAsLastSibling();
    }

    public void RotateShape(ItemShapeCell[,] currentItemShape)
    {
        int shapeHeight = currentItemShape.GetLength(1);
        int shapeWidth = currentItemShape.GetLength(0);

        ItemShapeCell[,] rotatedShape = new ItemShapeCell[shapeHeight, shapeWidth];

        for (int x = 0; x < shapeWidth; x++)
        {
            for (int y = 0; y < shapeHeight; y++)
            {
                rotatedShape[shapeHeight - 1 - y, x] = currentItemShape[x, y];
            }
        }
        
        RefreshShape(rotatedShape);

        spriteRectTransform.localRotation *= Quaternion.Euler(0, 0, -90f);

        //resize item rect to match rotated sprite
        RectTransform itemRect = GetComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(itemRect.sizeDelta.y,itemRect.sizeDelta.x);

        Debug.Log("rotated!!");
        //return rotatedShape;
    }

    public void RefreshShape(ItemShapeCell[,] newShape)
    {
        foreach (GameObject cell in shape)
        {
            Destroy(cell);
        }
        shape.Clear();
        itemShape = newShape;
        GeneratePreview();
        Debug.Log("REFRESHED!");
    }
}
