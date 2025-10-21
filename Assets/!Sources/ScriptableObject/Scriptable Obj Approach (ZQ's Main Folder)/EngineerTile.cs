// --------------------------------------------------------------
// Creation Date: 2025-10-17 21:41
// Author: ZQlie
// Description: -
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "EngineerTile", menuName = "Custom Tiles/EngineerTile")]
public class EngineerTile : EventTile
{
    private static Tilemap eventTilemap;

    public override void OnPlayerEnter(GameObject player)
    {
        Debug.Log($"Player entered Engineer Tile");

        // Find and cache EventTilemap
        if (eventTilemap == null)
        {
            GameObject tilemapObj = GameObject.Find("EventTilemap");
            if (tilemapObj != null)
                eventTilemap = tilemapObj.GetComponent<Tilemap>();
        }

        // Snap train to exact center of this tile IMMEDIATELY
        if (eventTilemap != null)
        {
            Vector3Int tilePos = eventTilemap.WorldToCell(player.transform.position);
            Vector3 centerPos = eventTilemap.GetCellCenterWorld(tilePos);
            player.transform.position = centerPos;
            
            Debug.Log($"[EngineerTile] Snapped train to center at {tilePos}");
        }

        // Get reference to TrainFreezeController AFTER snapping to center
        TrainFreezeController freezeController = player.GetComponent<TrainFreezeController>();
        if (freezeController != null)
            freezeController.FreezeTrain();

        // Logic for engineer or link to manager
    }

    public override void OnPlayerExit(GameObject player)
    {
        Debug.Log($"Player exited Engineer Tile");
    }
}