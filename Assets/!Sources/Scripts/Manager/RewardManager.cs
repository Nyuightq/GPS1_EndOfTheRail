// NO Tweening

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance;
    public static System.Action OnRewardClosed;

    [System.Serializable]
    public class RewardSlot
    {
        public ItemSO itemSO;
        public RectTransform anchorPoint;
        public TextMeshProUGUI nameText;
    }

    [Header("Reward Configuration")]
    [SerializeField] private List<ItemSO> availableRewards = new List<ItemSO>();
    [SerializeField] private List<RewardSlot> rewardSlots = new List<RewardSlot>(3);

    [Header("Reward UI")]
    [SerializeField] private GameObject rewardUIPanel;
    [SerializeField] private RectTransform rewardContainer;

    [Header("Tween Settings")]
    [SerializeField] private float tweenDuration = 0.3f;

    private InventoryGridScript inventoryGrid;
    private GameObject itemSpawnPrefab;
    private List<RewardItemData> rewardItems = new List<RewardItemData>();

    private class RewardItemData
    {
        public GameObject itemObject;
        public RectTransform rectTransform;
        public Item itemScript;
        public Vector2 initialAnchoredPosition;
        public RectTransform anchorPoint;
        public bool wasPlacedInInventory;
        public Coroutine moveRoutine;

        public RewardItemData(GameObject obj, Vector2 initialPos, RectTransform anchor)
        {
            itemObject = obj;
            rectTransform = obj.GetComponent<RectTransform>();
            itemScript = obj.GetComponent<Item>();
            initialAnchoredPosition = initialPos;
            anchorPoint = anchor;
            wasPlacedInInventory = false;
            moveRoutine = null;
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
        inventoryGrid = FindFirstObjectByType<InventoryGridScript>();
        
        if (inventoryGrid != null)
        {
            itemSpawnPrefab = inventoryGrid.ItemSpawnPrefab;
        }
        else
        {
            Debug.LogError("RewardManager: InventoryGridScript not found!");
        }

        if (rewardSlots.Count != 3)
        {
            Debug.LogWarning("RewardManager: Should have exactly 3 reward slots configured!");
        }

        if (rewardUIPanel != null)
        {
            rewardUIPanel.SetActive(false);
        }
    }

    public void OpenRewardUI(GameObject player = null)
    {
        ShowRewards();
    }

    public void ShowRewards()
    {
        if (rewardUIPanel == null || rewardContainer == null)
        {
            Debug.LogError("RewardManager: UI Panel or Reward Container not configured!");
            return;
        }

        if (itemSpawnPrefab == null)
        {
            Debug.LogError("RewardManager: Item spawn prefab not found!");
            return;
        }

        if (rewardSlots.Count == 0)
        {
            Debug.LogError("RewardManager: No reward slots configured!");
            return;
        }

        ClearRewards();

        List<ItemSO> randomizedRewards = GetRandomRewards(rewardSlots.Count);

        for (int i = 0; i < rewardSlots.Count && i < randomizedRewards.Count; i++)
        {
            rewardSlots[i].itemSO = randomizedRewards[i];
            
            if (rewardSlots[i].nameText != null)
            {
                rewardSlots[i].nameText.text = randomizedRewards[i].itemName;
            }
        }

        SpawnRewardItemsAtAnchors(randomizedRewards);

        rewardUIPanel.SetActive(true);
    }

    private void SpawnRewardItemsAtAnchors(List<ItemSO> items)
    {
        for (int i = 0; i < items.Count && i < rewardSlots.Count; i++)
        {
            ItemSO itemData = items[i];
            RectTransform anchorPoint = rewardSlots[i].anchorPoint;

            if (anchorPoint == null)
            {
                Debug.LogError($"RewardManager: Anchor point {i} is null!");
                continue;
            }

            // Instantiate under reward container
            GameObject newItem = Instantiate(itemSpawnPrefab, rewardContainer);
            
            Item itemScript = newItem.GetComponent<Item>();
            if (itemScript != null)
            {
                itemScript.itemData = itemData;
            }

            RectTransform itemRect = newItem.GetComponent<RectTransform>();

            // Position at anchor point
            itemRect.anchoredPosition = anchorPoint.anchoredPosition;

            // Store reward data
            RewardItemData rewardData = new RewardItemData(
                newItem, 
                anchorPoint.anchoredPosition, 
                anchorPoint
            );
            rewardItems.Add(rewardData);

            Debug.Log($"Spawned {itemData.itemName} at anchor position {anchorPoint.anchoredPosition}");
        }
    }

    private List<ItemSO> GetRandomRewards(int count)
    {
        List<ItemSO> rewards = new List<ItemSO>();
        List<ItemSO> tempList = new List<ItemSO>(availableRewards);

        if (tempList.Count == 0)
        {
            Debug.LogError("RewardManager: No available rewards configured!");
            return rewards;
        }

        count = Mathf.Min(count, tempList.Count);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, tempList.Count);
            rewards.Add(tempList[randomIndex]);
            tempList.RemoveAt(randomIndex);
        }

        return rewards;
    }

    /// <summary>
    /// Called from ItemDragManager.LeftRelease() - REQUIRED FOR SNAP-BACK TO WORK
    /// Add this line to ItemDragManager.LeftRelease() after AttachToInventory():
    /// 
    /// if (RewardManager.Instance != null) 
    ///     RewardManager.Instance.OnItemReleased(gameObject);
    /// </summary>
    public void OnItemReleased(GameObject item)
    {
        RewardItemData rewardData = rewardItems.Find(r => r.itemObject == item);
        
        if (rewardData == null)
            return;

        // Stop any ongoing tween
        StopMoveRoutineIfAny(rewardData);

        if (rewardData.itemScript.state == Item.itemState.equipped)
        {
            // Successfully placed in inventory
            Debug.Log("Item placed in inventory! Closing reward panel.");
            rewardData.wasPlacedInInventory = true;
            
            // Remove from tracking
            rewardItems.Remove(rewardData);
            
            // Close panel
            CloseRewardUI();
        }
        else
        {
            // Not placed - snap back to anchor
            Debug.Log("Item not placed, snapping back to anchor point.");
            SnapBackToAnchor(rewardData, tweenDuration);
        }
    }

    /// <summary>
    /// Snaps item back to anchor position with smooth tweening
    /// </summary>
    private void SnapBackToAnchor(RewardItemData rewardData, float duration)
    {
        if (rewardData.itemObject == null)
            return;

        // Ensure item is child of reward container
        if (rewardData.rectTransform.parent != rewardContainer)
        {
            Vector3 currentWorldPos = rewardData.rectTransform.position;
            rewardData.rectTransform.SetParent(rewardContainer);
            rewardData.rectTransform.position = currentWorldPos;
            
            Debug.Log("Item parent restored to reward container");
        }

        // Start tween movement back to anchor
        StopMoveRoutineIfAny(rewardData);
        rewardData.moveRoutine = StartCoroutine(
            MoveToAnchorRoutine(rewardData, rewardData.initialAnchoredPosition, duration)
        );
    }

    private void StopMoveRoutineIfAny(RewardItemData rewardData)
    {
        if (rewardData.moveRoutine != null)
        {
            StopCoroutine(rewardData.moveRoutine);
            rewardData.moveRoutine = null;
        }
    }

    /// <summary>
    /// Coroutine for smooth tweening movement (like reference code)
    /// </summary>
    private IEnumerator MoveToAnchorRoutine(RewardItemData rewardData, Vector2 destination, float duration)
    {
        if (duration > 0.0f)
        {
            float startTime = Time.time;
            Vector2 startPos = rewardData.rectTransform.anchoredPosition;
            float tweenCoeff = 1.0f / duration;

            float dt = 0.0f;
            while (dt < 1.0f)
            {
                dt = (Time.time - startTime) * tweenCoeff;
                float t = EaseOutBack(dt);
                rewardData.rectTransform.anchoredPosition = Vector2.Lerp(startPos, destination, t);
                yield return null;
            }
        }

        // Ensure final position is exact
        rewardData.rectTransform.anchoredPosition = destination;
        
        Debug.Log($"Item snapped back to position {destination}");
        
        rewardData.moveRoutine = null;
    }

    /// <summary>
    /// Ease out back function (similar to DOTween's OutBack)
    /// Creates overshoot effect like in reference code's EaseOutQuad
    /// </summary>
    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    /// <summary>
    /// Alternative: Simple ease out quad (from reference code)
    /// </summary>
    private float EaseOutQuad(float t)
    {
        return 1.0f - (1.0f - t) * (1.0f - t);
    }

    private void ClearRewards()
    {
        foreach (RewardItemData rewardData in rewardItems)
        {
            if (rewardData.itemObject != null)
            {
                StopMoveRoutineIfAny(rewardData);
                
                // Only destroy if NOT equipped
                if (rewardData.itemScript == null || rewardData.itemScript.state != Item.itemState.equipped)
                {
                    Destroy(rewardData.itemObject);
                }
            }
        }

        rewardItems.Clear();

        foreach (RewardSlot slot in rewardSlots)
        {
            slot.itemSO = null;
            if (slot.nameText != null)
            {
                slot.nameText.text = "";
            }
        }
    }

    public void CloseRewardUI()
    {
        Debug.Log("CloseRewardUI called");
        
        for (int i = rewardItems.Count - 1; i >= 0; i--)
        {
            RewardItemData rewardData = rewardItems[i];
            
            if (rewardData.itemObject != null && !rewardData.wasPlacedInInventory)
            {
                StopMoveRoutineIfAny(rewardData);
                
                bool isStillInRewardPanel = rewardData.itemObject.transform.parent == rewardContainer;
                
                if (isStillInRewardPanel)
                {
                    Debug.Log($"Destroying unequipped reward: {rewardData.itemObject.name}");
                    Destroy(rewardData.itemObject);
                }
            }
            
            rewardItems.RemoveAt(i);
        }
        
        foreach (RewardSlot slot in rewardSlots)
        {
            slot.itemSO = null;
            if (slot.nameText != null)
            {
                slot.nameText.text = "";
            }
        }
        
        if (rewardUIPanel != null)
        {
            rewardUIPanel.SetActive(false);
        }

        OnRewardClosed?.Invoke();
    }

    public void OnDeclineButtonClicked()
    {
        Debug.Log("Decline button clicked!");
        CloseRewardUI();
    }

    public void DestroyInstance()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Stop all running coroutines
        foreach (RewardItemData rewardData in rewardItems)
        {
            StopMoveRoutineIfAny(rewardData);
        }
    }
}