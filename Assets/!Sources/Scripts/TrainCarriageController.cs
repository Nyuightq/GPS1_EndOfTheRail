// --------------------------------------------------------------
// Creation Date: 2025-11-27
// Author: Assistant
// Description: Controls train carriages with strict 1-tile grid spacing
// --------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

public class TrainCarriageController : MonoBehaviour
{
    [Header("Carriage Settings")]
    [SerializeField] private GameObject carriagePrefab;
    [SerializeField] private int maxCarriages = 3;
    
    [Header("Inventory-Based Spawning")]
    [SerializeField] private InventoryGridScript inventoryGrid;
    [SerializeField] private int inventorySlotsPerCarriage = 12;
    private int lastInventorySize = 0;
    
    [Header("References")]
    private TrainMovement trainMovement;
    private RailGridScript gridScript;
    
    // Store carriage game objects
    private List<GameObject> carriages = new List<GameObject>();
    
    // Store complete position history (world positions recorded frequently)
    private List<Vector3> positionHistory = new List<Vector3>();
    
    // How many positions to keep in history
    private int maxHistoryLength = 500;
    
    // Tile spacing in world units (assumes 1 unit tiles)
    // Increased slightly to prevent overlap
    private float tileSpacing = 1.2f;
    
    private void Awake()
    {
        trainMovement = GetComponent<TrainMovement>();
        if (trainMovement == null)
        {
            Debug.LogError("TrainMovement component not found!");
        }
    }
    
    private void Start()
    {
        // Get grid reference from TrainMovement
        if (trainMovement != null && trainMovement.gridManager != null)
        {
            gridScript = trainMovement.gridManager.GetComponent<RailGridScript>();
        }
        
        // Find inventory grid if not assigned
        if (inventoryGrid == null)
        {
            inventoryGrid = FindFirstObjectByType<InventoryGridScript>();
            if (inventoryGrid == null)
            {
                Debug.LogError("InventoryGridScript not found! Carriages won't spawn based on inventory.");
            }
        }
        
        // Initialize position history with current position
        positionHistory.Add(transform.position);
        
        // Calculate initial inventory size (count ACTIVE cells)
        if (inventoryGrid != null)
        {
            lastInventorySize = CountActiveCells();
            Debug.Log($"[TrainCarriageController] Initial active inventory cells: {lastInventorySize}");
        }
        
        // Don't spawn any carriages at start - they'll spawn as inventory grows
    }
    
    /// <summary>
    /// Checks inventory size and spawns carriages based on growth
    /// </summary>
    private void CheckInventorySizeAndSpawnCarriages()
    {
        if (inventoryGrid == null) return;
        
        // Count ACTIVE cells instead of just dimensions
        int currentInventorySize = CountActiveCells();
        
        // Debug log to track changes
        if (currentInventorySize != lastInventorySize)
        {
            Debug.Log($"[TrainCarriageController] Active inventory cells changed: {lastInventorySize} -> {currentInventorySize}");
        }
        
        // Check if inventory size increased
        if (currentInventorySize > lastInventorySize)
        {
            int slotsAdded = currentInventorySize - lastInventorySize;
            lastInventorySize = currentInventorySize;
            
            // Calculate how many carriages should exist based on total inventory size
            int baseInventorySize = 20; // Initial size (adjust this to match your actual initial inventory)
            int additionalSlots = currentInventorySize - baseInventorySize;
            int targetCarriageCount = Mathf.Min(additionalSlots / inventorySlotsPerCarriage, maxCarriages);
            
            Debug.Log($"[TrainCarriageController] Additional slots: {additionalSlots}, Target carriages: {targetCarriageCount}, Current carriages: {carriages.Count}");
            
            // Spawn carriages if we need more
            while (carriages.Count < targetCarriageCount)
            {
                SpawnSingleCarriage();
                Debug.Log($"[TrainCarriageController] ✓ Carriage spawned! Total carriages: {carriages.Count}/{maxCarriages}");
            }
        }
    }
    
