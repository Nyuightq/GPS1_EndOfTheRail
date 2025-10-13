// --------------------------------------------------------------
// Creation Date: 2025-10-12
// Author: Liew Zhi Qian
// Description: Handles day/night transitions, UI overlay, and night encounters
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class DayCycleScript : MonoBehaviour
{
    [Header("Debug fields (for viewing only)")]
    [SerializeField] private int tilesMoved = 0;
    [SerializeField] private int day = 0;
    [SerializeField] private TimeState currentTime = TimeState.Day;

    [Header("Cycle Settings")]
    [SerializeField] private int dayLength = 20;
    [SerializeField] private int nightLength = 10;
    [SerializeField] private int dayLengthMod = 0;

    [Header("UI Settings")]
    [SerializeField] private GameObject nightPanel;

    [Header("Event Settings")]
    [SerializeField] private Tilemap eventTilemap;          // ← Assign EventTilemap
    [SerializeField] private EncounterTile encounterTileSO; // ← Assign EncounterTile ScriptableObject
    [SerializeField, Range(0f, 1f)] private float globalSpawnChance = 0.3f;

    private enum TimeState { Day, Night }

    public void setTilesMoved(int val) { tilesMoved = val; }
    public void addTilesMoved(int val) { tilesMoved += val; } 
    public int getTilesMoved() { return tilesMoved; }

    private void Start()
    {
        if (nightPanel != null)
            nightPanel.SetActive(false);
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

                    if (nightPanel != null)
                        nightPanel.SetActive(true);

                    Debug.Log("Night has begun!");
                    SpawnNightEncounters(); // 
                }
                break;

            case TimeState.Night:
                if (tilesMoved >= nightLength)
                {
                    currentTime = TimeState.Day;
                    tilesMoved = 0;
                    day += 1;

                    if (nightPanel != null)
                        nightPanel.SetActive(false);

                    ClearNightEncounters(); // 
                    Debug.Log("Day has begun!");
                }
                break;
        }
    }

    // Spawns EncounterTiles on empty normal rails at night
    // -------------------------------------------------------
    private void SpawnNightEncounters()
    {
        if (eventTilemap == null || encounterTileSO == null)
        {
            Debug.LogWarning("Event Tilemap or EncounterTile not assigned!");
            return;
        }

        RailGridScript grid = FindObjectOfType<RailGridScript>();
        if (grid == null || grid.railDataMap == null)
        {
            Debug.LogWarning("No RailGridScript or rails found.");
            return;
        }

        foreach (var kvp in grid.railDataMap)
        {
            Vector3Int pos = kvp.Key;
            RailData data = kvp.Value;

            // Only on normal rails and skip occupied event tiles (like Rest, End, Combat)
            if (data.railType == RailData.railTypes.normal)
            {
                TileBase currentTile = eventTilemap.GetTile(pos);
                if (currentTile is EventTile) // skip pre-placed EventTiles
                    continue;

                float roll = Random.value;
                float finalChance = encounterTileSO.spawnChance * globalSpawnChance;

                if (roll <= finalChance)
                {
                    eventTilemap.SetTile(pos, encounterTileSO.tileVisual);
                    Debug.Log($"Encounter Tile spawned at {pos}");
                }
            }
        }
    }

    // Clears only EncounterTiles when day returns
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

        Debug.Log($"🧹 Cleared {cleared} EncounterTiles from EventTilemap.");
    }
}
