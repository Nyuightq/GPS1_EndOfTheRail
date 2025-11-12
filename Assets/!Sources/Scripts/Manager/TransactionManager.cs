using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class TransactionManager : MonoBehaviour
{
    public static TransactionManager Instance { get; private set; }

    // Event so TrainFreezeController can listen and resume the train
    public static event System.Action OnTransactionClosed;

    [System.Serializable]
    public class TransactionItem
    {
        public ItemSO itemSO;
        public int cost;
        public TMP_Text costText;
    }

    [Header("UI References")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private Button declineButton;
    [SerializeField] private RectTransform transactionContainer; // Container for transaction items

    [Header("TMP References")]
    [SerializeField] private TMP_Text scrapText;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Transaction Items")]
    [SerializeField] private List<TransactionItem> transactionItems = new List<TransactionItem>();

    [Header("Item Spawn Points")]
    [SerializeField] private Transform[] spawnPoints = new Transform[3]; // 3 empty GameObjects as spawn points

    [Header("Tween Settings")]
    [SerializeField] private float tweenDuration = 0.5f;
    [SerializeField] private Ease tweenEase = Ease.OutBack;

    private PlayerInventoryTemp currentPlayer;
    private List<TransactionItemData> spawnedTransactionItems = new List<TransactionItemData>();
    private InventoryGridScript inventoryGrid;
    private GameObject itemSpawnPrefab;
    private Coroutine monitorCoroutine;

    // --- State controls ---
    public bool IsTransactionUIActive { get; private set; } = false;
    public bool IsCooldownActive { get; private set; } = false;

    private class TransactionItemData
    {
        public GameObject itemObject;
        public Vector2 originalAnchoredPosition;
        public int cost;
        public ItemDragManager dragManager;

        public TransactionItemData(GameObject item, Vector2 originalPos, int itemCost)
        {
            itemObject = item;
            originalAnchoredPosition = originalPos;
            cost = itemCost;
        }
    }

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (uiPanel != null)
            uiPanel.SetActive(false);
    }

    private void Start()
    {
        // Find inventory grid reference
        inventoryGrid = FindFirstObjectByType<InventoryGridScript>();
        
        if (inventoryGrid != null)
        {
            itemSpawnPrefab = inventoryGrid.ItemSpawnPrefab;
        }
        else
        {
            Debug.LogError("TransactionManager: InventoryGridScript not found!");
        }

        // Setup decline button
        if (declineButton != null)
        {
            declineButton.onClick.RemoveAllListeners();
            declineButton.onClick.AddListener(OnDeclineButtonClicked);
        }
    }

    public void OpenTransactionUI(GameObject player)
    {
        if (IsTransactionUIActive || IsCooldownActive)
        {
            Debug.Log("[TransactionManager] Ignored OpenTransactionUI request (UI busy or cooldown active)");
            return;
        }

        currentPlayer = player.GetComponent<PlayerInventoryTemp>();
        if (currentPlayer == null)
        {
            Debug.LogError("TransactionManager: PlayerInventoryTemp not found on player!");
            return;
        }

        if (itemSpawnPrefab == null)
        {
            Debug.LogError("TransactionManager: Item spawn prefab not found!");
            return;
        }

        // Clear previous items
        ClearTransactionItems();

        // Spawn transaction items
        SpawnTransactionItems();

        // Show UI
        if (uiPanel != null)
        {
            uiPanel.SetActive(true);
            IsTransactionUIActive = true;
        }

        UpdateScrapUI();

        // Start monitoring drag states
        monitorCoroutine = StartCoroutine(MonitorTransactionItems());

        Debug.Log("[TransactionManager] Transaction UI opened");
    }

    private void SpawnTransactionItems()
    {
        for (int i = 0; i < transactionItems.Count && i < spawnPoints.Length; i++)
        {
            TransactionItem transItem = transactionItems[i];
            
            // Skip if itemSO is null
            if (transItem.itemSO == null)
            {
                Debug.LogWarning($"[TransactionManager] Transaction item {i} has null ItemSO, skipping.");
                continue;
            }

            // Instantiate item under transaction container
            GameObject newItem = Instantiate(itemSpawnPrefab, transactionContainer);
            Item itemScript = newItem.GetComponent<Item>();
            itemScript.itemData = transItem.itemSO;

            RectTransform newItemRect = newItem.GetComponent<RectTransform>();

            // Position at spawn point
            if (spawnPoints[i] != null)
            {
                RectTransform spawnPointRect = spawnPoints[i].GetComponent<RectTransform>();
                if (spawnPointRect != null)
                {
                    newItemRect.anchoredPosition = spawnPointRect.anchoredPosition;
                }
            }

            // Update cost text
            if (transItem.costText != null)
            {
                transItem.costText.text = $"{transItem.cost}";
            }

            // Store transaction data
            TransactionItemData transactionData = new TransactionItemData(
                newItem, 
                newItemRect.anchoredPosition, 
                transItem.cost
            );

            // Get ItemDragManager reference
            ItemDragManager dragManager = newItem.GetComponent<ItemDragManager>();
            if (dragManager != null)
            {
                transactionData.dragManager = dragManager;
            }

            spawnedTransactionItems.Add(transactionData);
        }
    }

    private IEnumerator MonitorTransactionItems()
    {
        while (spawnedTransactionItems.Count > 0 && uiPanel.activeSelf)
        {
            for (int i = spawnedTransactionItems.Count - 1; i >= 0; i--)
            {
                TransactionItemData transactionData = spawnedTransactionItems[i];
                
                if (transactionData.itemObject == null)
                {
                    // Item was destroyed
                    spawnedTransactionItems.RemoveAt(i);
                    continue;
                }

                Item itemScript = transactionData.itemObject.GetComponent<Item>();
                if (itemScript == null) continue;

                // Check if item was successfully equipped (placed in inventory)
                if (itemScript.state == Item.itemState.equipped)
                {
                    // Check if player has enough scraps
                    if (currentPlayer != null && currentPlayer.scrapCount >= transactionData.cost)
                    {
                        // Deduct scraps
                        currentPlayer.SpendScrap(transactionData.cost);
                        feedbackText.text = $"Purchased for {transactionData.cost} scraps!";
                        Debug.Log($"Transaction successful! Spent {transactionData.cost} scraps.");

                        // Remove from tracking
                        spawnedTransactionItems.RemoveAt(i);
                        
                        // Close UI after short delay
                        yield return new WaitForSeconds(0.1f);
                        CloseTransactionUI();
                        yield break;
                    }
                    else
                    {
                        // Not enough scraps - return item
                        feedbackText.text = "Not enough scraps!";
                        Debug.Log("Not enough scraps for this item!");
                        
                        // Force item back to unequipped state
                        itemScript.state = Item.itemState.unequipped;
                        
                        // Tween back to original position
                        TweenItemBackToPosition(transactionData);
                    }
                }
            }

            yield return null; // Wait one frame
        }
    }

    /// <summary>
    /// Alternative: Call this from ItemDragManager.LeftRelease() for immediate response
    /// </summary>
    public void OnItemDragReleased(GameObject item)
    {
        TransactionItemData transactionData = spawnedTransactionItems.Find(t => t.itemObject == item);
        
        if (transactionData == null)
            return;

        Item itemScript = item.GetComponent<Item>();
        
        if (itemScript != null)
        {
            if (itemScript.state == Item.itemState.equipped)
            {
                // Check if player has enough scraps
                if (currentPlayer != null && currentPlayer.scrapCount >= transactionData.cost)
                {
                    // Deduct scraps
                    currentPlayer.SpendScrap(transactionData.cost);
                    feedbackText.text = $"Purchased for {transactionData.cost} scraps!";
                    Debug.Log($"Transaction successful! Spent {transactionData.cost} scraps.");

                    // Close UI
                    CloseTransactionUI();
                }
                else
                {
                    // Not enough scraps
                    feedbackText.text = "Not enough scraps!";
                    Debug.Log("Not enough scraps for this item!");
                    
                    // Force item back to unequipped state
                    itemScript.state = Item.itemState.unequipped;
                    
                    // Tween back
                    TweenItemBackToPosition(transactionData);
                }
            }
            else
            {
                // Not placed, tween back
                Debug.Log("Item not placed, tweening back to transaction spot.");
                TweenItemBackToPosition(transactionData);
            }
        }
    }

    private void TweenItemBackToPosition(TransactionItemData transactionData)
    {
        if (transactionData.itemObject == null) return;

        RectTransform itemRect = transactionData.itemObject.GetComponent<RectTransform>();
        
        // Check if item is still child of transaction container
        if (itemRect.parent != transactionContainer)
        {
            // Store current world position
            Vector3 currentWorldPos = itemRect.position;
            
            // Change parent back to transaction container
            itemRect.SetParent(transactionContainer);
            
            // Restore world position temporarily
            itemRect.position = currentWorldPos;
        }

        // Tween back to original position
        itemRect.DOAnchorPos(transactionData.originalAnchoredPosition, tweenDuration)
            .SetEase(tweenEase);
    }

    private void ClearTransactionItems()
    {
        // Stop monitoring coroutine if running
        if (monitorCoroutine != null)
        {
            StopCoroutine(monitorCoroutine);
            monitorCoroutine = null;
        }

        // Only destroy items that are still in transaction panel (not equipped)
        foreach (TransactionItemData transactionData in spawnedTransactionItems)
        {
            if (transactionData.itemObject != null)
            {
                Item itemScript = transactionData.itemObject.GetComponent<Item>();
                
                // Only destroy if NOT equipped
                if (itemScript == null || itemScript.state != Item.itemState.equipped)
                {
                    Destroy(transactionData.itemObject);
                }
            }
        }

        spawnedTransactionItems.Clear();
        
        // Clear cost texts
        foreach (TransactionItem transItem in transactionItems)
        {
            if (transItem.costText != null)
                transItem.costText.text = "";
        }
        
        // Kill all active tweens
        DOTween.Kill(transform);
    }

    public void CloseTransactionUI()
    {
        if (!IsTransactionUIActive)
            return;

        Debug.Log("CloseTransactionUI called");
        
        // Stop monitoring coroutine FIRST
        if (monitorCoroutine != null)
        {
            StopCoroutine(monitorCoroutine);
            monitorCoroutine = null;
        }
        
        // Only destroy items still in transaction container
        for (int i = spawnedTransactionItems.Count - 1; i >= 0; i--)
        {
            TransactionItemData transactionData = spawnedTransactionItems[i];
            
            if (transactionData.itemObject != null)
            {
                // Check if parent is still transaction container
                bool isStillInTransactionPanel = transactionData.itemObject.transform.parent == transactionContainer;
                
                if (isStillInTransactionPanel)
                {
                    Debug.Log($"Destroying unpurchased transaction item: {transactionData.itemObject.name}");
                    Destroy(transactionData.itemObject);
                }
                else
                {
                    Debug.Log($"Preserving purchased item (parent changed): {transactionData.itemObject.name}");
                }
            }
            
            spawnedTransactionItems.RemoveAt(i);
        }
        
        // Clear cost texts
        foreach (TransactionItem transItem in transactionItems)
        {
            if (transItem.costText != null)
                transItem.costText.text = "";
        }
        
        // Kill all active tweens
        DOTween.Kill(transform);
        
        // Hide panel
        if (uiPanel != null)
            uiPanel.SetActive(false);

        currentPlayer = null;
        IsTransactionUIActive = false;

        Debug.Log("Transaction UI closed. Event fired.");
        OnTransactionClosed?.Invoke();

        // Begin cooldown before another tile can trigger
        StartCoroutine(CloseCooldown());
    }

    private IEnumerator CloseCooldown()
    {
        IsCooldownActive = true;
        yield return new WaitForSeconds(0.5f);
        IsCooldownActive = false;
        Debug.Log("[TransactionManager] Cooldown finished, ready for next trigger.");
    }

    private void UpdateScrapUI()
    {
        if (currentPlayer == null) return;

        if (scrapText != null)
            scrapText.text = "Scraps: " + currentPlayer.scrapCount;
    }

    /// <summary>
    /// Called by the Decline button in UI
    /// </summary>
    public void OnDeclineButtonClicked()
    {
        Debug.Log("Transaction declined by player.");
        feedbackText.text = "Transaction declined.";
        CloseTransactionUI();
    }

    public void DestroyInstance()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
            Instance = null;
            Debug.Log("[TransactionManager] Instance destroyed for replay.");
        }
    }
}