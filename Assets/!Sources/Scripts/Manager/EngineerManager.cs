using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// EngineerManager - Bulletproof merge system with proper state management
/// Fixed all parenting, state clearing, and merge validation bugs
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

    public void OpenEngineerUI(GameObject player = null)
    {
        if (IsEngineerUIActive || IsCooldownActive)
        {
            Debug.Log("[EngineerManager] Ignored OpenEngineerUI - UI busy or cooldown active");
            return;
        }

        // Delete all unequipped items BEFORE opening
        DeleteUnequippedItems();

        // CRITICAL: Completely clear all slots to prevent data retention
        ForceCleanAllSlots();
        
        isProcessing = false;
        engineerUIPanel.ShowEventPanel();
        IsEngineerUIActive = true;

        if (feedbackText != null)
            feedbackText.text = "Drag two identical items to merge";

        Debug.Log("[EngineerManager] Engineer UI opened fresh - unequipped items deleted");
    }

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

        // Check if item was being dragged FROM a slot (to return it to inventory)
        bool wasInSlot1 = slot1.IsOccupied() && slot1.slottedItemObject == item;
        bool wasInSlot2 = slot2.IsOccupied() && slot2.slottedItemObject == item;
        bool wasInSlot3 = slot3.IsOccupied() && slot3.slottedItemObject == item;

        if (wasInSlot1 || wasInSlot2 || wasInSlot3)
        {
            // Item being dragged from a slot - check if returning to inventory
            MergeSlot sourceSlot = wasInSlot1 ? slot1 : (wasInSlot2 ? slot2 : slot3);
            
            // If not near any slot, return to inventory
            MergeSlot targetSlot = GetSlotAtPosition(localPointInPanel);
            if (targetSlot == null || targetSlot == sourceSlot)
            {
                Debug.Log($"[EngineerManager] Returning item from {GetSlotName(sourceSlot)} to inventory");
                ReturnSingleItemToInventory(sourceSlot);
                CheckMergeValidity();
                return;
            }
            else
            {
                // Trying to move to different slot - not allowed
                if (feedbackText != null)
                    feedbackText.text = "Cannot move between slots!";
                
                // Snap back to original slot
                StartCoroutine(MoveToSlotRoutine(itemRect, sourceSlot.slotAnchor.anchoredPosition, tweenDuration));
                return;
            }
        }

        // Item is being dragged from inventory - try to slot it
        MergeSlot targetNewSlot = GetSlotAtPosition(localPointInPanel);

        if (targetNewSlot != null)
        {
            if (TrySlotItem(targetNewSlot, item, itemScript, dragManager))
            {
                Debug.Log($"[EngineerManager] Item {itemScript.itemData.itemName} slotted into {GetSlotName(targetNewSlot)}");
                CheckMergeValidity();
            }
            else
            {
                ForceReturnToInventory(item, dragManager);
            }
        }
        else
        {
            // Not near any slot - return to inventory
            ForceReturnToInventory(item, dragManager);
        }
    }

    /// <summary>
    /// Try to slot an item - with proper validation
    /// </summary>
    private bool TrySlotItem(MergeSlot slot, GameObject itemObject, Item itemScript, ItemDragManager dragManager)
    {
        // Don't allow slotting into slot3
        if (slot == slot3)
        {
            if (feedbackText != null)
                feedbackText.text = "Slot 3 is for results only!";
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

        // Clear from inventory grid
        if (inventoryGrid != null)
        {
            inventoryGrid.MarkCells(
                Vector2Int.FloorToInt(dragManager.TopLeftCellPos), 
                itemScript.itemShape, 
                null
            );
        }

        // CRITICAL: Properly reparent to engineer panel
        itemRect.SetParent(engineerUIPanel.transform, true); // worldPositionStays = true

        // Animate to slot
        StartCoroutine(MoveToSlotRoutine(itemRect, slot.slotAnchor.anchoredPosition, tweenDuration));

        // Store slot data
        slot.slottedItemSO = itemScript.itemData;
        slot.slottedItemObject = itemObject;
        slot.dragManager = dragManager;

        Debug.Log($"[EngineerManager] Slotted {itemScript.itemData.itemName} (level {itemScript.level})");

        return true;
    }

    private MergeSlot GetSlotAtPosition(Vector2 position)
    {
        if (slot1?.slotAnchor != null && IsPositionInSlot(position, slot1.slotAnchor))
            return slot1;

        if (slot2?.slotAnchor != null && IsPositionInSlot(position, slot2.slotAnchor))
            return slot2;

        return null;
    }

    private bool IsPositionInSlot(Vector2 position, RectTransform slotRect)
    {
        float threshold = 80f; // Increased for better detection
        float distance = Vector2.Distance(position, slotRect.anchoredPosition);
        return distance < threshold;
    }

    /// <summary>
    /// Check if merge is valid - compares both name AND level
    /// </summary>
    private void CheckMergeValidity()
    {
        // If either slot is empty, can't merge
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
            // Valid merge
            Debug.Log($"[EngineerManager] Valid merge: {slot1.slottedItemSO.itemName} (level {item1.level})");
            
            if (feedbackText != null) 
                feedbackText.text = $"Ready to merge: {slot1.slottedItemSO.itemName} (Lvl {item1.level})";
            
            if (mergeButton != null) 
                mergeButton.interactable = true;
        }
        else
        {
            // Invalid merge
            if (mergeButton != null) 
                mergeButton.interactable = false;

            if (!nameMatches && !levelMatches)
            {
                if (feedbackText != null)
                    feedbackText.text = "Items don't match! (name and level differ)";
            }
            else if (!nameMatches)
            {
                if (feedbackText != null)
                    feedbackText.text = "Items don't match! (different types)";
            }
            else // !levelMatches
            {
                if (feedbackText != null)
                    feedbackText.text = $"Items don't match! (level {item1.level} vs {item2.level})";
            }

            Debug.Log($"[EngineerManager] Invalid merge - returning slot2 item");
            StartCoroutine(ReturnSlotItemDelayed(slot2, 0.45f));
        }
    }

    /// <summary>
    /// Force return item to inventory using drag manager
    /// </summary>
