using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance;
    public static System.Action OnRewardClosed;

    [Header("Reward Configuration")]
    [SerializeField] private List<ItemSO> availableRewards = new List<ItemSO>();
    [SerializeField] private int numberOfRewards = 3;

    [Header("Reward UI")]
    [SerializeField] private GameObject rewardUIPanel;
    [SerializeField] private RectTransform rewardContainer; // Container for reward items (like inventoryRect)

    [Header("Reward Spawn Settings")]
    [SerializeField] private Vector2 spawnCentreOffset = Vector2.zero;
    [SerializeField] private float spawnMarginX = 50f; // Spacing between items

    [Header("Tween Settings")]
    [SerializeField] private float tweenDuration = 0.5f;
    [SerializeField] private Ease tweenEase = Ease.OutBack;

    private List<ItemSO> selectedRewards = new List<ItemSO>();
    private List<RewardItemData> spawnedRewardItems = new List<RewardItemData>();
    private InventoryGridScript inventoryGrid;
    private GameObject itemSpawnPrefab;
    private Coroutine monitorCoroutine;

    private class RewardItemData
    {
        public GameObject itemObject;
        public Vector2 originalAnchoredPosition;
        public ItemDragManager dragManager;

        public RewardItemData(GameObject item, Vector2 originalPos)
        {
            itemObject = item;
            originalAnchoredPosition = originalPos;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
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
            Debug.LogError("RewardManager: InventoryGridScript not found!");
        }

        // Hide UI on start
        if (rewardUIPanel != null)
        {
            rewardUIPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Shows the reward UI with randomized items (matches your existing API)
    /// </summary>
    public void OpenRewardUI(GameObject player = null)
    {
        ShowRewards();
    }

    /// <summary>
    /// Shows the reward UI with randomized items
    /// </summary>
    public void ShowRewards()
    {
        if (rewardUIPanel == null || rewardContainer == null)
        {
            Debug.LogError("RewardManager: UI Panel or Reward Container not properly configured!");
            return;
        }

        if (itemSpawnPrefab == null)
        {
            Debug.LogError("RewardManager: Item spawn prefab not found from InventoryGridScript!");
            return;
        }

        // Clear previous rewards
        ClearRewards();

        // Randomize and select rewards
        selectedRewards = GetRandomRewards(numberOfRewards);

        // Spawn rewards using EXACT same pattern as InventoryGridScript.SpawnItems()
        SpawnRewardItems(itemSpawnPrefab, selectedRewards, spawnCentreOffset, spawnMarginX);

        // Show UI
        rewardUIPanel.SetActive(true);

        // Start monitoring drag states
        monitorCoroutine = StartCoroutine(MonitorRewardItems());
    }

    /// <summary>
    /// Gets random rewards from the available rewards list
    /// </summary>
    private List<ItemSO> GetRandomRewards(int count)
    {
        List<ItemSO> rewards = new List<ItemSO>();
        List<ItemSO> tempList = new List<ItemSO>(availableRewards);

        // Ensure we don't try to get more rewards than available
        count = Mathf.Min(count, tempList.Count);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, tempList.Count);
            rewards.Add(tempList[randomIndex]);
            tempList.RemoveAt(randomIndex); // Remove to avoid duplicates
        }

        return rewards;
    }

    /// <summary>
    /// Spawns reward items using EXACT same method as InventoryGridScript.SpawnItems()
    /// </summary>
    private void SpawnRewardItems(GameObject itemPrefab, List<ItemSO> itemList, Vector2 pos, float spawnMarginX)
    {
        float cellSize = GameManager.cellSize;
        float offsetX = pos.x;
        float totalWidth = spawnMarginX * (itemList.Count - 1);
        foreach (ItemSO reward in itemList) totalWidth += reward.itemWidth * cellSize;

        for (int i = 0; i < itemList.Count; i++)
        {
            // EXACT SAME as InventoryGridScript - instantiate under rewardContainer (like inventoryRect)
            GameObject newItem = Instantiate(itemPrefab, rewardContainer);
            Item itemScript = newItem.GetComponent<Item>();
            itemScript.itemData = itemList[i];

            RectTransform newItemRect = newItem.GetComponent<RectTransform>();

            // EXACT SAME positioning as InventoryGridScript
            Vector2 itemPosition = new Vector2(offsetX - totalWidth * 0.5f, pos.y);
            newItemRect.anchoredPosition = itemPosition;
            offsetX += itemList[i].itemWidth * cellSize + spawnMarginX;

            // Store reward data for tracking
            RewardItemData rewardData = new RewardItemData(newItem, itemPosition);

            // Get ItemDragManager reference
            ItemDragManager dragManager = newItem.GetComponent<ItemDragManager>();
            if (dragManager != null)
            {
                rewardData.dragManager = dragManager;
            }

            spawnedRewardItems.Add(rewardData);
        }
    }

    /// <summary>
    /// Coroutine to monitor drag states and handle rewards
    /// </summary>
    private IEnumerator MonitorRewardItems()
    {
        while (spawnedRewardItems.Count > 0 && rewardUIPanel.activeSelf)
        {
            for (int i = spawnedRewardItems.Count - 1; i >= 0; i--)
            {
                RewardItemData rewardData = spawnedRewardItems[i];
                
                if (rewardData.itemObject == null)
                {
                    // Item was destroyed
                    spawnedRewardItems.RemoveAt(i);
                    continue;
                }

                Item itemScript = rewardData.itemObject.GetComponent<Item>();
                if (itemScript == null) continue;

                // Check if item was successfully equipped (placed in inventory)
                if (itemScript.state == Item.itemState.equipped)
                {
                    // Remove from tracking FIRST
                    spawnedRewardItems.RemoveAt(i);
                    
                    // Then close UI
                    yield return new WaitForSeconds(0.1f);
                    CloseRewardUI();
                    yield break;
                }
            }

            yield return null; // Wait one frame
        }
    }

    /// <summary>
    /// Tweens item back to original position if not placed in inventory
    /// </summary>
    private void TweenItemBackToPosition(RewardItemData rewardData)
    {
        if (rewardData.itemObject == null) return;

        RectTransform itemRect = rewardData.itemObject.GetComponent<RectTransform>();
        
        // Check if item is still child of reward container
        if (itemRect.parent != rewardContainer)
        {
            // Store current world position
            Vector3 currentWorldPos = itemRect.position;
            
            // Change parent back to reward container
            itemRect.SetParent(rewardContainer);
            
            // Restore world position temporarily
            itemRect.position = currentWorldPos;
        }

        // Tween back to original position
        itemRect.DOAnchorPos(rewardData.originalAnchoredPosition, tweenDuration)
            .SetEase(tweenEase);
    }

    /// <summary>
    /// Alternative approach: Call this from ItemDragManager.LeftRelease() for immediate response
    /// Add this to your ItemDragManager.LeftRelease() method:
    /// if (RewardManager.Instance != null) RewardManager.Instance.OnItemDragReleased(gameObject);
    /// </summary>
    public void OnItemDragReleased(GameObject item)
    {
        RewardItemData rewardData = spawnedRewardItems.Find(r => r.itemObject == item);
        
        if (rewardData == null)
            return;

        Item itemScript = item.GetComponent<Item>();
        
        if (itemScript != null)
        {
            if (itemScript.state == Item.itemState.equipped)
            {
                // Successfully placed in inventory - close panel
                Debug.Log("Item placed in inventory! Closing reward panel.");
                CloseRewardUI();
            }
            else
            {
                // Not placed, tween back
                Debug.Log("Item not placed, tweening back to reward spot.");
                TweenItemBackToPosition(rewardData);
            }
        }
    }

    /// <summary>
    /// Clears all spawned reward items without destroying equipped ones
    /// </summary>
    private void ClearRewards()
    {
        // Stop monitoring coroutine if running
        if (monitorCoroutine != null)
        {
            StopCoroutine(monitorCoroutine);
            monitorCoroutine = null;
        }

        // Only destroy items that are still in reward panel (not equipped)
        foreach (RewardItemData rewardData in spawnedRewardItems)
        {
            if (rewardData.itemObject != null)
            {
                Item itemScript = rewardData.itemObject.GetComponent<Item>();
                
                // Only destroy if NOT equipped (still in reward panel)
                if (itemScript == null || itemScript.state != Item.itemState.equipped)
                {
                    Destroy(rewardData.itemObject);
                }
            }
        }

        spawnedRewardItems.Clear();
        selectedRewards.Clear();
        
        // Kill all active tweens to prevent errors
        DOTween.Kill(transform);
    }

    /// <summary>
    /// Closes the reward UI - called by decline button or when item is placed
    /// </summary>
