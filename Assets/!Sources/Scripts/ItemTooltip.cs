// --------------------------------------------------------------
// Creation Date: 2025-11-16
// Author: User
// Description: Handles item tooltip display on hover
// --------------------------------------------------------------
using UnityEngine;
using TMPro;

public class ItemTooltip : MonoBehaviour
{
    [Header("Tooltip UI References")]
    [SerializeField] private RectTransform tooltipBackground;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    
    [Header("Tooltip Settings")]
    [SerializeField] private Vector2 offset = new Vector2(10f, -10f);
    [SerializeField] private float padding = 10f;
    [SerializeField] private float maxWidth = 300f; // Maximum width for the tooltip
    
    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private bool isVisible = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        
        // Hide tooltip on start
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        ItemDragManager.OnItemHoverEnter += ShowTooltip;
        ItemDragManager.OnItemHoverExit += HideTooltip;
    }

    private void OnDisable()
    {
        ItemDragManager.OnItemHoverEnter -= ShowTooltip;
        ItemDragManager.OnItemHoverExit -= HideTooltip;
    }

    private void Update()
    {
        if (isVisible)
        {
            // Follow mouse position
            Vector2 mousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                Input.mousePosition,
                parentCanvas.worldCamera,
                out mousePosition
            );
            
            rectTransform.anchoredPosition = mousePosition + offset;
        }
    }

    private void ShowTooltip(Item item)
    {
        if (item == null || item.itemData == null) return;

        // Set text from ItemSO
        itemNameText.text = item.itemData.itemName;
        itemDescriptionText.text = item.itemData.itemDescription;

        // Set max width for description text to enable wrapping
        itemDescriptionText.rectTransform.sizeDelta = new Vector2(maxWidth - (padding * 2), itemDescriptionText.rectTransform.sizeDelta.y);

        // Force text to update
        itemNameText.ForceMeshUpdate();
        itemDescriptionText.ForceMeshUpdate();

        // Calculate required size based on text
        float nameHeight = itemNameText.preferredHeight;
        float descHeight = itemDescriptionText.preferredHeight;
        float textWidth = Mathf.Min(Mathf.Max(itemNameText.preferredWidth, itemDescriptionText.preferredWidth), maxWidth - (padding * 2));

        // Resize background to fit text with padding
        tooltipBackground.sizeDelta = new Vector2(
            textWidth + (padding * 2),
            nameHeight + descHeight + (padding * 3) // Extra padding between texts
        );

        gameObject.SetActive(true);
        isVisible = true;
    }

    private void HideTooltip()
    {
        gameObject.SetActive(false);
        isVisible = false;
    }
}