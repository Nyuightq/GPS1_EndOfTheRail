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
    [SerializeField] private ItemSO itemData;
    [SerializeField] private GameObject inventoryCellPrefab;
    public ItemShapeCell[,] itemShape { get; private set; }

    private Image image;
    public RectTransform rectTransform;

    private GameObject shapePreviewCell;
    public List<GameObject> shape = new List<GameObject>();

    int shapeHeight;
    int shapeWidth;
    int cellSize = 16;


    private void Awake()
    {
        rectTransform = sprite.GetComponent<RectTransform>();
        image = sprite.GetComponent<Image>();

        itemShape = itemData.getShapeGrid();

        shapeHeight = itemShape.GetLength(1) * cellSize;
        shapeWidth = itemShape.GetLength(0) * cellSize;

        
    }

    private void Start()
    {
        if (itemData != null && itemData.itemSprite != null)
        {
            image.sprite = itemData.itemSprite;
        }
        image.SetNativeSize();
        RectTransform itemRect = GetComponent<RectTransform>();
        itemRect.sizeDelta = rectTransform.sizeDelta;
        //itemRect.

        //rectTransform.pivot = image.sprite.pivot / image.sprite.rect.size;


        Vector2 topLeft = new Vector2(rectTransform.position.x - shapeWidth / 2, rectTransform.position.y + shapeHeight / 2);

        generatePreview();
        rectTransform.SetAsLastSibling();
        GetComponent<RectTransform>().SetAsLastSibling();
    }

    private void generatePreview()
    {
        for (int x = 0; x < shapeWidth / cellSize; x++)
        {
            for (int y = 0; y < shapeHeight / cellSize; y++)
            {
                if (itemShape[x, y].filled)
                {
                    GameObject newCell = Instantiate(inventoryCellPrefab, GetComponent<RectTransform>());

                    newCell.GetComponent<DebugInventorySlot>().setInfo((int)x, (int)y);

                    RectTransform shapeRect = newCell.GetComponent<RectTransform>();

                    shapeRect.localScale = new Vector3(1, 1, 1);
                    Image shapeImage = newCell.GetComponent<Image>();
                    Color c = shapeImage.color;
                    c.a = 0.5f;
                    shapeImage.color = c;

                    shapeRect.anchorMin = new Vector2(0f, 1f);
                    shapeRect.anchorMax = new Vector2(0f, 1f);
                    shapeRect.pivot = new Vector2(0.5f, 0.5f);

                    shapeRect.anchoredPosition = new Vector2(x * cellSize + cellSize * 0.5f, -y * cellSize - cellSize * 0.5f);

                    shape.Add(newCell);
                    Debug.Log("added new cell");
                }
            }
        }
    }

}
