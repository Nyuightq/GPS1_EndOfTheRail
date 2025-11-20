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
        [Header("Runtime Data (Auto-filled)")]
        public ItemSO itemSO;
        [Header("Anchor Point (Manual-filled)")]
        public RectTransform anchorPoint;
        public TextMeshProUGUI nameText;
    }

    [Header("Reward Configuration")]
    [SerializeField] private List<ItemSO> availableRewards = new List<ItemSO>();
    [SerializeField] private List<RewardSlot> rewardSlots = new List<RewardSlot>(3);

    [Header("Reward UI")]
    [SerializeField] private UI_BaseEventPanel rewardUIPanel;
    [SerializeField] private RectTransform rewardContainer;

    [Header("Tween Settings")]
    [SerializeField] private float tweenDuration = 0.3f;

    [Header("Spawn Bounds")]
    [SerializeField] private bool enforceSpawnBounds = true;

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
            rewardUIPanel.HideEventPanel();
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

        // Get random rewards (limited to number of slots)
        List<ItemSO> randomizedRewards = GetRandomRewards(rewardSlots.Count);

        // Assign to slots AND spawn items in one pass (FIXED)
        for (int i = 0; i < rewardSlots.Count && i < randomizedRewards.Count; i++)
        {
            ItemSO itemData = randomizedRewards[i];
            RewardSlot slot = rewardSlots[i];
            
            // Assign to slot
            slot.itemSO = itemData;
            
            // Update name text
            if (slot.nameText != null)
            {
                slot.nameText.text = itemData.itemName;
            }

            // Spawn item at anchor point
            if (slot.anchorPoint == null)
            {
                Debug.LogError($"RewardManager: Anchor point {i} is null!");
                continue;
            }

            Vector2 spawnPosition = slot.anchorPoint.anchoredPosition;

            // Validate spawn bounds using container's rect
            if (enforceSpawnBounds && !IsPositionInBounds(spawnPosition, rewardContainer))
            {
                Debug.LogWarning($"RewardManager: Anchor {i} position {spawnPosition} is out of bounds! Clamping...");
                spawnPosition = ClampToBounds(spawnPosition, rewardContainer);
            }

            // Instantiate item
            GameObject newItem = Instantiate(itemSpawnPrefab, rewardContainer);
            
            Item itemScript = newItem.GetComponent<Item>();
            if (itemScript != null)
            {
                itemScript.itemData = itemData;
            }

            RectTransform itemRect = newItem.GetComponent<RectTransform>();
            itemRect.anchoredPosition = spawnPosition;

            // Store reward data
            RewardItemData rewardData = new RewardItemData(
                newItem, 
                spawnPosition, 
                slot.anchorPoint
            );
            rewardItems.Add(rewardData);

            Debug.Log($"✓ Spawned {itemData.itemName} at position {spawnPosition}");
        }

        rewardUIPanel.ShowEventPanel();
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
    /// Check if position is within the container's rect bounds
    /// </summary>
    private bool IsPositionInBounds(Vector2 position, RectTransform container)
    {
        if (container == null)
            return true;

        Rect rect = container.rect;
        return position.x >= rect.xMin && position.x <= rect.xMax &&
               position.y >= rect.yMin && position.y <= rect.yMax;
    }

    /// <summary>
    /// Clamp position to container's rect bounds
    /// </summary>
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
        RewardItemData rewardData = rewardItems.Find(r => r.itemObject == item);
        
        if (rewardData == null)
            return;

        StopMoveRoutineIfAny(rewardData);

        if (rewardData.itemScript.state == Item.itemState.equipped)
        {
            Debug.Log("Item placed in inventory! Closing reward panel.");
            rewardData.wasPlacedInInventory = true;
            rewardItems.Remove(rewardData);
            CloseRewardUI();
        }
        else
        {
            Debug.Log("Item not placed, snapping back to anchor point.");
            SnapBackToAnchor(rewardData, tweenDuration);
        }
    }

    private void SnapBackToAnchor(RewardItemData rewardData, float duration)
    {
        if (rewardData.itemObject == null)
            return;

        if (rewardData.rectTransform.parent != rewardContainer)
        {
            Vector3 currentWorldPos = rewardData.rectTransform.position;
            rewardData.rectTransform.SetParent(rewardContainer);
            rewardData.rectTransform.position = currentWorldPos;
            
            Debug.Log("Item parent restored to reward container");
        }

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

        rewardData.rectTransform.anchoredPosition = destination;
        Debug.Log($"Item snapped back to position {destination}");
        rewardData.moveRoutine = null;
    }

    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private void ClearRewards()
    {
        foreach (RewardItemData rewardData in rewardItems)
        {
            if (rewardData.itemObject != null)
            {
                StopMoveRoutineIfAny(rewardData);
                
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
        
        SoundManager.Instance.PlaySFX("SFX_ButtonOnCancel");
        rewardUIPanel.HideEventPanel(()=> OnRewardClosed?.Invoke());
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
        foreach (RewardItemData rewardData in rewardItems)
        {
            StopMoveRoutineIfAny(rewardData);
        }
    }
}