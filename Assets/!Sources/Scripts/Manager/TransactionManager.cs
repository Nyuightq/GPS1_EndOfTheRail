using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TransactionManager : MonoBehaviour
{
    public static TransactionManager Instance { get; private set; }
    public static event System.Action OnTransactionClosed;

    [System.Serializable]
    public class TransactionSlot
    {
        [Header("Runtime Data (Auto-filled)")]
        public ItemSO itemSO;
        public int cost;
        
        [Header("UI References")]
        public RectTransform anchorPoint;
        public TMP_Text nameText;
        public TMP_Text costText;
        public Image slotImage; // The slot's background/border image
    }

    [System.Serializable]
    public class ItemWithCost
    {
        public ItemSO itemSO;
        public int cost;
    }

    [Header("Available Items Pool (with specific costs)")]
    [SerializeField] private List<ItemWithCost> availableItems = new List<ItemWithCost>();

    [Header("Transaction Slots")]
    [SerializeField] private List<TransactionSlot> transactionSlots = new List<TransactionSlot>(3);

    [Header("UI References")]
    [SerializeField] private UI_BaseEventPanel uiPanel;
    [SerializeField] private Button declineButton;
    [SerializeField] private RectTransform transactionContainer;

    [Header("Feedback")]
    [SerializeField] private TMP_Text scrapText;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Tween Settings")]
    [SerializeField] private float tweenDuration = 0.3f;

    [Header("Spawn Bounds")]
    [SerializeField] private bool enforceSpawnBounds = true;

    private PlayerStatusManager playerStatus;
    private List<TransactionItemData> spawnedItems = new List<TransactionItemData>();
    private InventoryGridScript inventoryGrid;
    private GameObject itemSpawnPrefab;

    public bool IsTransactionUIActive { get; private set; } = false;
    public bool IsCooldownActive { get; private set; } = false;

    private class TransactionItemData
    {
        public GameObject itemObject;
        public RectTransform rectTransform;
        public Item itemScript;
        public Vector2 initialAnchoredPosition;
        public RectTransform anchorPoint;
        public int cost;
        public bool wasPurchased;
        public Coroutine moveRoutine;
        public int slotIndex; // Track which slot this item belongs to

        public TransactionItemData(GameObject obj, Vector2 initialPos, RectTransform anchor, int itemCost, int slot)
        {
            itemObject = obj;
            rectTransform = obj.GetComponent<RectTransform>();
            itemScript = obj.GetComponent<Item>();
            initialAnchoredPosition = initialPos;
            anchorPoint = anchor;
            cost = itemCost;
            wasPurchased = false;
            moveRoutine = null;
            slotIndex = slot;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (uiPanel != null)
            uiPanel.HideEventPanel();
    }

    private void Start()
    {
        inventoryGrid = FindFirstObjectByType<InventoryGridScript>();
        
        if (inventoryGrid != null)
        {
            itemSpawnPrefab = inventoryGrid.ItemSpawnPrefab;
        }
        else
        {
            Debug.LogError("TransactionManager: InventoryGridScript not found!");
        }

        if (transactionSlots.Count != 3)
        {
            Debug.LogWarning("TransactionManager: Should have exactly 3 transaction slots!");
        }

        if (declineButton != null)
        {
            declineButton.onClick.RemoveAllListeners();
            declineButton.onClick.AddListener(OnDeclineButtonClicked);
            Debug.Log("TransactionManager: Decline button listener added.");
        }
        else
        {
            Debug.LogWarning("TransactionManager: Decline button not assigned!");
        }
    }

    public void OpenTransactionUI(GameObject player)
    {
        if (IsTransactionUIActive || IsCooldownActive)
        {
            Debug.Log("[TransactionManager] Ignored OpenTransactionUI (UI busy or cooldown)");
            return;
        }

        playerStatus = FindFirstObjectByType<PlayerStatusManager>();
        if (playerStatus == null)
        {
            Debug.LogError("TransactionManager: PlayerStatusManager not found in scene!");
            return;
        }

        if (itemSpawnPrefab == null)
        {
            Debug.LogError("TransactionManager: Item spawn prefab not found!");
            return;
        }

        ClearTransactionItems();
        RandomizeAndSpawnTransactionItems();

        if (uiPanel != null)
        {
            uiPanel.ShowEventPanel();
            IsTransactionUIActive = true;
        }

        UpdateScrapUI();
        
        // Clear feedback when opening
        if (feedbackText != null)
            feedbackText.text = "";
            
        Debug.Log("[TransactionManager] Transaction UI opened with randomized items.");
    }

    private void RandomizeAndSpawnTransactionItems()
    {
        if (availableItems.Count == 0)
        {
            Debug.LogError("TransactionManager: No available items in pool!");
            return;
        }

        List<ItemWithCost> tempPool = new List<ItemWithCost>(availableItems);
        int itemsToSelect = Mathf.Min(transactionSlots.Count, tempPool.Count);

        for (int i = 0; i < itemsToSelect; i++)
        {
            int randomIndex = Random.Range(0, tempPool.Count);
            ItemWithCost selectedItem = tempPool[randomIndex];
            tempPool.RemoveAt(randomIndex);

            TransactionSlot slot = transactionSlots[i];

            slot.itemSO = selectedItem.itemSO;
            slot.cost = selectedItem.cost;

            if (slot.nameText != null)
            {
                slot.nameText.text = selectedItem.itemSO.itemName;
            }

            if (slot.costText != null)
            {
                slot.costText.text = $"Cost: {selectedItem.cost}";
            }
            
            // Reset slot appearance
            if (slot.slotImage != null)
            {
                slot.slotImage.color = Color.white; // Reset to normal color
            }

            if (slot.anchorPoint == null)
            {
                Debug.LogWarning($"TransactionManager: Slot {i} anchor point is null!");
                continue;
            }

            Vector2 spawnPosition = slot.anchorPoint.anchoredPosition;

            if (enforceSpawnBounds && !IsPositionInBounds(spawnPosition, transactionContainer))
            {
                Debug.LogWarning($"TransactionManager: Anchor {i} position {spawnPosition} is out of bounds! Clamping...");
                spawnPosition = ClampToBounds(spawnPosition, transactionContainer);
            }

            GameObject newItem = Instantiate(itemSpawnPrefab, transactionContainer);
            
            Item itemScript = newItem.GetComponent<Item>();
            if (itemScript != null)
            {
                itemScript.itemData = selectedItem.itemSO;
            }

            RectTransform itemRect = newItem.GetComponent<RectTransform>();
            itemRect.anchoredPosition = spawnPosition;

            TransactionItemData transData = new TransactionItemData(
                newItem,
                spawnPosition,
                slot.anchorPoint,
                selectedItem.cost,
                i // Pass slot index
            );
            spawnedItems.Add(transData);

            Debug.Log($"✓ Spawned {selectedItem.itemSO.itemName} at position {spawnPosition} (Cost: {selectedItem.cost})");
        }
    }

    private bool IsPositionInBounds(Vector2 position, RectTransform container)
    {
        if (container == null)
            return true;

        Rect rect = container.rect;
        return position.x >= rect.xMin && position.x <= rect.xMax &&
               position.y >= rect.yMin && position.y <= rect.yMax;
    }

    private Vector2 ClampToBounds(Vector2 position, RectTransform container)
    {
        if (container == null)
            return position;

        Rect rect = container.rect;
        return new Vector2(
            Mathf.Clamp(position.x, rect.xMin, rect.xMax),
            Mathf.Clamp(position.y, rect.yMin, rect.yMax)
        );
    }

    public void OnItemReleased(GameObject item)
    {
        if (!IsTransactionUIActive)
            return;

        TransactionItemData transData = spawnedItems.Find(t => t.itemObject == item);
        
        if (transData == null)
            return;

        // Check if already purchased
        if (transData.wasPurchased)
        {
            Debug.Log("Item already purchased, ignoring.");
            return;
        }

        StopMoveRoutineIfAny(transData);

        if (transData.itemScript.state == Item.itemState.equipped)
        {
            Debug.Log($"Item {transData.itemScript.itemData.itemName} placed in inventory. Checking scraps...");

            if (playerStatus != null && playerStatus.Scraps >= transData.cost)
            {
                bool success = playerStatus.ConsumeScraps(transData.cost);
                
                if (success)
                {
                    if (feedbackText != null)
                        feedbackText.text = $"Purchased {transData.itemScript.itemData.itemName} for {transData.cost} scraps!";
                    
                    Debug.Log($"✓ Transaction successful! Spent {transData.cost} scraps.");

                    transData.wasPurchased = true;
                    
                    // Dim the slot to show item was purchased
                    if (transData.slotIndex >= 0 && transData.slotIndex < transactionSlots.Count)
                    {
                        TransactionSlot slot = transactionSlots[transData.slotIndex];
                        if (slot.slotImage != null)
                        {
                            // Dim the slot - you can adjust the color/alpha as desired
                            Color dimmedColor = slot.slotImage.color;
                            dimmedColor.a = 0.3f; // Make it 30% transparent
                            slot.slotImage.color = dimmedColor;
                        }
                        
                        // Update cost text to show "SOLD"
                        if (slot.costText != null)
                        {
                            slot.costText.text = "SOLD";
                        }
                    }

                    UpdateScrapUI();
                    
                    // Check if all items purchased (optional auto-close)
                    CheckAllItemsPurchased();
                }
                else
                {
                    Debug.LogError("ConsumeScraps failed unexpectedly!");
                    HandleInsufficientScraps(transData);
                }
            }
            else
            {
                HandleInsufficientScraps(transData);
            }
        }
        else if (transData.itemScript.state == Item.itemState.unequipped)
        {
            Debug.Log("Item not placed in inventory, snapping back to anchor.");
            SnapBackToAnchor(transData, tweenDuration);
        }
    }

    private void CheckAllItemsPurchased()
    {
        // Optional: Auto-close if all items are purchased
        // Comment out if you don't want this behavior
        bool allPurchased = true;
        foreach (TransactionItemData transData in spawnedItems)
        {
            if (!transData.wasPurchased)
            {
                allPurchased = false;
                break;
            }
        }

        if (allPurchased)
        {
            if (feedbackText != null)
                feedbackText.text = "All items purchased! Closing...";
            StartCoroutine(CloseUIAfterDelay(1f));
        }
    }

    private void HandleInsufficientScraps(TransactionItemData transData)
    {
        if (feedbackText != null)
            feedbackText.text = "Not enough scraps!";
        
        Debug.Log($"✗ Not enough scraps! Need {transData.cost}, have {playerStatus?.Scraps ?? 0}");

        if (inventoryGrid != null && transData.itemScript != null)
        {
            InvCellData[,] grid = inventoryGrid.inventoryGrid;
            bool foundItem = false;
            
            for (int x = 0; x < grid.GetLength(0) && !foundItem; x++)
            {
                for (int y = 0; y < grid.GetLength(1) && !foundItem; y++)
                {
                    if (grid[x, y].item == transData.itemObject)
                    {
                        Vector2Int topLeft = new Vector2Int(x, y);
                        inventoryGrid.MarkCells(topLeft, transData.itemScript.itemShape, null);
                        Debug.Log($"Cleared item from inventory grid at {topLeft}");
                        foundItem = true;
                    }
                }
            }

            if (!foundItem)
            {
                Debug.LogWarning("Item not found in inventory grid, but marked as equipped!");
            }
        }

        transData.itemScript.state = Item.itemState.unequipped;
        SnapBackToAnchor(transData, tweenDuration);
    }

    private IEnumerator CloseUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseTransactionUI();
    }

    private void SnapBackToAnchor(TransactionItemData transData, float duration)
    {
        if (transData.itemObject == null)
            return;

        if (transData.rectTransform.parent != transactionContainer)
        {
            Vector3 worldPos = transData.rectTransform.position;
            transData.rectTransform.SetParent(transactionContainer);
            transData.rectTransform.position = worldPos;
            
            Debug.Log("Item parent restored to transaction container");
        }

        StopMoveRoutineIfAny(transData);
        transData.moveRoutine = StartCoroutine(
            MoveToAnchorRoutine(transData, transData.initialAnchoredPosition, duration)
        );
    }

    private void StopMoveRoutineIfAny(TransactionItemData transData)
    {
        if (transData.moveRoutine != null)
        {
            StopCoroutine(transData.moveRoutine);
            transData.moveRoutine = null;
        }
    }

    private IEnumerator MoveToAnchorRoutine(TransactionItemData transData, Vector2 destination, float duration)
    {
        if (transData.itemObject == null)
        {
            transData.moveRoutine = null;
            yield break;
        }

        if (duration > 0.0f)
        {
            float startTime = Time.time;
            Vector2 startPos = transData.rectTransform.anchoredPosition;
            float tweenCoeff = 1.0f / duration;

            float dt = 0.0f;
            while (dt < 1.0f && transData.itemObject != null)
            {
                dt = (Time.time - startTime) * tweenCoeff;
                float t = EaseOutBack(dt);
                
                if (transData.rectTransform != null)
                {
                    transData.rectTransform.anchoredPosition = Vector2.Lerp(startPos, destination, t);
                }
                
                yield return null;
            }
        }

        if (transData.rectTransform != null)
        {
            transData.rectTransform.anchoredPosition = destination;
            Debug.Log($"✓ Item snapped back to {destination}");
        }
        
        transData.moveRoutine = null;
    }

    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private void ClearTransactionItems()
    {
        foreach (TransactionItemData transData in spawnedItems)
        {
            if (transData.itemObject != null)
            {
                StopMoveRoutineIfAny(transData);
                
                if (transData.itemScript == null || transData.itemScript.state != Item.itemState.equipped)
                {
                    Destroy(transData.itemObject);
                }
            }
        }

        spawnedItems.Clear();

        foreach (TransactionSlot slot in transactionSlots)
        {
            slot.itemSO = null;
            slot.cost = 0;
            
            if (slot.nameText != null)
                slot.nameText.text = "";
            
            if (slot.costText != null)
                slot.costText.text = "";
                
            if (slot.slotImage != null)
                slot.slotImage.color = Color.white;
        }
    }

    public void CloseTransactionUI()
    {
        if (!IsTransactionUIActive)
            return;

        Debug.Log("CloseTransactionUI called");

        for (int i = spawnedItems.Count - 1; i >= 0; i--)
        {
            TransactionItemData transData = spawnedItems[i];
            
            if (transData.itemObject != null && !transData.wasPurchased)
            {
                StopMoveRoutineIfAny(transData);
                
                bool isStillInPanel = transData.itemObject.transform.parent == transactionContainer;
                
                if (isStillInPanel || transData.itemScript.state == Item.itemState.unequipped)
                {
                    Debug.Log($"Destroying unpurchased item: {transData.itemObject.name}");
                    Destroy(transData.itemObject);
                }
            }
        }

        spawnedItems.Clear();

        foreach (TransactionSlot slot in transactionSlots)
        {
            slot.itemSO = null;
            slot.cost = 0;
            
            if (slot.nameText != null)
                slot.nameText.text = "";
            
            if (slot.costText != null)
                slot.costText.text = "";
                
            if (slot.slotImage != null)
                slot.slotImage.color = Color.white;
        }

        if (uiPanel != null)
        {
            uiPanel.HideEventPanel(() => OnTransactionClosed?.Invoke());
            SoundManager.Instance.PlaySFX("SFX_ButtonOnCancel");
        }

        playerStatus = null;
        IsTransactionUIActive = false;

        Debug.Log("Transaction UI closed.");

        StartCoroutine(CloseCooldown());
    }

    private IEnumerator CloseCooldown()
    {
        IsCooldownActive = true;
        yield return new WaitForSeconds(0.5f);
        IsCooldownActive = false;
        Debug.Log("[TransactionManager] Cooldown finished.");
    }

    private void UpdateScrapUI()
    {
        if (playerStatus == null) return;

        if (scrapText != null)
            scrapText.text = "Scraps: " + playerStatus.Scraps;
    }

    public void OnDeclineButtonClicked()
    {
        Debug.Log("=== TRANSACTION CLOSED ===");
        if (feedbackText != null)
            feedbackText.text = "Transaction closed.";
        CloseTransactionUI();
    }

    public void DestroyInstance()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
            Instance = null;
        }
    }

    private void OnDestroy()
    {
        if (declineButton != null)
        {
            declineButton.onClick.RemoveListener(OnDeclineButtonClicked);
        }

        foreach (TransactionItemData transData in spawnedItems)
        {
            StopMoveRoutineIfAny(transData);
        }
    }
}