// --------------------------------------------------------------
// Creation Date: 2025-10-13 00:03
// Modified: 2025-11-23
// Author: ZQlie
// Description: Event tile that triggers combat encounters
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Custom Tiles/Encounter Tile")]
public class EncounterTile : EventTile
{
    [Header("Visuals")]
    public Tile tileVisual; // Drag a Tile asset for this Encounter tile

    private static Tilemap eventTilemap;
    private static Vector3Int? lastEncounterTilePos;

    public override void OnPlayerEnter(GameObject player)
    {
        Debug.Log("Player stepped on an Encounter Tile!");

        // Find and cache EventTilemap once
        if (eventTilemap == null)
        {
            GameObject tilemapObj = GameObject.Find("EventTilemap");
            if (tilemapObj != null)
                eventTilemap = tilemapObj.GetComponent<Tilemap>();
        }

        // Snap train to the exact center of this tile
        if (eventTilemap != null)
        {
            Vector3Int tilePos = eventTilemap.WorldToCell(player.transform.position);
            Vector3 centerPos = eventTilemap.GetCellCenterWorld(tilePos);
            player.transform.position = centerPos;

            lastEncounterTilePos = tilePos;

            Debug.Log($"[EncounterTile] Snapped train to center at {tilePos}");
        }

        // Freeze train AFTER snapping to center
        TrainFreezeController freezeController = player.GetComponent<TrainFreezeController>();
        if (freezeController == null)
            freezeController = FindFirstObjectByType<TrainFreezeController>();

        if (freezeController != null)
            freezeController.FreezeTrain();
        else
            Debug.LogWarning("[EncounterTile] No TrainFreezeController found on player or scene.");

        // Start combat
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.StartCombat(CombatManager.CombatType.Encounter);

            // Subscribe once to combat end event
            CombatManager.OnCombatClosed -= DeleteEncounterTile;
            CombatManager.OnCombatClosed += DeleteEncounterTile;
        }
        else
        {
            Debug.LogWarning("[EncounterTile] No CombatManager found in scene!");
        }
    }

    public override void OnPlayerExit(GameObject player)
    {
        // Extra safety: if the player somehow leaves early, delete the tile
        DeleteEncounterTile();
    }

    private static void DeleteEncounterTile()
    {
        if (eventTilemap != null && lastEncounterTilePos.HasValue)
        {
            Vector3Int pos = lastEncounterTilePos.Value;

            if (eventTilemap.GetTile(pos) != null)
            {
                eventTilemap.SetTile(pos, null);
                Debug.Log($"[EncounterTile] Deleted Encounter Tile at {pos}");
            }
            else
            {
                Debug.Log($"[EncounterTile] Tried to delete Encounter Tile at {pos}, but no tile found.");
            }

            lastEncounterTilePos = null;
        }

        // Unsubscribe to prevent repeated deletion
        CombatManager.OnCombatClosed -= DeleteEncounterTile;
    }
}