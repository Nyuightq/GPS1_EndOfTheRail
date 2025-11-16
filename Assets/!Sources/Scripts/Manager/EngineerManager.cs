using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class EngineerManager : MonoBehaviour
{
    public static EngineerManager Instance { get; private set; }
    public static event System.Action OnEngineerClosed;

    [System.Serializable]
    public class MergeSlot
    {
        [Header("UI References")]
        public RectTransform slotAnchor;
        public Image slotImage;
        
        [Header("Runtime Data")]
        public ItemSO slottedItemSO;
        public GameObject slottedItemObject;
        public Vector2 originalInventoryPosition;
    }

    [Header("Engineer UI")]
    [SerializeField] private GameObject engineerUIPanel;
    
    [Header("Merge Slots (2 input slots)")]
    [SerializeField] private MergeSlot slot1;
    [SerializeField] private MergeSlot slot2;
    
    [Header("Result Display")]
    [SerializeField] private RectTransform resultAnchor;
    [SerializeField] private Image resultImage;
    [SerializeField] private TMP_Text resultText;
    
    [Header("Buttons")]
    [SerializeField] private Button mergeButton;
    [SerializeField] private Button declineButton;
    
    [Header("Feedback")]
    [SerializeField] private TMP_Text feedbackText;
    
    [Header("Tween Settings")]
    [SerializeField] private float tweenDuration = 0.3f;

    private InventoryGridScript inventoryGrid;
    public bool IsEngineerUIActive { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (engineerUIPanel != null)
            engineerUIPanel.SetActive(false);
    }

    private void Start()
    {
        inventoryGrid = FindFirstObjectByType<InventoryGridScript>();
        
        if (inventoryGrid == null)
        {
            Debug.LogError("EngineerManager: InventoryGridScript not found!");
        }

        // Setup button listeners
        if (mergeButton != null)
        {
            mergeButton.onClick.RemoveAllListeners();
            mergeButton.onClick.AddListener(OnMergeButtonClicked);
        }
        
        if (declineButton != null)
        {
            declineButton.onClick.RemoveAllListeners();
            declineButton.onClick.AddListener(OnDeclineButtonClicked);
        }

        // Initialize slots
        if (slot1 != null)
        {
            slot1.slottedItemSO = null;
            slot1.slottedItemObject = null;
        }
        
        if (slot2 != null)
        {
            slot2.slottedItemSO = null;
            slot2.slottedItemObject = null;
        }
    }

    public void OpenEngineerUI(GameObject player = null)
    {
        if (IsEngineerUIActive)
        {
            Debug.Log("[EngineerManager] Engineer UI already open");
            return;
        }

        ClearSlots();
        engineerUIPanel.SetActive(true);
        IsEngineerUIActive = true;
        
        if (feedbackText != null)
            feedbackText.text = "Drag two identical items to merge";
            
        Debug.Log("[EngineerManager] Engineer UI opened");
    }

    /// <summary>
    /// Called when item starts being dragged - ensures item renders on top
    /// </summary>
    public void OnItemDragStarted(GameObject item)
    {
        if (!IsEngineerUIActive)
            return;

        RectTransform itemRect = item.GetComponent<RectTransform>();
        
        // Move to canvas root during drag so it appears on top of everything
        // This matches the DraggableItem pattern from the reference
        itemRect.SetParent(itemRect.root); 
        itemRect.SetAsLastSibling(); // Render on top
        
        Debug.Log($"[EngineerManager] Item moved to root for dragging: {item.name}");
    }

    /// <summary>
    /// Called by ItemDragManager when item is released
    /// </summary>
    public void OnItemReleased(GameObject item)
    {
        if (!IsEngineerUIActive)
            return;

        Item itemScript = item.GetComponent<Item>();
        if (itemScript == null || itemScript.itemData == null)
            return;

        RectTransform itemRect = item.GetComponent<RectTransform>();
        
        // IMPORTANT: Convert world position to engineer panel's local space
        Vector3 worldPos = itemRect.position;
        Vector2 localPointInPanel;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            engineerUIPanel.GetComponent<RectTransform>(), 
            RectTransformUtility.WorldToScreenPoint(null, worldPos), 
            null, 
            out localPointInPanel
        );

        // Check which slot the item is over using panel-local coordinates
        MergeSlot targetSlot = GetSlotAtPosition(localPointInPanel);

        if (targetSlot != null)
        {
            // Try to slot the item
            if (TrySlotItem(targetSlot, item, itemScript))
            {
                Debug.Log($"✓ Item {itemScript.itemData.itemName} slotted successfully");
                
                // Check if merge is valid
                CheckMergeValidity();
            }
            else
            {
                // Slot occupied or invalid, snap back
                SnapBackToInventory(item, itemScript);
            }
        }
        else
        {
            // Not over any slot, snap back to inventory
            SnapBackToInventory(item, itemScript);
        }
    }

    /// <summary>
    /// Attempts to slot an item into a merge slot
    /// </summary>
    private bool TrySlotItem(MergeSlot slot, GameObject itemObject, Item itemScript)
    {
        // Check if slot is already occupied
        if (slot.slottedItemObject != null)
        {
            if (feedbackText != null)
                feedbackText.text = "Slot already occupied!";
            return false;
        }

        // Store original inventory position before slotting
        slot.originalInventoryPosition = itemObject.GetComponent<RectTransform>().anchoredPosition;

        // Change parent to engineer panel
        RectTransform itemRect = itemObject.GetComponent<RectTransform>();
        Vector3 worldPos = itemRect.position;
        itemRect.SetParent(engineerUIPanel.transform);
        itemRect.position = worldPos;

        // Clear item from inventory grid
        if (inventoryGrid != null)
        {
            ClearItemFromInventory(itemObject, itemScript);
        }

        // Snap to slot anchor
        StartCoroutine(MoveToSlotRoutine(itemRect, slot.slotAnchor.anchoredPosition, tweenDuration));

        // Store slot data
        slot.slottedItemSO = itemScript.itemData;
        slot.slottedItemObject = itemObject;
        itemScript.state = Item.itemState.unequipped;

        return true;
    }

    /// <summary>
    /// Gets the slot at given position (if any)
    /// </summary>
    private MergeSlot GetSlotAtPosition(Vector2 position)
    {
        // Check slot 1
        if (slot1?.slotAnchor != null && IsPositionInSlot(position, slot1.slotAnchor))
        {
            return slot1;
        }

        // Check slot 2
        if (slot2?.slotAnchor != null && IsPositionInSlot(position, slot2.slotAnchor))
        {
            return slot2;
        }

        return null;
    }

    /// <summary>
    /// Checks if position is within slot bounds
    /// </summary>
    private bool IsPositionInSlot(Vector2 position, RectTransform slotRect)
    {
        float threshold = 50f; // Adjust based on slot size
        float distance = Vector2.Distance(position, slotRect.anchoredPosition);
        return distance < threshold;
    }

    /// <summary>
    /// Checks if both slots have valid matching items
    /// </summary>
    private void CheckMergeValidity()
    {
        if (slot1.slottedItemSO == null || slot2.slottedItemSO == null)
        {
            // Not both slots filled
            if (feedbackText != null)
                feedbackText.text = "Place two identical items to merge";
            return;
        }

        // Check if items are identical
        bool isMergeValid = slot1.slottedItemSO.itemName == slot2.slottedItemSO.itemName;

        if (isMergeValid)
        {
            Debug.Log($"[EngineerManager] IsMergeValid = true (Items: {slot1.slottedItemSO.itemName})");
            
            if (feedbackText != null)
                feedbackText.text = $"Ready to merge: {slot1.slottedItemSO.itemName}";
                
            if (mergeButton != null)
                mergeButton.interactable = true;
        }
        else
        {
            Debug.Log($"[EngineerManager] IsMergeValid = false (Items don't match: {slot1.slottedItemSO.itemName} != {slot2.slottedItemSO.itemName})");
            
            if (feedbackText != null)
                feedbackText.text = "Items don't match! Returning second item...";
                
            if (mergeButton != null)
                mergeButton.interactable = false;

            // Return the second item to inventory
            StartCoroutine(ReturnItemToInventoryDelayed(slot2, 0.5f));
        }
    }

    /// <summary>
    /// Snaps item back to its original inventory position
    /// </summary>
    private void SnapBackToInventory(GameObject itemObject, Item itemScript)
    {
        if (itemObject == null || itemScript == null)
            return;

        Debug.Log($"Snapping {itemScript.itemData.itemName} back to inventory");

        RectTransform itemRect = itemObject.GetComponent<RectTransform>();
        
        // Find the slot this item belongs to (if any)
        MergeSlot ownerSlot = null;
        if (slot1?.slottedItemObject == itemObject)
            ownerSlot = slot1;
        else if (slot2?.slottedItemObject == itemObject)
            ownerSlot = slot2;

        if (ownerSlot != null)
        {
            // Return to stored inventory position
            Vector3 worldPos = itemRect.position;
            itemRect.SetParent(inventoryGrid.inventoryRect);
            itemRect.position = worldPos;

            StartCoroutine(MoveToSlotRoutine(itemRect, ownerSlot.originalInventoryPosition, tweenDuration, () =>
            {
                // Re-attach to inventory after tweening
                ItemDragManager dragManager = itemObject.GetComponent<ItemDragManager>();
                if (dragManager != null)
                {
                    dragManager.AttachToInventory();
                }
            }));

            // Clear slot
            ownerSlot.slottedItemSO = null;
            ownerSlot.slottedItemObject = null;
        }
        else
        {
            // Item wasn't slotted yet, just re-attach to inventory
            ItemDragManager dragManager = itemObject.GetComponent<ItemDragManager>();
            if (dragManager != null)
            {
                dragManager.AttachToInventory();
            }
        }

        if (feedbackText != null)
            feedbackText.text = "Item returned to inventory";
    }

    /// <summary>
    /// Coroutine to return item to inventory after delay
    /// </summary>
    private IEnumerator ReturnItemToInventoryDelayed(MergeSlot slot, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (slot?.slottedItemObject != null)
        {
            Item itemScript = slot.slottedItemObject.GetComponent<Item>();
            SnapBackToInventory(slot.slottedItemObject, itemScript);
        }
    }

    /// <summary>
    /// Clears item from inventory grid
    /// </summary>
    private void ClearItemFromInventory(GameObject itemObject, Item itemScript)
    {
        if (inventoryGrid == null || itemScript == null)
            return;

        InvCellData[,] grid = inventoryGrid.inventoryGrid;
        bool foundItem = false;

        for (int x = 0; x < grid.GetLength(0) && !foundItem; x++)
        {
            for (int y = 0; y < grid.GetLength(1) && !foundItem; y++)
            {
                if (grid[x, y].item == itemObject)
                {
                    Vector2Int topLeft = new Vector2Int(x, y);
                    inventoryGrid.MarkCells(topLeft, itemScript.itemShape, null);
                    Debug.Log($"Cleared item from inventory grid at {topLeft}");
                    foundItem = true;
                }
            }
        }
    }

    /// <summary>
    /// Tween movement coroutine
    /// </summary>
    private IEnumerator MoveToSlotRoutine(RectTransform rectTransform, Vector2 destination, float duration, System.Action onComplete = null)
    {
        if (rectTransform == null)
            yield break;

        if (duration > 0.0f)
        {
            float startTime = Time.time;
            Vector2 startPos = rectTransform.anchoredPosition;
            float tweenCoeff = 1.0f / duration;

            float dt = 0.0f;
            while (dt < 1.0f && rectTransform != null)
            {
                dt = (Time.time - startTime) * tweenCoeff;
                float t = EaseOutBack(dt);

                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = Vector2.Lerp(startPos, destination, t);
                }

                yield return null;
            }
        }

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = destination;
        }

        onComplete?.Invoke();
    }

    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    /// <summary>
    /// Merge button handler
    /// </summary>
    private void OnMergeButtonClicked()
    {
        if (slot1.slottedItemSO == null || slot2.slottedItemSO == null)
        {
            Debug.Log("[EngineerManager] Cannot merge: slots not filled");
            return;
        }

        if (slot1.slottedItemSO.itemName != slot2.slottedItemSO.itemName)
        {
            Debug.Log("[EngineerManager] Cannot merge: items don't match");
            return;
        }

        Debug.Log($"=== MERGE ATTEMPTED ===");
        Debug.Log($"Item 1: {slot1.slottedItemSO.itemName}");
        Debug.Log($"Item 2: {slot2.slottedItemSO.itemName}");
        Debug.Log($"IsMergeValid = true");
        Debug.Log($"[TODO] Actual merge logic goes here");

        if (feedbackText != null)
            feedbackText.text = $"Merged {slot1.slottedItemSO.itemName}!";

        // Destroy the slotted items
        if (slot1.slottedItemObject != null)
            Destroy(slot1.slottedItemObject);
            
        if (slot2.slottedItemObject != null)
            Destroy(slot2.slottedItemObject);

        ClearSlots();
        
        // Close UI after short delay
        StartCoroutine(CloseUIAfterDelay(1.5f));
    }

    /// <summary>
    /// Decline button handler
    /// </summary>
    private void OnDeclineButtonClicked()
    {
        Debug.Log("=== ENGINEER MERGE DECLINED ===");
        
        // Return all slotted items to inventory
        if (slot1?.slottedItemObject != null)
        {
            Item itemScript = slot1.slottedItemObject.GetComponent<Item>();
            SnapBackToInventory(slot1.slottedItemObject, itemScript);
        }

        if (slot2?.slottedItemObject != null)
        {
            Item itemScript = slot2.slottedItemObject.GetComponent<Item>();
            SnapBackToInventory(slot2.slottedItemObject, itemScript);
        }

        StartCoroutine(CloseUIAfterDelay(0.5f));
    }

    private IEnumerator CloseUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseEngineerUI();
    }

    /// <summary>
    /// Clears both slots
    /// </summary>
    private void ClearSlots()
    {
        if (slot1 != null)
        {
            slot1.slottedItemSO = null;
            slot1.slottedItemObject = null;
        }

        if (slot2 != null)
        {
            slot2.slottedItemSO = null;
            slot2.slottedItemObject = null;
        }

        if (mergeButton != null)
            mergeButton.interactable = false;

        if (feedbackText != null)
            feedbackText.text = "";
    }

    public void CloseEngineerUI()
    {
        if (!IsEngineerUIActive)
            return;

        Debug.Log("[EngineerManager] Closing Engineer UI");

        // Return any remaining slotted items
        if (slot1?.slottedItemObject != null)
        {
            Destroy(slot1.slottedItemObject);
        }

        if (slot2?.slottedItemObject != null)
        {
            Destroy(slot2.slottedItemObject);
        }

        ClearSlots();

        engineerUIPanel.SetActive(false);
        IsEngineerUIActive = false;

        OnEngineerClosed?.Invoke();
    }

    private void OnDestroy()
    {
        if (mergeButton != null)
            mergeButton.onClick.RemoveListener(OnMergeButtonClicked);
            
        if (declineButton != null)
            declineButton.onClick.RemoveListener(OnDeclineButtonClicked);
    }
}