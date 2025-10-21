using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Custom Tiles/Reward Tile")]
public class RewardTile : EventTile
{
    private static Tilemap eventTilemap;
    private static Vector3Int? lastRewardTilePos;

    public override void OnPlayerEnter(GameObject player)
    {
        Debug.Log("Player entered Reward Tile");
        
        // Find and cache EventTilemap
        if (eventTilemap == null)
        {
            GameObject tilemapObj = GameObject.Find("EventTilemap");
            if (tilemapObj != null)
                eventTilemap = tilemapObj.GetComponent<Tilemap>();
        }

        // Cache tile position and IMMEDIATELY snap train to center
        if (eventTilemap != null)
        {
            Vector3Int tilePos = eventTilemap.WorldToCell(player.transform.position);
            lastRewardTilePos = tilePos;
            
            // NEW: Snap train to exact center of this tile IMMEDIATELY
            Vector3 centerPos = eventTilemap.GetCellCenterWorld(tilePos);
            player.transform.position = centerPos;
            
            Debug.Log($"[RewardTileHandler] Registered RewardTile at {tilePos}, snapped train to center");
        }

        // Freeze train AFTER snapping to center
        TrainFreezeController freezeController = player.GetComponent<TrainFreezeController>();
        if (freezeController != null)
            freezeController.FreezeTrain();

        // Open UI
        RewardManager.Instance.OpenRewardUI(player);

        // Subscribe once to reward UI close event
        RewardManager.OnRewardClosed -= DeleteRewardTile;
        RewardManager.OnRewardClosed += DeleteRewardTile;
    }

    public override void OnPlayerExit(GameObject player)
    {
        Debug.Log("Player exited Reward Tile");
        RewardManager.Instance.CloseRewardUI();
    }

    private static void DeleteRewardTile()
    {
        if (eventTilemap != null && lastRewardTilePos.HasValue)
        {
            Vector3Int pos = lastRewardTilePos.Value;
            if (eventTilemap.GetTile(pos) != null)
            {
                eventTilemap.SetTile(pos, null);
                Debug.Log($"Reward Tile deleted at {pos}");
            }
            else
            {
                Debug.Log($"Tried to delete Reward Tile at {pos}, but no tile found.");
            }

            lastRewardTilePos = null;
        }

        // Unsubscribe after deletion to prevent double calls
        RewardManager.OnRewardClosed -= DeleteRewardTile;
    }
}