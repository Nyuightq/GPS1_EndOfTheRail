using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EngineerManager : MonoBehaviour
{
    public static EngineerManager Instance { get; private set; }
    public static event System.Action OnEngineerClosed;

    [Header("Engineer UI")]
    [SerializeField] private GameObject engineerUIPanel;

    [Header("Merge UI Boxes")]
    [SerializeField] private RectTransform boxA;       // Empty UI box A
    [SerializeField] private RectTransform boxB;       // Empty UI box B
    [SerializeField] private Image boxAImage;          // Visual feedback for Box A
    [SerializeField] private Image boxBImage;          // Visual feedback for Box B

    [Header("Box Visual Settings")]
    [SerializeField] private Color emptyBoxColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
    [SerializeField] private Color filledBoxColor = new Color(0.3f, 0.8f, 0.3f, 0.8f);
    [SerializeField] private Color invalidBoxColor = new Color(0.8f, 0.3f, 0.3f, 0.8f);

    [Header("Tween Settings")]
    [SerializeField] private float tweenDuration = 0.3f;

    private ItemSO itemA;       // Stored Item from Box A
    private ItemSO itemB;       // Stored Item from Box B

    private MergeBoxData boxAData;
    private MergeBoxData boxBData;

    public bool IsMergeValid { get; private set; } = false;

    private class MergeBoxData
    {
        public GameObject itemObject;
        public RectTransform rectTransform;
        public Item itemScript;
        public Vector2 initialAnchoredPosition;
        public RectTransform boxRect;
        public Coroutine moveRoutine;

        public MergeBoxData(GameObject obj, Vector2 initialPos, RectTransform box)
        {
            itemObject = obj;
            rectTransform = obj?.GetComponent<RectTransform>();
            itemScript = obj?.GetComponent<Item>();
            initialAnchoredPosition = initialPos;
            boxRect = box;
            moveRoutine = null;
        }

        public void Clear()
        {
            itemObject = null;
            rectTransform = null;
            itemScript = null;
            moveRoutine = null;
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
    }

    private void Start()
    {
        if (engineerUIPanel != null)
        {
            engineerUIPanel.SetActive(false);
        }

        UpdateBoxVisuals();
    }

    public void OpenEngineerUI(GameObject player = null)
    {
        ClearBoxes();
        engineerUIPanel.SetActive(true);
    }

    public void CloseEngineerUI()
    {
        ClearBoxes();
        engineerUIPanel.SetActive(false);
        OnEngineerClosed?.Invoke();
    }

    /// <summary>
    /// Called when an item is released - check if it's over a merge box
    /// </summary>
    public void OnItemReleased(GameObject item)
    {
        if (item == null) return;

        Item itemScript = item.GetComponent<Item>();
        RectTransform itemRect = item.GetComponent<RectTransform>();
        
        if (itemScript == null || itemRect == null) return;

        // Check which box (if any) the item is over
        bool isOverBoxA = IsOverBox(itemRect, boxA);
        bool isOverBoxB = IsOverBox(itemRect, boxB);

        if (isOverBoxA && boxAData == null)
        {
            PlaceItemInBox(item, itemScript, boxA, ref boxAData, ref itemA);
        }
        else if (isOverBoxB && boxBData == null)
        {
            PlaceItemInBox(item, itemScript, boxB, ref boxBData, ref itemB);
        }
        else
        {
            // Item not placed in a valid box - let it return to wherever it came from
            Debug.Log("Item not placed in merge box");
        }
    }

    /// <summary>
    /// Check if item's position overlaps with a box
    /// </summary>
    private bool IsOverBox(RectTransform itemRect, RectTransform boxRect)
    {
        if (itemRect == null || boxRect == null) return false;

        Vector3[] itemCorners = new Vector3[4];
        itemRect.GetWorldCorners(itemCorners);
        
        Vector3[] boxCorners = new Vector3[4];
        boxRect.GetWorldCorners(boxCorners);

        // Simple overlap check using center points
        Vector2 itemCenter = (itemCorners[0] + itemCorners[2]) / 2f;
        
        return itemCenter.x >= boxCorners[0].x && itemCenter.x <= boxCorners[2].x &&
               itemCenter.y >= boxCorners[0].y && itemCenter.y <= boxCorners[2].y;
    }

    /// <summary>
    /// Place an item into a merge box
    /// </summary>
    private void PlaceItemInBox(GameObject item, Item itemScript, RectTransform box, 
                                ref MergeBoxData boxData, ref ItemSO itemSO)
    {
        if (itemScript.itemData == null)
        {
            Debug.LogError("Item has no ItemSO data!");
            return;
        }

        // Store item data
        itemSO = itemScript.itemData;
        
        // Reparent to box
        RectTransform itemRect = item.GetComponent<RectTransform>();
        Vector3 currentWorldPos = itemRect.position;
        itemRect.SetParent(box);
        itemRect.position = currentWorldPos;

        // Create box data
        boxData = new MergeBoxData(item, Vector2.zero, box);

        // Animate to center of box
        StopMoveRoutineIfAny(boxData);
        boxData.moveRoutine = StartCoroutine(
            MoveToBoxCenterRoutine(boxData, Vector2.zero, tweenDuration)
        );

        Debug.Log($"Placed {itemSO.itemName} in merge box");

        // Check merge validity
        CheckMergeValidity();
        UpdateBoxVisuals();
    }

    /// <summary>
    /// Remove an item from a specific box
    /// </summary>
    public void RemoveItemFromBox(bool isBoxA)
    {
        if (isBoxA && boxAData != null)
        {
            StopMoveRoutineIfAny(boxAData);
            if (boxAData.itemObject != null)
            {
                // Don't destroy - let inventory system handle it
                boxAData.itemObject.transform.SetParent(null);
            }
            boxAData.Clear();
            boxAData = null;
            itemA = null;
        }
        else if (!isBoxA && boxBData != null)
        {
            StopMoveRoutineIfAny(boxBData);
            if (boxBData.itemObject != null)
            {
                boxBData.itemObject.transform.SetParent(null);
            }
            boxBData.Clear();
            boxBData = null;
            itemB = null;
        }

        CheckMergeValidity();
        UpdateBoxVisuals();
    }

    /// <summary>
    /// Checks whether the two items are the same
    /// </summary>
    private void CheckMergeValidity()
    {
        IsMergeValid = false;

        if (itemA == null || itemB == null)
        {
            Debug.Log("Merge invalid: One or both boxes empty");
            return;
        }

        // Compare item names
        if (itemA.itemName == itemB.itemName)
        {
            IsMergeValid = true;
            Debug.Log($"Merge valid! Both items are: {itemA.itemName}");
        }
        else
        {
            Debug.Log($"Merge invalid: {itemA.itemName} != {itemB.itemName}");
        }

        UpdateBoxVisuals();
    }

    /// <summary>
    /// Update box visual feedback based on state
    /// </summary>
    private void UpdateBoxVisuals()
    {
        // Update Box A
        if (boxAImage != null)
        {
            if (itemA == null)
            {
                boxAImage.color = emptyBoxColor;
            }
            else if (IsMergeValid)
            {
                boxAImage.color = filledBoxColor;
            }
            else if (itemB != null) // Both filled but invalid
            {
                boxAImage.color = invalidBoxColor;
            }
            else
            {
                boxAImage.color = filledBoxColor;
            }
        }

        // Update Box B
        if (boxBImage != null)
        {
            if (itemB == null)
            {
                boxBImage.color = emptyBoxColor;
            }
            else if (IsMergeValid)
            {
                boxBImage.color = filledBoxColor;
            }
            else if (itemA != null) // Both filled but invalid
            {
                boxBImage.color = invalidBoxColor;
            }
            else
            {
                boxBImage.color = filledBoxColor;
            }
        }
    }

    /// <summary>
    /// Clears both merge boxes
    /// </summary>
    public void ClearBoxes()
    {
        // Clear Box A
        if (boxAData != null)
        {
            StopMoveRoutineIfAny(boxAData);
            if (boxAData.itemObject != null)
            {
                Destroy(boxAData.itemObject);
            }
            boxAData.Clear();
            boxAData = null;
        }

        // Clear Box B
        if (boxBData != null)
        {
            StopMoveRoutineIfAny(boxBData);
            if (boxBData.itemObject != null)
            {
                Destroy(boxBData.itemObject);
            }
            boxBData.Clear();
            boxBData = null;
        }

        // Clear children (fallback cleanup)
        foreach (Transform child in boxA)
            Destroy(child.gameObject);

        foreach (Transform child in boxB)
            Destroy(child.gameObject);

        itemA = null;
        itemB = null;
        IsMergeValid = false;

        UpdateBoxVisuals();
    }

    private void StopMoveRoutineIfAny(MergeBoxData boxData)
    {
        if (boxData?.moveRoutine != null)
        {
            StopCoroutine(boxData.moveRoutine);
            boxData.moveRoutine = null;
        }
    }

    private IEnumerator MoveToBoxCenterRoutine(MergeBoxData boxData, Vector2 destination, float duration)
    {
        if (boxData.rectTransform == null) yield break;

        if (duration > 0.0f)
        {
            float startTime = Time.time;
            Vector2 startPos = boxData.rectTransform.anchoredPosition;
            float tweenCoeff = 1.0f / duration;

            float dt = 0.0f;
            while (dt < 1.0f)
            {
                if (boxData.rectTransform == null) yield break;

                dt = (Time.time - startTime) * tweenCoeff;
                float t = EaseOutBack(dt);
                boxData.rectTransform.anchoredPosition = Vector2.Lerp(startPos, destination, t);
                yield return null;
            }
        }

        if (boxData.rectTransform != null)
        {
            boxData.rectTransform.anchoredPosition = destination;
        }

        boxData.moveRoutine = null;
    }

    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private void OnDestroy()
    {
        if (boxAData != null)
            StopMoveRoutineIfAny(boxAData);
        
        if (boxBData != null)
            StopMoveRoutineIfAny(boxBData);
    }
}