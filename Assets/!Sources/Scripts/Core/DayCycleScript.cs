// --------------------------------------------------------------
// Creation Date: 2025-10-12
// Modified: 2025-11-23
// Author: -
// Description: Handles day/night transitions, UI overlay, and night encounters
//              Now uses EnemyNumberEncounterData to determine spawn counts
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class DayCycleScript : MonoBehaviour
{
    [Header("Debug fields (for viewing only)")]
    [SerializeField] private int tilesMoved = 0;
    [SerializeField] private int day = 0;
    [SerializeField] private TimeState currentTime = TimeState.Day;
    [SerializeField] private int totalTilesMovedAllTime = 0;

    [Header("Cycle Settings")]
    [SerializeField] private int dayLength = 20;
    [SerializeField] private int nightLength = 10;
    [SerializeField] private int dayLengthMod = 0;

    [Header("Event Settings")]
    [SerializeField] private Tilemap eventTilemap;
    [SerializeField] private EncounterTile encounterTileSO;
    
    [Header("Enemy Encounter Data")]
    [SerializeField] private EnemyNumberEncounterData enemyEncounterData;
    
    public int GetDay() => day;

    [SerializeField] private Transform playerTrain;

    [Header("UI Image Swap (Day / Night)")]
    [SerializeField] private RectTransform uiImageA;
    [SerializeField] private RectTransform uiImageB;

    private Vector3 uiA_DayPos;
    private Vector3 uiB_DayPos;

    public enum TimeState { Day, Night }
    public bool IsDayTime => currentTime == TimeState.Day;
    
    [Header("Public getters for Day Progress Slider")]
    public int TilesMoved => tilesMoved;
    public int DayLength => dayLength;
    public int DayLengthMod => dayLengthMod;
    public int NightLength => nightLength;
    public TimeState CurrentTime => currentTime;

    public void setTilesMoved(int val) 
    { 
        tilesMoved = Mathf.Max(0, val); // Prevent negative values
    }
    
public void addTilesMoved(int val) 
{ 
    tilesMoved += val;
    tilesMoved = Mathf.Max(0, tilesMoved);
    
    // ADD THESE LINES
    totalTilesMovedAllTime += val;
    totalTilesMovedAllTime = Mathf.Max(0, totalTilesMovedAllTime);
}
    
    public int getTilesMoved() { return tilesMoved; }

    private void Start()
    {
        if (uiImageA != null) uiA_DayPos = uiImageA.anchoredPosition;
        if (uiImageB != null) uiB_DayPos = uiImageB.anchoredPosition;
        
        if (enemyEncounterData == null)
        {
            Debug.LogWarning("[DayCycleScript] EnemyNumberEncounterData not assigned! Please assign it in the inspector.");
        }
    }

    public int GetTotalTilesMovedAllTime() 
    { 
        return totalTilesMovedAllTime; 
    }

    private void Update()
    {
        switch (currentTime)
        {
            case TimeState.Day:
                if (tilesMoved >= dayLength + dayLengthMod)
                {
                    currentTime = TimeState.Night;
                    tilesMoved = 0;
                    day++; // Increment day counter

                    Debug.Log($"Night has begun! Day {day} completed.");
                    
                    SoundManager.Instance.PlaySFX("SFX_Travel_OnSwitchNight");
                    StartCoroutine(SpawnEncountersWithDelay(0.05f));
                    SwapUIPositions();
                }
                break;
                
            case TimeState.Night:
                if (tilesMoved >= nightLength)
                {
                    currentTime = TimeState.Day;
                    tilesMoved = 0;

                    Debug.Log($"Day has begun! Starting day {day + 1}.");
                    
                    ClearNightEncounters();
                    RestoreUIPositions();

                    foreach (GameObject item in GameManager.instance.inventoryScript.equippedItems)
                    {
                        item.GetComponent<Item>().TriggerEffectDayStart();
                    }
                }
                break;
        }
    }

    private IEnumerator SpawnEncountersWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnNightEncounters();
    }

    private void SpawnNightEncounters()
    {
        if (eventTilemap == null || encounterTileSO == null)
        {
            Debug.LogWarning("Event Tilemap or EncounterTile not assigned!");
            return;
        }

        RailGridScript grid = FindFirstObjectByType<RailGridScript>();
        if (grid == null || grid.railDataMap == null)
        {
            Debug.LogWarning("No RailGridScript or rails found.");
            return;
        }

        TrainMovement train = null;
        if (playerTrain != null) train = playerTrain.GetComponent<TrainMovement>();
        if (train == null) train = FindFirstObjectByType<TrainMovement>();

        if (train == null)
        {
            Debug.LogWarning("Train not found - spawning globally (no ahead filter).");
            SpawnEncountersOnTiles(grid);
            return;
        }

        Vector3Int trainTile = train.GetTilePos();
        Vector2 forward = train.GetForwardDirection();
        if (forward == Vector2.zero) forward = Vector2.right;

        const int stepsAhead = 8;
        const int lateralRange = 1;

        HashSet<Vector3Int> aheadTiles = new HashSet<Vector3Int>();

        Vector3Int current = trainTile;
        Vector2 normalizedForward = forward.normalized;

        for (int i = 0; i < stepsAhead; i++)
        {
            Vector3Int next = current + new Vector3Int(Mathf.RoundToInt(normalizedForward.x), Mathf.RoundToInt(normalizedForward.y), 0);

            if (!grid.railAtPos(next)) break;

            aheadTiles.Add(next);

            if (lateralRange > 0)
            {
                List<Vector3Int> adj = grid.getAdjacentTiles(next);
                foreach (var a in adj)
                {
                    Vector2 dirToAdj = new Vector2(a.x - trainTile.x, a.y - trainTile.y);
                    if (Vector2.Dot(dirToAdj, normalizedForward) > 0.1f && grid.railAtPos(a))
                        aheadTiles.Add(a);
                }
            }

            current = next;
        }

        if (aheadTiles.Count == 0)
        {
            Debug.LogWarning("No ahead tiles found; falling back to global spawn.");
            SpawnEncountersOnTiles(grid);
            return;
        }

        // Determine how many encounters to spawn based on the ScriptableObject data
        int targetEncounterCount = GetTargetEncounterCount();
        
        // Convert HashSet to List for easier random selection
        List<Vector3Int> availableTiles = new List<Vector3Int>();
        foreach (var pos in aheadTiles)
        {
            if (!grid.railAtPos(pos)) continue;
            RailData data = grid.railDataMap[pos];
            if (data.railType != RailData.railTypes.normal) continue;

            TileBase currentTile = eventTilemap.GetTile(pos);
            if (currentTile is EventTile) continue;

            availableTiles.Add(pos);
        }

        if (availableTiles.Count == 0)
        {
            Debug.LogWarning("No valid tiles available for encounter spawning.");
            return;
        }

        // Spawn the exact number of encounters determined by the data
        int spawned = 0;
        int attemptsLeft = Mathf.Min(targetEncounterCount, availableTiles.Count);
        
        while (spawned < targetEncounterCount && availableTiles.Count > 0 && attemptsLeft > 0)
        {
            int randomIndex = Random.Range(0, availableTiles.Count);
            Vector3Int pos = availableTiles[randomIndex];
            
            eventTilemap.SetTile(pos, encounterTileSO.tileVisual);
            spawned++;
            
            // Remove this tile from available tiles to avoid duplicates
            availableTiles.RemoveAt(randomIndex);
            attemptsLeft--;
        }

        Debug.Log($"SpawnNightEncounters (Day {day}): spawned {spawned}/{targetEncounterCount} encounters on {aheadTiles.Count} tiles ahead.");
    }

    private void SpawnEncountersOnTiles(RailGridScript grid)
    {
        // Determine how many encounters to spawn
        int targetEncounterCount = GetTargetEncounterCount();
        
        // Collect all valid tiles
        List<Vector3Int> availableTiles = new List<Vector3Int>();
        foreach (var kvp in grid.railDataMap)
        {
            Vector3Int pos = kvp.Key;
            RailData data = kvp.Value;
            if (data.railType == RailData.railTypes.normal)
            {
                TileBase currentTile = eventTilemap.GetTile(pos);
                if (currentTile is EventTile) continue;

                availableTiles.Add(pos);
            }
        }

        if (availableTiles.Count == 0)
        {
            Debug.LogWarning("No valid tiles available for encounter spawning.");
            return;
        }

        // Spawn encounters up to the target count
        int spawned = 0;
        int attemptsLeft = Mathf.Min(targetEncounterCount, availableTiles.Count);
        
        while (spawned < targetEncounterCount && availableTiles.Count > 0 && attemptsLeft > 0)
        {
            int randomIndex = Random.Range(0, availableTiles.Count);
            Vector3Int pos = availableTiles[randomIndex];
            
            eventTilemap.SetTile(pos, encounterTileSO.tileVisual);
            spawned++;
            
            availableTiles.RemoveAt(randomIndex);
            attemptsLeft--;
        }

        Debug.Log($"SpawnEncountersOnTiles (Day {day}): Spawned {spawned}/{targetEncounterCount} globally.");
    }

    /// <summary>
    /// Gets the target number of encounters to spawn based on the current day and ScriptableObject data.
    /// </summary>
    private int GetTargetEncounterCount()
    {
        if (enemyEncounterData == null)
        {
            Debug.LogWarning("[DayCycleScript] EnemyEncounterData is null, defaulting to 2 encounters.");
            return 2;
        }

        int count = enemyEncounterData.GetRandomEnemyCount(day);
        Debug.Log($"[DayCycleScript] Day {day}: Target encounter count = {count}");
        return count;
    }

    private void ClearNightEncounters()
    {
        if (eventTilemap == null || encounterTileSO == null) return;

        BoundsInt bounds = eventTilemap.cellBounds;
        int cleared = 0;

        foreach (var pos in bounds.allPositionsWithin)
        {
            TileBase tile = eventTilemap.GetTile(pos);
            if (tile == encounterTileSO.tileVisual)
            {
                eventTilemap.SetTile(pos, null);
                cleared++;
            }
        }

        Debug.Log($"Cleared {cleared} EncounterTiles from EventTilemap.");
    }
    
    private void SwapUIPositions()
    {
        if (uiImageA == null || uiImageB == null) return;

        Vector3 temp = uiImageA.anchoredPosition;
        uiImageA.anchoredPosition = uiImageB.anchoredPosition;
        uiImageB.anchoredPosition = temp;
    }

    private void RestoreUIPositions()
    {
        if (uiImageA == null || uiImageB == null) return;

        uiImageA.anchoredPosition = uiA_DayPos;
        uiImageB.anchoredPosition = uiB_DayPos;
    }
}