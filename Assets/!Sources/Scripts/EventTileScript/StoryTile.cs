// --------------------------------------------------------------
// Creation Date: 2025-10-13
// Author: -
// Description: Custom Story Tile that triggers unique dialogue
// or story text when the player enters it.
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Custom Tiles/StoryTile")]
public class StoryTile : EventTile
{
    [Header("Story Content")]
    [TextArea(3, 6)] 
    public string storyText;        // Unique text shown for this tile
    public Sprite storyImage;       // Optional: image to show

    private static Tilemap eventTilemap;

    public override void OnPlayerEnter(GameObject player)
    {
        Debug.Log($"Player entered Story Tile: {name}");

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
            
            Debug.Log($"[StoryTile] Snapped train to center at {tilePos}");
        }

        // Get reference to TrainFreezeController AFTER snapping to center
        TrainFreezeController freezeController = player.GetComponent<TrainFreezeController>();
        if (freezeController != null)
            freezeController.FreezeTrain();

        // Open Story UI with this tile's unique data
        StoryManager.Instance.OpenStoryUI(this, player);
    }

    public override void OnPlayerExit(GameObject player)
    {
        Debug.Log($"Player exited Story Tile: {name}");
        // You can optionally auto-close UI here, or leave it controlled by UI button.
    }
}