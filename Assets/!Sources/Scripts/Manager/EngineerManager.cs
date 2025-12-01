using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// EngineerManager - Player-managed merge system
/// Items must be manually returned to inventory by player
/// No auto-return functionality
/// </summary>
public class EngineerManager : MonoBehaviour
{
    public static EngineerManager Instance { get; private set; }
    public static event System.Action OnEngineerClosed;

    [System.Serializable]
    public class MergeSlot
    {
        [Header("UI References")]
        public RectTransform slotAnchor;
        public Image slotImage; // The child image to flash

        [Header("Runtime Data")]
        public ItemSO slottedItemSO;
        public GameObject slottedItemObject;
        public ItemDragManager dragManager;

        public void Clear()
        {
            slottedItemSO = null;
            slottedItemObject = null;
            dragManager = null;
        }

        public bool IsOccupied()
        {
            return slottedItemObject != null;
        }
    }

    [Header("Engineer UI")]
    [SerializeField] private UI_BaseEventPanel engineerUIPanel;

    [Header("Merge Slots")]
    [SerializeField] private MergeSlot slot1;
    [SerializeField] private MergeSlot slot2;
    [SerializeField] private MergeSlot slot3; // Result slot

    [Header("Buttons")]
    [SerializeField] private Button mergeButton;
    [SerializeField] private Button declineButton;

    [Header("Feedback")]
    [SerializeField] private TMP_Text feedbackText;

    [Header("Tween Settings")]
    [SerializeField] private float tweenDuration = 0.28f;

    [Header("Visual Feedback")]
    [SerializeField] private Color invalidMergeColor = new Color(1f, 0.3f, 0.3f, 0.5f); // Red tint
    [SerializeField] private float invalidMergeFlashDuration = 0.3f;

    private InventoryGridScript inventoryGrid;
    private bool isProcessing = false;

    public bool IsEngineerUIActive { get; private set; }
    public bool IsCooldownActive { get; private set; } = false;

    [Header("Merge Tracking")]
    [SerializeField] private int successfulMergeCounter = 0;