public void CloseRewardUI()
{
    Debug.Log("CloseRewardUI called");
    
    // Stop monitoring coroutine FIRST
    if (monitorCoroutine != null)
    {
        StopCoroutine(monitorCoroutine);
        monitorCoroutine = null;
    }
    
    // Only destroy items still in reward container
    for (int i = spawnedRewardItems.Count - 1; i >= 0; i--)
    {
        RewardItemData rewardData = spawnedRewardItems[i];
        
        if (rewardData.itemObject != null)
        {
            // Check if parent is still reward container
            bool isStillInRewardPanel = rewardData.itemObject.transform.parent == rewardContainer;
            
            if (isStillInRewardPanel)
            {
                Debug.Log($"Destroying unequipped reward item: {rewardData.itemObject.name}");
                Destroy(rewardData.itemObject);
            }
            else
            {
                Debug.Log($"Preserving equipped item (parent changed): {rewardData.itemObject.name}");
            }
        }
        
        spawnedRewardItems.RemoveAt(i);
    }
    
    selectedRewards.Clear();
    
    // Kill all active tweens
    DOTween.Kill(transform);
    
    // Hide panel
    if (rewardUIPanel != null)
    {
        rewardUIPanel.SetActive(false);
    }

    OnRewardClosed?.Invoke();
}

    /// <summary>
    /// Called by the Decline button in UI
    /// Hook this up in the inspector: Button -> OnClick() -> RewardManager.OnDeclineButtonClicked()
    /// </summary>
    public void OnDeclineButtonClicked()
    {
        Debug.Log("Decline button clicked!");
        CloseRewardUI();
    }

    /// <summary>
    /// Destroys the RewardManager instance (called from cleanup)
    /// </summary>
    public void DestroyInstance()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        Destroy(gameObject);
    }
}