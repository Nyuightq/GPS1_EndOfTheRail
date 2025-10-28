using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Custom Tiles/Combat Tile")]
public class CombatTile : EventTile
{
    [Header("Visuals")]
    public Tile tileVisual; // Assign a Tile asset in the Inspector


    private static Tilemap eventTilemap;
    private static Vector3Int? lastCombatTilePos;

    public override void OnPlayerEnter(GameObject player)
    {
        Debug.Log("Player stepped on a Combat Tile!");

        // Find and cache EventTilemap
        if (eventTilemap == null)
        {
            GameObject tilemapObj = GameObject.Find("EventTilemap");
            if (tilemapObj != null)
                eventTilemap = tilemapObj.GetComponent<Tilemap>();
        }

        // Snap player (train) to tile center immediately
        if (eventTilemap != null)
        {
            Vector3Int tilePos = eventTilemap.WorldToCell(player.transform.position);
            Vector3 centerPos = eventTilemap.GetCellCenterWorld(tilePos);
            player.transform.position = centerPos;

            lastCombatTilePos = tilePos;
            Debug.Log($"[CombatTile] Snapped train to center at {tilePos}");
        }

        // Freeze train using TrainFreezeController AFTER snapping to center
        TrainFreezeController freezeController = player.GetComponent<TrainFreezeController>();
        if (freezeController == null)
            freezeController = Object.FindFirstObjectByType<TrainFreezeController>();

        if (freezeController != null)
            freezeController.FreezeTrain();
        else
            Debug.LogWarning("[CombatTile] No TrainFreezeController found!");

        // Start combat
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.StartCombat();

            // Subscribe to combat close event once
            CombatManager.OnCombatClosed -= DeleteCombatTile;
            CombatManager.OnCombatClosed += DeleteCombatTile;
        }
        else
        {
            Debug.LogWarning("[CombatTile] No CombatManager found in scene!");
        }
    }

    public override void OnPlayerExit(GameObject player)
    {
        // Optional safeguard: if somehow player exits before combat ends, delete the tile
        DeleteCombatTile();
    }

    private static void DeleteCombatTile()
    {
        if (eventTilemap != null && lastCombatTilePos.HasValue)
        {
            Vector3Int pos = lastCombatTilePos.Value;
            if (eventTilemap.GetTile(pos) != null)
            {
                eventTilemap.SetTile(pos, null);
                Debug.Log($"[CombatTile] Deleted Combat Tile at {pos}");
            }
            else
            {
                Debug.Log($"[CombatTile] Tried to delete Combat Tile at {pos}, but no tile found.");
            }

            lastCombatTilePos = null;
        }

        // Unsubscribe to avoid duplicate calls
        CombatManager.OnCombatClosed -= DeleteCombatTile;
    }
}
