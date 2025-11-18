using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// EngineerManager - Simplified version using ItemDragManager's equippedPos
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
        
        // Store reference to drag manager instead of duplicating position data
        public ItemDragManager dragManager;
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
    [SerializeField] private float tweenDuration = 0.28f;

    private InventoryGridScript inventoryGrid;
    private bool isProcessing = false;

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

        ClearSlots();
    }

    public void OpenEngineerUI(GameObject player = null)
    {
        if (IsEngineerUIActive)
        {
            Debug.Log("[EngineerManager] Engineer UI already open");
            return;
        }

        ClearSlots();
        isProcessing = false;
        engineerUIPanel.SetActive(true);
        IsEngineerUIActive = true;

        if (feedbackText != null)
            feedbackText.text = "Drag two identical items to merge";

        Debug.Log("[EngineerManager] Engineer UI opened");
    }

    public void OnItemDragStarted(GameObject item)
    {
        if (!IsEngineerUIActive)
            return;

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
        if (!IsEngineerUIActive || isProcessing)
            return;

        Item itemScript = item.GetComponent<Item>();
        if (itemScript == null || itemScript.itemData == null)
            return;

        RectTransform itemRect = item.GetComponent<RectTransform>();

        // Convert world position to engineer panel's local space
        Vector3 worldPos = itemRect.position;
        Vector2 localPointInPanel;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            engineerUIPanel.GetComponent<RectTransform>(),
            RectTransformUtility.WorldToScreenPoint(null, worldPos),
            null,
            out localPointInPanel
        );

        MergeSlot targetSlot = GetSlotAtPosition(localPointInPanel);

        if (targetSlot != null)
        {
            if (TrySlotItem(targetSlot, item, itemScript))
            {
                Debug.Log($"✓ Item {itemScript.itemData.itemName} slotted into {GetSlotName(targetSlot)}");
                CheckMergeValidity();
            }
            else
            {
                SnapBackToInventory(item);
            }
        }
        else
        {
            SnapBackToInventory(item);
        }
    }

    /// <summary>
    /// Simplified slotting - just store the drag manager reference
    /// The drag manager already knows its equipped position!
    /// </summary>
    private bool TrySlotItem(MergeSlot slot, GameObject itemObject, Item itemScript)
    {
        if (slot.slottedItemObject != null)
        {
            if (feedbackText != null)
                feedbackText.text = "Slot already occupied!";
            return false;
        }

        ItemDragManager dragManager = itemObject.GetComponent<ItemDragManager>();
        if (dragManager == null)
        {
            Debug.LogWarning($"ItemDragManager not found on {itemScript.itemData.itemName}!");
            return false;
        }

        RectTransform itemRect = itemObject.GetComponent<RectTransform>();

        // Clear from inventory grid (dragManager knows the topLeftCellPos)
        if (inventoryGrid != null)
        {
            inventoryGrid.MarkCells(Vector2Int.FloorToInt(dragManager.TopLeftCellPos), itemScript.itemShape, null);
        }

        // Store the world position before reparenting
        Vector3 worldPos = itemRect.position;
        
        // Reparent to engineer panel
        itemRect.SetParent(engineerUIPanel.transform);
        itemRect.position = worldPos; // Maintain visual position

        // Animate to slot anchor
        StartCoroutine(MoveToSlotRoutine(itemRect, slot.slotAnchor.anchoredPosition, tweenDuration));

        // Store slot data - just keep reference to drag manager
        slot.slottedItemSO = itemScript.itemData;
        slot.slottedItemObject = itemObject;
        slot.dragManager = dragManager; // Store the drag manager reference

        Debug.Log($"[EngineerManager] Slotted item. DragManager has equippedPos: {dragManager.EquippedPos}, topLeftCell: {dragManager.TopLeftCellPos}");

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
        float threshold = 50f;
        float distance = Vector2.Distance(position, slotRect.anchoredPosition);
        return distance < threshold;
    }

    private void CheckMergeValidity()
    {
        if (slot1.slottedItemSO == null || slot2.slottedItemSO == null)
        {
            if (mergeButton != null) mergeButton.interactable = false;
            if (feedbackText != null) feedbackText.text = "Place two identical items to merge";
            return;
        }

        bool isMergeValid = slot1.slottedItemSO.itemName == slot2.slottedItemSO.itemName;

        if (isMergeValid)
        {
            Debug.Log($"[EngineerManager] IsMergeValid = true (Items: {slot1.slottedItemSO.itemName})");
            if (feedbackText != null) feedbackText.text = $"Ready to merge: {slot1.slottedItemSO.itemName}";
            if (mergeButton != null) mergeButton.interactable = true;
        }
        else
        {
            Debug.Log($"[EngineerManager] IsMergeValid = false (Items don't match)");
            if (feedbackText != null) feedbackText.text = "Items don't match! Returning second item...";
            if (mergeButton != null) mergeButton.interactable = false;

            StartCoroutine(ReturnItemToInventoryDelayed(slot2, 0.45f));
        }
    }

    private void SnapBackToInventory(GameObject itemObject)
    {
        if (itemObject == null) return;

        Item itemScript = itemObject.GetComponent<Item>();
        if (itemScript == null) return;

        Debug.Log($"Snapping {itemScript.itemData.itemName} back to inventory");

        MergeSlot owner = null;
        if (slot1?.slottedItemObject == itemObject) owner = slot1;
        else if (slot2?.slottedItemObject == itemObject) owner = slot2;

        if (owner != null)
        {
            ItemDragManager dragManager = owner.dragManager;
            
            // Clear slot
            owner.slottedItemSO = null;
            owner.slottedItemObject = null;
            owner.dragManager = null;

            // Return using the drag manager's stored position
            StartCoroutine(RestoreItemCoroutine(itemObject, dragManager));
        }
        else
        {
            // Fallback: just call AttachToInventory
            ItemDragManager dm = itemObject.GetComponent<ItemDragManager>();
            if (dm != null) dm.AttachToInventory();
        }

        if (feedbackText != null) feedbackText.text = "Item returned to inventory";
    }

    private IEnumerator ReturnItemToInventoryDelayed(MergeSlot slot, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (slot?.slottedItemObject != null)
        {
            GameObject go = slot.slottedItemObject;
            ItemDragManager dragManager = slot.dragManager;

            // Clear slot immediately
            slot.slottedItemSO = null;
            slot.slottedItemObject = null;
            slot.dragManager = null;

            StartCoroutine(RestoreItemCoroutine(go, dragManager));
        }
    }

    private IEnumerator ReturnAllItemsAndClose()
    {
        GameObject itemA = slot1?.slottedItemObject;
        ItemDragManager dragA = slot1?.dragManager;

        GameObject itemB = slot2?.slottedItemObject;
        ItemDragManager dragB = slot2?.dragManager;

        if (itemA != null)
        {
            slot1.slottedItemSO = null;
            slot1.slottedItemObject = null;
            slot1.dragManager = null;
            StartCoroutine(RestoreItemCoroutine(itemA, dragA));
        }

        if (itemB != null)
        {
            slot2.slottedItemSO = null;
            slot2.slottedItemObject = null;
            slot2.dragManager = null;
            StartCoroutine(RestoreItemCoroutine(itemB, dragB));
        }

        yield return new WaitForSeconds(tweenDuration + 0.12f);

        CloseEngineerUI();
    }

    /// <summary>
    /// SIMPLIFIED restoration using equippedPos from ItemDragManager
    /// </summary>
    private IEnumerator RestoreItemCoroutine(GameObject itemObject, ItemDragManager dragManager)
    {
        if (itemObject == null || dragManager == null) 
        {
            Debug.LogError("RestoreItemCoroutine: null object or dragManager");
            yield break;
        }

        Item itemScript = itemObject.GetComponent<Item>();
        RectTransform itemRect = itemObject.GetComponent<RectTransform>();

        if (inventoryGrid == null || inventoryGrid.inventoryRect == null)
        {
            Debug.LogError("EngineerManager: inventoryGrid or inventoryRect missing");
            yield break;
        }

        // Remove temporary canvas components BEFORE reparenting
        Canvas itemCanvas = itemObject.GetComponent<Canvas>();
        if (itemCanvas != null && itemCanvas.overrideSorting && itemCanvas.sortingOrder == 1000)
        {
            UnityEngine.UI.GraphicRaycaster raycaster = itemObject.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (raycaster != null) Destroy(raycaster);
            Destroy(itemCanvas);
        }

        // Get current visual position in inventory space (for smooth tween start)
        Vector2 startAnchored;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            inventoryGrid.inventoryRect,
            RectTransformUtility.WorldToScreenPoint(null, itemRect.position),
            null,
            out startAnchored
        );

        // Reparent to inventory
        itemRect.SetParent(inventoryGrid.inventoryRect);
        itemRect.localScale = Vector3.one;
        itemRect.localRotation = Quaternion.identity;
        itemRect.anchoredPosition = startAnchored;

        // Get target position from dragManager (this is the key simplification!)
        Vector2 targetPos = dragManager.EquippedPos;
        Vector2Int topLeftCell = Vector2Int.FloorToInt(dragManager.TopLeftCellPos);

        Debug.Log($"[EngineerManager] Restoring to equippedPos: {targetPos}, topLeftCell: {topLeftCell}");

        // Tween to target position
        float elapsed = 0f;
        while (elapsed < tweenDuration)
        {
            elapsed += Time.deltaTime;
            float t = EaseOutBack(Mathf.Clamp01(elapsed / tweenDuration));
            itemRect.anchoredPosition = Vector2.Lerp(startAnchored, targetPos, t);
            yield return null;
        }
        itemRect.anchoredPosition = targetPos;

        // Re-mark cells in inventory
        if (inventoryGrid != null)
        {
            inventoryGrid.MarkCells(topLeftCell, itemScript.itemShape, itemObject);
            itemScript.state = Item.itemState.equipped;
            itemScript.TriggerEffectEquip();
        }

        Debug.Log($"✓ Restored {itemScript.itemData.itemName} to inventory at {topLeftCell}");
    }

    private IEnumerator MoveToSlotRoutine(RectTransform rectTransform, Vector2 destination, float duration, System.Action onComplete = null)
    {
        if (rectTransform == null)
            yield break;

        if (duration > 0.0f)
        {
            float elapsed = 0f;
            Vector2 startPos = rectTransform.anchoredPosition;

            while (elapsed < duration && rectTransform != null)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutBack(Mathf.Clamp01(elapsed / duration));

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

    private void OnMergeButtonClicked()
    {
        if (isProcessing) return;
        if (slot1.slottedItemSO == null || slot2.slottedItemSO == null) return;
        if (slot1.slottedItemSO.itemName != slot2.slottedItemSO.itemName) return;

        isProcessing = true;

        if (feedbackText != null)
            feedbackText.text = $"Merging {slot1.slottedItemSO.itemName}...";

        // TODO: Implement merge recipe -> spawn upgraded item
        MergeItems(slot1.slottedItemObject,slot2.slottedItemObject);

        StartCoroutine(ReturnAllItemsAndClose());
    }

    private void OnDeclineButtonClicked()
    {
        if (isProcessing) return;

        isProcessing = true;

        if (feedbackText != null)
            feedbackText.text = "Merge cancelled. Returning items...";

        StartCoroutine(ReturnAllItemsAndClose());
    }

    private void ClearSlots()
    {
        if (slot1 != null)
        {
            slot1.slottedItemSO = null;
            slot1.slottedItemObject = null;
            slot1.dragManager = null;
        }

        if (slot2 != null)
        {
            slot2.slottedItemSO = null;
            slot2.slottedItemObject = null;
            slot2.dragManager = null;
        }

        if (mergeButton != null)
            mergeButton.interactable = false;

        if (feedbackText != null)
            feedbackText.text = "";
    }

    public void CloseEngineerUI()
    {
        if (!IsEngineerUIActive) return;

        Debug.Log("[EngineerManager] Closing Engineer UI");

        ClearSlots();
        isProcessing = false;

        engineerUIPanel.SetActive(false);
        IsEngineerUIActive = false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            TrainFreezeController freezeController = player.GetComponent<TrainFreezeController>();
            if (freezeController != null)
            {
                freezeController.ResumeTrain();
                Debug.Log("[EngineerManager] Train resumed.");
            }
        }

        OnEngineerClosed?.Invoke();
    }

    private string GetSlotName(MergeSlot slot)
    {
        if (slot == slot1) return "Slot 1";
        if (slot == slot2) return "Slot 2";
        return "Unknown Slot";
    }

    private void MergeItems(GameObject item1, GameObject item2)
    {
        item1.GetComponent<Item>().level++;
        Destroy(item2);
        Debug.Log($"<color=teal>component level: {item1.GetComponent<Item>().level}</color>");
    }

    private void OnDestroy()
    {
        if (mergeButton != null)
            mergeButton.onClick.RemoveListener(OnMergeButtonClicked);

        if (declineButton != null)
            declineButton.onClick.RemoveListener(OnDeclineButtonClicked);
    }
}