    /// <summary>
    /// Spawns a single carriage at the correct position
    /// </summary>
    private void SpawnSingleCarriage()
    {
        if (carriagePrefab == null)
        {
            Debug.LogError("Carriage prefab not assigned!");
            return;
        }
        
        if (gridScript == null)
        {
            Debug.LogError("Grid script not found!");
            return;
        }
        
        int carriageIndex = carriages.Count;
        
        // Calculate spawn position based on train's current position and existing carriages
        Vector3 spawnPos;
        
        if (carriageIndex == 0)
        {
            // First carriage spawns behind the train
            Vector3Int currentTile = trainMovement.GetTilePos();
            Vector2 backwardDirection = -trainMovement.GetForwardDirection();
            Vector3Int carriageTile = currentTile + Vector3Int.RoundToInt((Vector3)(backwardDirection * 1));
            spawnPos = gridScript.snapToGrid(carriageTile);
        }
        else
        {
            // Subsequent carriages spawn behind the last carriage
            Vector3 lastCarriagePos = carriages[carriageIndex - 1].transform.position;
            Vector2 backwardDirection = -trainMovement.GetForwardDirection();
            spawnPos = lastCarriagePos + (Vector3)(backwardDirection * tileSpacing);
            spawnPos = gridScript.snapToGrid(Vector3Int.RoundToInt(spawnPos));
        }
        
        // Instantiate carriage
        GameObject carriage = Instantiate(carriagePrefab, spawnPos, Quaternion.identity);
        carriage.transform.parent = transform.parent;
        carriage.name = $"Carriage_{carriageIndex + 1}";
        
        // Get or add animation controller
        TrainAnimationController animController = carriage.GetComponent<TrainAnimationController>();
        if (animController == null)
        {
            animController = carriage.AddComponent<TrainAnimationController>();
        }
        
        // Set initial direction
        animController.UpdateDirection(trainMovement.GetForwardDirection());
        
        carriages.Add(carriage);
        
        // Add spawn position to history
        positionHistory.Add(spawnPos);
        
        Debug.Log($"[TrainCarriageController] Carriage spawned at position: {spawnPos}");
    }
    
    private void LateUpdate()
    {
        if (trainMovement == null) return;
        
        // Check if inventory size has changed and spawn carriages accordingly
        CheckInventorySizeAndSpawnCarriages();
        
        if (carriages.Count == 0) return;
        
        // Record train's current position every frame
        Vector3 currentTrainPos = transform.position;
        
        // Only add to history if train has moved a minimum distance (reduces redundant positions)
        if (positionHistory.Count == 0 || Vector3.Distance(currentTrainPos, positionHistory[0]) > 0.01f)
        {
            positionHistory.Insert(0, currentTrainPos);
        }
        
        // Limit history size
        if (positionHistory.Count > maxHistoryLength)
        {
            positionHistory.RemoveRange(maxHistoryLength, positionHistory.Count - maxHistoryLength);
        }
        
        // Update carriage positions based on history
        UpdateCarriagePositions();
    }
    
    /// <summary>
    /// Updates each carriage to follow the train's path at exact tile spacing
    /// </summary>
    private void UpdateCarriagePositions()
    {
        for (int i = 0; i < carriages.Count; i++)
        {
            if (carriages[i] == null)
            {
                Debug.LogWarning($"Carriage {i} is null!");
                continue;
            }
            
            // Calculate the target distance behind the train
            float targetDistance = tileSpacing * (i + 1);
            
            // Find the position in history that is exactly targetDistance away
            Vector3 targetPos = FindPositionAtDistance(targetDistance);
            
            // Update carriage position
            carriages[i].transform.position = targetPos;
            
            // Update sprite direction
            UpdateCarriageDirection(i, targetPos);
        }
    }
    
