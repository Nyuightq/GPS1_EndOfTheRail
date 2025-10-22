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
    public string storyText;        // Unique story text
    public Sprite storyImage;       // Optional story image

    private static Tilemap eventTilemap;

    // Debounce tracking to prevent double-triggering
    private static GameObject lastPlayer;
    private static StoryTile lastTriggeredTile;

    public override void OnPlayerEnter(GameObject player)
    {
        // Ignore duplicate triggers for same tile & same player
        if (lastPlayer == player && lastTriggeredTile == this)
        {
            Debug.Log($"[StoryTile] Duplicate trigger ignored for: {name}");
            return;
        }

        lastPlayer = player;
        lastTriggeredTile = this;

        Debug.Log($"Player entered Story Tile: {name}");

        // Cache EventTilemap reference if needed
        if (eventTilemap == null)
        {
            GameObject tilemapObj = GameObject.Find("EventTilemap");
            if (tilemapObj != null)
                eventTilemap = tilemapObj.GetComponent<Tilemap>();
        }

        // Snap player (train) to tile center
        if (eventTilemap != null)
        {
            Vector3Int tilePos = eventTilemap.WorldToCell(player.transform.position);
            Vector3 centerPos = eventTilemap.GetCellCenterWorld(tilePos);
            player.transform.position = centerPos;
            Debug.Log($"[StoryTile] Snapped train to center at {tilePos}");
        }

        // Freeze the train
        TrainFreezeController freezeController = player.GetComponent<TrainFreezeController>();
        if (freezeController != null)
            freezeController.FreezeTrain();

        // Show story UI
        StoryManager.Instance.OpenStoryUI(this, player);
    }

    public override void OnPlayerExit(GameObject player)
    {
        // Reset debounce when player exits
        if (lastPlayer == player)
        {
            lastPlayer = null;
            lastTriggeredTile = null;
        }

        Debug.Log($"Player exited Story Tile: {name}");
    }
}
