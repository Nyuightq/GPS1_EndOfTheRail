// --------------------------------------------------------------
// Creation Date: 2025-10-20 03:26
// Author: User
// Description: -
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    [SerializeField] private ItemSO itemData;
    private ItemShapeCell[,] itemShape;

    private Image image;
    private RectTransform rectTransform;

    int shapeHeight;
    int shapeWidth;
    int cellSize = 16;


    private void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        rectTransform.SetAsLastSibling();
        itemShape = itemData.getShapeGrid();

        shapeHeight = itemShape.GetLength(0)*cellSize;
        shapeWidth = itemShape.GetLength(1)*cellSize;

        if (itemData != null && itemData.itemSprite != null)
        {
            image.sprite = itemData.itemSprite;
            image.SetNativeSize();
        }
    }

    private void Update()
    {
        Vector2 topLeft = new Vector2(rectTransform.position.x - shapeWidth / 2, rectTransform.position.y + shapeHeight / 2);
        for(int x = 0; x< shapeWidth/cellSize;x++)
        {
            for(int y=0;  y< shapeHeight/cellSize;y++)
            {

            }
        }

    }
}