    /// <summary>
    /// Finds a position in the path history at a specific distance from the train
    /// </summary>
    private Vector3 FindPositionAtDistance(float targetDistance)
    {
        if (positionHistory.Count < 2)
            return positionHistory[0];
        
        float accumulatedDistance = 0f;
        
        // Walk through the position history
        for (int i = 0; i < positionHistory.Count - 1; i++)
        {
            Vector3 currentPos = positionHistory[i];
            Vector3 nextPos = positionHistory[i + 1];
            
            float segmentLength = Vector3.Distance(currentPos, nextPos);
            
            // Check if target distance falls within this segment
            if (accumulatedDistance + segmentLength >= targetDistance)
            {
                // Interpolate between currentPos and nextPos
                float remainingDistance = targetDistance - accumulatedDistance;
                float t = remainingDistance / segmentLength;
                
                return Vector3.Lerp(currentPos, nextPos, t);
            }
            
            accumulatedDistance += segmentLength;
        }
        
        // If we've walked through entire history, return last position
        return positionHistory[positionHistory.Count - 1];
    }
    
    /// <summary>
    /// Updates the visual direction of a carriage based on its movement
    /// </summary>
    private void UpdateCarriageDirection(int carriageIndex, Vector3 currentPos)
    {
        TrainAnimationController animController = carriages[carriageIndex].GetComponent<TrainAnimationController>();
        if (animController == null) return;
        
        // Calculate target distance for this carriage
        float targetDistance = tileSpacing * (carriageIndex + 1);
        
        // Find a position slightly CLOSER to the train (ahead in the path)
        float aheadDistance = targetDistance - 0.2f;
        if (aheadDistance < 0) aheadDistance = 0;
        
        Vector3 aheadPos = FindPositionAtDistance(aheadDistance);
        
        // Calculate direction: FROM behind position TO current position (forward direction)
        // This is the direction the carriage is traveling
        Vector2 direction = (aheadPos - currentPos).normalized;
        
        // Only update if direction is significant
        if (direction.magnitude > 0.1f)
        {
            animController.UpdateDirection(direction);
        }
    }
    
    /// <summary>
    /// Removes all carriages (useful for cleanup or reset)
    /// </summary>
    public void RemoveAllCarriages()
    {
        foreach (GameObject carriage in carriages)
        {
            if (carriage != null)
            {
                Destroy(carriage);
            }
        }
        
        carriages.Clear();
        positionHistory.Clear();
        Debug.Log("All carriages removed");
    }
    
    /// <summary>
    /// Gets the current number of carriages that should exist based on inventory size
    /// </summary>
    public int GetTargetCarriageCount()
    {
        if (inventoryGrid == null) return 0;
        
        int currentInventorySize = CountActiveCells();
        int baseInventorySize = 20;
        int additionalSlots = currentInventorySize - baseInventorySize;
        return Mathf.Min(additionalSlots / inventorySlotsPerCarriage, maxCarriages);
    }
    
    /// <summary>
    /// Counts the number of ACTIVE cells in the inventory grid
    /// </summary>
    private int CountActiveCells()
    {
        if (inventoryGrid == null || inventoryGrid.inventoryGrid == null)
            return 0;
            
        int activeCells = 0;
        foreach (var cell in inventoryGrid.inventoryGrid)
        {
            if (cell != null && cell.active)
            {
                activeCells++;
            }
        }
        return activeCells;
    }
    
    /// <summary>
    /// Gets a specific carriage by index
    /// </summary>
    public GameObject GetCarriage(int index)
    {
        if (index >= 0 && index < carriages.Count)
        {
            return carriages[index];
        }
        return null;
    }
    
    /// <summary>
    /// Manual trigger to check and spawn carriages (useful for debugging)
    /// </summary>
    public void ForceCheckCarriages()
    {
        CheckInventorySizeAndSpawnCarriages();
    }
    
    private void OnDestroy()
    {
        // Clean up carriages when train is destroyed
        RemoveAllCarriages();
    }
}