private void ForceReturnToInventory(GameObject item, ItemDragManager dragManager)
{
    if (item == null || dragManager == null) return;

    Debug.Log($"[EngineerManager] Force returning item to inventory");

    RemoveTemporaryCanvas(item);
    
    // Check if item has ever been in inventory
    Item itemScript = item.GetComponent<Item>();
    if (itemScript != null && itemScript.state == Item.itemState.equipped)
    {
        // Item was previously in inventory - use normal attachment
        dragManager.AttachToInventory();
    }
    else
    {
        // Item has NEVER been in inventory - find a spot for it
        InventoryGridScript inventory = FindFirstObjectByType<InventoryGridScript>();
        if (inventory != null)
        {
            // Use AddItem to properly place it
            inventory.AddItem(inventory.ItemSpawnPrefab, itemScript.itemData);
            // Destroy the current orphaned instance
            Destroy(item);
        }
    }

    if (feedbackText != null)
        feedbackText.text = "Item returned to inventory";
}

    /// <summary>
    /// Return a single item from a slot to inventory
    /// </summary>
    private void ReturnSingleItemToInventory(MergeSlot slot)
    {
        if (!slot.IsOccupied()) return;

        GameObject item = slot.slottedItemObject;
        ItemDragManager dragManager = slot.dragManager;

        // Clear slot FIRST
        slot.Clear();

        // Return item
        if (dragManager != null)
        {
            StartCoroutine(RestoreItemWithAnimation(item, dragManager));
        }
        else
        {
            ForceReturnToInventory(item, item.GetComponent<ItemDragManager>());
        }
    }

    /// <summary>
    /// Return slotted item after delay (for mismatches)
    /// </summary>
    private IEnumerator ReturnSlotItemDelayed(MergeSlot slot, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (slot.IsOccupied())
        {
            ReturnSingleItemToInventory(slot);
        }
    }

    /// <summary>
    /// Perform merge and move to slot3
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
        ItemDragManager mergedDragManager = slot1.dragManager;

        // Perform the actual merge (level up item1, destroy item2)
        Item mergedItemScript = mergedItem.GetComponent<Item>();
        mergedItemScript.level++;
        
        Destroy(itemToDestroy);
        successfulMergeCounter++;

        Debug.Log($"<color=teal>[EngineerManager] Merge complete! New level: {mergedItemScript.level}</color>");

        // CRITICAL: Clear slots IMMEDIATELY after destruction
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
            slot3.dragManager = mergedDragManager;

            Debug.Log($"[EngineerManager] Merged item in Slot 3, ready to drag out or merge more");
        }

        yield return new WaitForSeconds(0.1f);

        // Reset for next merge
        isProcessing = false;
        
        if (feedbackText != null)
            feedbackText.text = "Merge complete! Drag more items or take result.";
        
        if (mergeButton != null)
            mergeButton.interactable = false;
    }

    /// <summary>
    /// Return all items when declining
    /// </summary>
    private IEnumerator ReturnAllItemsAndClose()
    {
        bool hasItems = false;

        if (slot1.IsOccupied())
        {
            hasItems = true;
            ReturnSingleItemToInventory(slot1);
        }

        if (slot2.IsOccupied())
        {
            hasItems = true;
            ReturnSingleItemToInventory(slot2);
        }

        if (slot3.IsOccupied())
        {
            hasItems = true;
            ReturnSingleItemToInventory(slot3);
        }

        if (hasItems)
        {
            yield return new WaitForSeconds(tweenDuration + 0.15f);
        }

        CloseEngineerUI();
    }

    /// <summary>
    /// Restore item with smooth animation
    /// </summary>
    private IEnumerator RestoreItemWithAnimation(GameObject itemObject, ItemDragManager dragManager)
    {
        if (itemObject == null || dragManager == null) 
        {
            Debug.LogError("[EngineerManager] RestoreItemWithAnimation: null reference");
            yield break;
        }

        Item itemScript = itemObject.GetComponent<Item>();
        RectTransform itemRect = itemObject.GetComponent<RectTransform>();

        if (inventoryGrid == null || inventoryGrid.inventoryRect == null)
        {
            Debug.LogError("[EngineerManager] Inventory reference missing");
            yield break;
        }

        // Remove temporary canvas
        RemoveTemporaryCanvas(itemObject);

        // Get current position in inventory space
        Vector2 startAnchored;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            inventoryGrid.inventoryRect,
            RectTransformUtility.WorldToScreenPoint(null, itemRect.position),
            null,
            out startAnchored
        );

        // Reparent to inventory
        itemRect.SetParent(inventoryGrid.inventoryRect, false);
        itemRect.localScale = Vector3.one;
        itemRect.localRotation = Quaternion.identity;
        itemRect.anchoredPosition = startAnchored;

        // Get target position
        Vector2 targetPos = dragManager.EquippedPos;
        Vector2Int topLeftCell = Vector2Int.FloorToInt(dragManager.TopLeftCellPos);

        // Tween to position
        float elapsed = 0f;
        while (elapsed < tweenDuration)
        {
            elapsed += Time.deltaTime;
            float t = EaseOutBack(Mathf.Clamp01(elapsed / tweenDuration));
            itemRect.anchoredPosition = Vector2.Lerp(startAnchored, targetPos, t);
            yield return null;
        }
        itemRect.anchoredPosition = targetPos;

        // Re-mark cells
        if (inventoryGrid != null)
        {
            inventoryGrid.MarkCells(topLeftCell, itemScript.itemShape, itemObject);
            itemScript.state = Item.itemState.equipped;
            itemScript.TriggerEffectEquip();
        }

        Debug.Log($"[EngineerManager] Restored {itemScript.itemData.itemName} to inventory");
    }

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

    private void OnMergeButtonClicked()
    {
        if (isProcessing) return;
        
        // Validate merge one more time
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

        StartCoroutine(CompleteMergeAndReset());
    }

    private void OnDeclineButtonClicked()
    {
        if (isProcessing) return;

        isProcessing = true;

        if (feedbackText != null)
            feedbackText.text = "Returning all items...";

        StartCoroutine(ReturnAllItemsAndClose());
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
    /// FORCE clean - destroys any lingering items
    /// </summary>
    private void ForceCleanAllSlots()
    {
        // Return any items still in slots
        if (slot1.IsOccupied()) ReturnSingleItemToInventory(slot1);
        if (slot2.IsOccupied()) ReturnSingleItemToInventory(slot2);
        if (slot3.IsOccupied()) ReturnSingleItemToInventory(slot3);

        ClearAllSlots();
        
        Debug.Log("[EngineerManager] Force cleaned all slots");
    }

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
            // Check if item is NOT in the equipped items list
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