    public int SuccessfulMergeCounter => successfulMergeCounter;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (engineerUIPanel != null)
            engineerUIPanel.HideEventPanel();
    }

    private void Start()
    {
        inventoryGrid = FindFirstObjectByType<InventoryGridScript>();

        if (inventoryGrid == null)
        {
            Debug.LogError("[EngineerManager] InventoryGridScript not found!");
        }

        if (mergeButton != null)
        {
            mergeButton.onClick.RemoveAllListeners();
            mergeButton.onClick.AddListener(OnMergeButtonClicked);
            mergeButton.interactable = false;
        }

        if (declineButton != null)
        {
            declineButton.onClick.RemoveAllListeners();
            declineButton.onClick.AddListener(OnDeclineButtonClicked);
        }

        ClearAllSlots();
    }

    /// <summary>
    /// Opens Engineer UI and deletes all unequipped items
    /// </summary>
    public void OpenEngineerUI(GameObject player = null)
    {
        if (IsEngineerUIActive || IsCooldownActive)
        {
            Debug.Log("[EngineerManager] Ignored OpenEngineerUI - UI busy or cooldown active");
            return;
        }

        // Step 1: Delete all unequipped items BEFORE opening
        DeleteUnequippedItems();

        // Step 2: Clear all slot data
        ForceCleanAllSlots();
        
        // Step 3: Show panel
        isProcessing = false;
        engineerUIPanel.ShowEventPanel();
        IsEngineerUIActive = true;

        if (feedbackText != null)
            feedbackText.text = "Drag two identical items to merge";

        Debug.Log("[EngineerManager] Engineer UI opened - unequipped items deleted");
    }

    /// <summary>
    /// Called when item drag starts - adds canvas for proper rendering
    /// </summary>
    public void OnItemDragStarted(GameObject item)
    {
        if (!IsEngineerUIActive) return;

        Canvas itemCanvas = item.GetComponent<Canvas>();

        if (itemCanvas == null)
        {
            itemCanvas = item.AddComponent<Canvas>();
            itemCanvas.overrideSorting = true;
            itemCanvas.sortingOrder = 1000;

            if (item.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            {
                item.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
        }
        else
        {
            itemCanvas.overrideSorting = true;
            itemCanvas.sortingOrder = 1000;
        }
    }

    /// <summary>
    /// Called when item is released - handles slotting logic
    /// </summary>
    public void OnItemReleased(GameObject item)
    {
        if (!IsEngineerUIActive || isProcessing) return;

        Item itemScript = item.GetComponent<Item>();
        ItemDragManager dragManager = item.GetComponent<ItemDragManager>();
        
        if (itemScript == null || itemScript.itemData == null || dragManager == null)
        {
            Debug.LogWarning("[EngineerManager] Invalid item released");
            return;
        }

        RectTransform itemRect = item.GetComponent<RectTransform>();

        // Convert position to engineer panel local space
        Vector2 localPointInPanel;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            engineerUIPanel.GetComponent<RectTransform>(),
            RectTransformUtility.WorldToScreenPoint(null, itemRect.position),
            null,
            out localPointInPanel
        );

        // Check which slot (if any) the item is near
        MergeSlot targetSlot = GetSlotAtPosition(localPointInPanel);

        if (targetSlot != null)
        {
            // Item is near a slot
            bool wasInSlot1 = slot1.IsOccupied() && slot1.slottedItemObject == item;
            bool wasInSlot2 = slot2.IsOccupied() && slot2.slottedItemObject == item;
            bool wasInSlot3 = slot3.IsOccupied() && slot3.slottedItemObject == item;

            if (wasInSlot1 || wasInSlot2 || wasInSlot3)
            {
                // Item is being moved between slots
                MergeSlot sourceSlot = wasInSlot1 ? slot1 : (wasInSlot2 ? slot2 : slot3);

                if (targetSlot == sourceSlot)
                {
                    // Dropped back in same slot - snap to center
                    StartCoroutine(MoveToSlotRoutine(itemRect, targetSlot.slotAnchor.anchoredPosition, tweenDuration));
                    return;
                }
                else
                {
                    // Trying to move to different slot - not allowed
                    if (feedbackText != null)
                        feedbackText.text = "Cannot move between slots! Drag back to inventory.";
                    
                    // Snap back to original slot
                    StartCoroutine(MoveToSlotRoutine(itemRect, sourceSlot.slotAnchor.anchoredPosition, tweenDuration));
                    return;
                }
            }
            else
            {
                // New item from inventory - try to slot it
                if (TrySlotItem(targetSlot, item, itemScript, dragManager))
                {
                    Debug.Log($"[EngineerManager] Item {itemScript.itemData.itemName} slotted into {GetSlotName(targetSlot)}");
                    CheckMergeValidity();
                }
            }
        }
        else
        {
            // Item is NOT near any slot - check if being returned to inventory
            bool wasInSlot1 = slot1.IsOccupied() && slot1.slottedItemObject == item;
            bool wasInSlot2 = slot2.IsOccupied() && slot2.slottedItemObject == item;
            bool wasInSlot3 = slot3.IsOccupied() && slot3.slottedItemObject == item;

            if (wasInSlot1 || wasInSlot2 || wasInSlot3)
            {
                // Item is being dragged from a slot to inventory
                MergeSlot sourceSlot = wasInSlot1 ? slot1 : (wasInSlot2 ? slot2 : slot3);
                
                Debug.Log($"[EngineerManager] Item being dragged from {GetSlotName(sourceSlot)} to inventory");
                
                // Step 1: Remove temporary canvas first
                RemoveTemporaryCanvas(item);
                
                // Step 2: Store current world position before reparenting
                Vector3 currentWorldPos = itemRect.position;
                
                // Step 3: Reparent to inventory canvas FIRST (critical for AttachToInventory to work)
                itemRect.SetParent(inventoryGrid.inventoryRect, false);
                
                // Step 4: Convert world position to inventory local space
                Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, currentWorldPos);
                Vector2 localPosInInventory;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    inventoryGrid.inventoryRect,
                    screenPos,
                    null,
                    out localPosInInventory
                );
                
                // Step 5: Set the anchored position
                itemRect.anchoredPosition = localPosInInventory;
                
                // Step 6: Reset transform properties
                itemRect.localScale = Vector3.one;
                itemRect.localRotation = Quaternion.identity;
                
                // Step 7: NOW call AttachToInventory - item is already in correct parent
                bool attachedSuccessfully = dragManager.AttachToInventory();
                
                if (attachedSuccessfully)
                {
                    // Clear the slot after successful attachment
                    sourceSlot.Clear();
                    
                    // Recheck validity after removing item
                    CheckMergeValidity();
                    
                    if (feedbackText != null)
                        feedbackText.text = "Item returned to inventory";
                    
                    Debug.Log($"[EngineerManager] Item successfully attached to inventory from {GetSlotName(sourceSlot)}");
                }
                else
                {
                    // Failed to attach - snap back to slot
                    Debug.LogWarning($"[EngineerManager] Failed to attach to inventory, snapping back to {GetSlotName(sourceSlot)}");
                    
                    // Reparent back to engineer panel
                    itemRect.SetParent(engineerUIPanel.transform, true);
                    
                    // Snap back to slot
                    StartCoroutine(MoveToSlotRoutine(itemRect, sourceSlot.slotAnchor.anchoredPosition, tweenDuration));
                    
                    if (feedbackText != null)
                        feedbackText.text = "No space in inventory!";
                }
            }
            else
            {
                // Item from inventory that wasn't slotted - just cleanup canvas
                RemoveTemporaryCanvas(item);
            }
        }
    }

    /// <summary>
    /// Try to slot an item - validates slot availability
    /// </summary>
    private bool TrySlotItem(MergeSlot slot, GameObject itemObject, Item itemScript, ItemDragManager dragManager)
    {
        // Don't allow slotting into slot3 (results only)
        if (slot == slot3)
        {
            if (feedbackText != null)
                feedbackText.text = "Slot 3 is for merged results only!";
            return false;
        }

        // Check if slot is occupied
        if (slot.IsOccupied())
        {
            if (feedbackText != null)
                feedbackText.text = "Slot already occupied!";
            return false;
        }

        RectTransform itemRect = itemObject.GetComponent<RectTransform>();

        // Clear from inventory grid if it was equipped
        if (itemScript.state == Item.itemState.equipped && inventoryGrid != null)
        {
            inventoryGrid.MarkCells(
                Vector2Int.FloorToInt(dragManager.TopLeftCellPos), 
                itemScript.itemShape, 
                null
            );
        }

        // Reparent to engineer panel
        itemRect.SetParent(engineerUIPanel.transform, true);

        // Animate to slot
        StartCoroutine(MoveToSlotRoutine(itemRect, slot.slotAnchor.anchoredPosition, tweenDuration));

        // Store slot data
        slot.slottedItemSO = itemScript.itemData;
        slot.slottedItemObject = itemObject;
        slot.dragManager = dragManager;

        // Update item state
        itemScript.state = Item.itemState.unequipped;

        Debug.Log($"[EngineerManager] Slotted {itemScript.itemData.itemName} (level {itemScript.level})");

        return true;
    }

    /// <summary>
    /// Gets the slot at a given position (or null)
    /// </summary>
    private MergeSlot GetSlotAtPosition(Vector2 position)
    {
        if (slot1?.slotAnchor != null && IsPositionInSlot(position, slot1.slotAnchor))
            return slot1;

        if (slot2?.slotAnchor != null && IsPositionInSlot(position, slot2.slotAnchor))
            return slot2;

        if (slot3?.slotAnchor != null && IsPositionInSlot(position, slot3.slotAnchor))
            return slot3;

        return null;
    }

    private bool IsPositionInSlot(Vector2 position, RectTransform slotRect)
    {
        float threshold = 80f;
        float distance = Vector2.Distance(position, slotRect.anchoredPosition);
        return distance < threshold;
    }

    /// <summary>
    /// Check if merge is valid - compares name AND level
    /// NEW: No auto-return on mismatch - just displays feedback with visual flash
    /// </summary>
    private void CheckMergeValidity()
    {
        // If either input slot is empty, can't merge
        if (!slot1.IsOccupied() || !slot2.IsOccupied())
        {
            if (mergeButton != null) mergeButton.interactable = false;
            
            if (feedbackText != null)
            {
                if (!slot1.IsOccupied() && !slot2.IsOccupied())
                    feedbackText.text = "Place two identical items to merge";
                else
                    feedbackText.text = "Need one more item to merge";
            }
            return;
        }

        Item item1 = slot1.slottedItemObject.GetComponent<Item>();
        Item item2 = slot2.slottedItemObject.GetComponent<Item>();

        // Check if names match AND levels match
        bool nameMatches = slot1.slottedItemSO.itemName == slot2.slottedItemSO.itemName;
        bool levelMatches = item1.level == item2.level;

        if (nameMatches && levelMatches)
        {
            // Valid merge - enable button
            Debug.Log($"[EngineerManager] Valid merge: {slot1.slottedItemSO.itemName} (level {item1.level})");
            
            if (feedbackText != null) 
                feedbackText.text = $"Ready to merge: {slot1.slottedItemSO.itemName} (Lvl {item1.level})";
            
            if (mergeButton != null) 
                mergeButton.interactable = true;
        }
        else
        {
            // Invalid merge - disable button, show feedback, and flash red
            if (mergeButton != null) 
                mergeButton.interactable = false;

            // Show visual feedback on slot anchors
            StartCoroutine(FlashInvalidMerge());

            if (!nameMatches && !levelMatches)
            {
                if (feedbackText != null)
                    feedbackText.text = "Items don't match! Drag them back to inventory.";
            }
            else if (!nameMatches)
            {
                if (feedbackText != null)
                    feedbackText.text = "Different item types! Drag them back to inventory.";
            }
            else // !levelMatches
            {
                if (feedbackText != null)
                    feedbackText.text = $"Level mismatch! (Lvl {item1.level} vs Lvl {item2.level}) Drag them back.";
            }

            Debug.Log($"[EngineerManager] Invalid merge - player must manually return items");
        }
    }

    /// <summary>
    /// Flash red tint on slot images to show invalid merge
    /// </summary>
    private IEnumerator FlashInvalidMerge()
    {
        Image slot1Image = slot1.slotImage;
        Image slot2Image = slot2.slotImage;

        if (slot1Image == null || slot2Image == null) 
        {
            Debug.LogWarning("[EngineerManager] Slot images not assigned!");
            yield break;
        }

        Color originalColor1 = slot1Image.color;
        Color originalColor2 = slot2Image.color;

        // Flash to red with pulse effect
        float elapsed = 0f;
        while (elapsed < invalidMergeFlashDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed * 4f, 1f); // Pulse effect
            
            slot1Image.color = Color.Lerp(originalColor1, invalidMergeColor, t);
            slot2Image.color = Color.Lerp(originalColor2, invalidMergeColor, t);
            
            yield return null;
        }

        // Restore original colors
        slot1Image.color = originalColor1;
        slot2Image.color = originalColor2;

        // Play error sound
        SoundManager.Instance?.PlaySFX("SFX_Component_OnError");
    }

    /// <summary>
    /// Perform merge and move result to slot3
    /// </summary>
    private IEnumerator CompleteMergeAndReset()
    {
        if (!slot1.IsOccupied() || !slot2.IsOccupied())
        {
            Debug.LogError("[EngineerManager] Merge failed - slots empty!");
            isProcessing = false;
            yield break;
        }

        GameObject mergedItem = slot1.slottedItemObject;
        GameObject itemToDestroy = slot2.slottedItemObject;

        // Perform the merge
        Item mergedItemScript = mergedItem.GetComponent<Item>();
        mergedItemScript.level++;
        
        Destroy(itemToDestroy);
        successfulMergeCounter++;

        Debug.Log($"<color=teal>[EngineerManager] Merge complete! New level: {mergedItemScript.level}</color>");

        // Clear input slots IMMEDIATELY
        slot1.Clear();
        slot2.Clear();

        // Move merged item to slot3
        if (slot3 != null && slot3.slotAnchor != null)
        {
            RectTransform mergedRect = mergedItem.GetComponent<RectTransform>();

            yield return MoveToSlotRoutine(mergedRect, slot3.slotAnchor.anchoredPosition, tweenDuration);

            // Store in slot3
            slot3.slottedItemSO = mergedItemScript.itemData;
            slot3.slottedItemObject = mergedItem;
            slot3.dragManager = mergedItem.GetComponent<ItemDragManager>();

            Debug.Log($"[EngineerManager] Merged item in Slot 3 - player can drag to inventory or merge more");
        }

        yield return new WaitForSeconds(0.1f);

        // Reset for next merge attempt
        isProcessing = false;
        
        if (feedbackText != null)
            feedbackText.text = "Merge complete! Drag result to inventory or merge more items.";
        
        if (mergeButton != null)
            mergeButton.interactable = false;
    }

    /// <summary>
    /// Animation routine for moving items to slots
    /// </summary>
    private IEnumerator MoveToSlotRoutine(RectTransform rectTransform, Vector2 destination, float duration)
    {
        if (rectTransform == null) yield break;

        float elapsed = 0f;
        Vector2 startPos = rectTransform.anchoredPosition;

        while (elapsed < duration && rectTransform != null)
        {
            elapsed += Time.deltaTime;
            float t = EaseOutBack(Mathf.Clamp01(elapsed / duration));
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, destination, t);
            yield return null;
        }

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = destination;
        }
    }

    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    /// <summary>
    /// Merge button clicked - validates and performs merge
    /// </summary>
    private void OnMergeButtonClicked()
    {
        if (isProcessing) return;
        
        // Final validation
        if (!slot1.IsOccupied() || !slot2.IsOccupied())
        {
            Debug.LogWarning("[EngineerManager] Merge clicked but slots are empty!");
            return;
        }

        Item item1 = slot1.slottedItemObject.GetComponent<Item>();
        Item item2 = slot2.slottedItemObject.GetComponent<Item>();

        bool nameMatches = slot1.slottedItemSO.itemName == slot2.slottedItemSO.itemName;
        bool levelMatches = item1.level == item2.level;

        if (!nameMatches || !levelMatches)
        {
            Debug.LogWarning("[EngineerManager] Merge validation failed!");
            return;
        }

        isProcessing = true;

        if (feedbackText != null)
            feedbackText.text = $"Merging {slot1.slottedItemSO.itemName}...";

        SoundManager.Instance?.PlaySFX("SFX_Component_OnMerge");

        StartCoroutine(CompleteMergeAndReset());
    }

    /// <summary>
    /// Decline button clicked - closes engineer UI
    /// Player must manually move all items back to inventory
    /// </summary>
    private void OnDeclineButtonClicked()
    {
        if (isProcessing) return;

        Debug.Log("[EngineerManager] Decline clicked - player must return items manually");

        if (feedbackText != null)
            feedbackText.text = "Closing engineer...";

        CloseEngineerUI();
    }

    /// <summary>
    /// Clear all slot data
    /// </summary>
    private void ClearAllSlots()
    {
        slot1?.Clear();
        slot2?.Clear();
        slot3?.Clear();

        if (mergeButton != null)
            mergeButton.interactable = false;

        if (feedbackText != null && !isProcessing)
            feedbackText.text = "Drag two identical items to merge";
    }

    /// <summary>
    /// Force clean all slots - does NOT return items
    /// </summary>
    private void ForceCleanAllSlots()
    {
        ClearAllSlots();
        Debug.Log("[EngineerManager] Force cleaned all slots - items remain where they are");
    }

    /// <summary>
    /// Close the engineer UI
    /// </summary>
    public void CloseEngineerUI()
    {
        if (!IsEngineerUIActive) return;

        Debug.Log("[EngineerManager] Closing Engineer UI");

        ClearAllSlots();
        isProcessing = false;
        engineerUIPanel.HideEventPanel(() => OnEngineerClosed?.Invoke());
        SoundManager.Instance?.PlaySFX("SFX_ButtonOnCancel");
        IsEngineerUIActive = false;
        
        StartCoroutine(CloseCooldown());
    }

    private IEnumerator CloseCooldown()
    {
        IsCooldownActive = true;
        yield return new WaitForSeconds(0.5f);
        IsCooldownActive = false;
    }

    private string GetSlotName(MergeSlot slot)
    {
        if (slot == slot1) return "Slot 1";
        if (slot == slot2) return "Slot 2";
        if (slot == slot3) return "Slot 3 (Result)";
        return "Unknown";
    }

    /// <summary>
    /// Removes the temporary canvas added during dragging
    /// </summary>
    private void RemoveTemporaryCanvas(GameObject itemObject)
    {
        Canvas itemCanvas = itemObject.GetComponent<Canvas>();
        if (itemCanvas != null && itemCanvas.overrideSorting && itemCanvas.sortingOrder == 1000)
        {
            UnityEngine.UI.GraphicRaycaster raycaster = itemObject.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (raycaster != null) Destroy(raycaster);
            Destroy(itemCanvas);
        }
    }

    private void OnDestroy()
    {
        if (mergeButton != null)
            mergeButton.onClick.RemoveListener(OnMergeButtonClicked);

        if (declineButton != null)
            declineButton.onClick.RemoveListener(OnDeclineButtonClicked);
    }

    /// <summary>
    /// Deletes all items that are not equipped in the inventory
    /// Called when engineer UI opens
    /// </summary>
    private void DeleteUnequippedItems()
    {
        if (inventoryGrid == null)
        {
            Debug.LogWarning("[EngineerManager] Cannot delete unequipped items - inventory reference missing");
            return;
        }

        Item[] items = FindObjectsByType<Item>(FindObjectsSortMode.None);
        int deletedCount = 0;

        foreach (Item item in items)
        {
            if (item != null && item.state != Item.itemState.equipped)
            {
                Debug.Log($"[EngineerManager] Deleting unequipped item: {item.itemData?.itemName ?? "Unknown"}");
                item.PrepareDeletion();
                deletedCount++;
            }
        }

        Debug.Log($"<color=orange>[EngineerManager] Deleted {deletedCount} unequipped item(s)</color>");
    }
}