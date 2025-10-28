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
    public string storyText;
    public Sprite storyImage;

    private static Tilemap eventTilemap;
    private static GameObject lastPlayer;
    private static StoryTile lastTriggeredTile;

    public override void OnPlayerEnter(GameObject player)
    {
        // Global debounce: skip if StoryManager is already active
        if (StoryManager.Instance != null && StoryManager.Instance.IsStoryActive)
        {
            Debug.Log($"[StoryTile] StoryManager busy, ignoring tile {name}");
            return;
        }

        // Skip duplicate triggers for same tile and player
        if (lastPlayer == player && lastTriggeredTile == this)
        {
            Debug.Log($"[StoryTile] Duplicate trigger ignored for: {name}");
            return;
        }

        lastPlayer = player;
        lastTriggeredTile = this;

        Debug.Log($"Player entered Story Tile: {name}");

        if (eventTilemap == null)
        {
            GameObject tilemapObj = GameObject.Find("EventTilemap");
            if (tilemapObj != null)
                eventTilemap = tilemapObj.GetComponent<Tilemap>();
        }

        if (eventTilemap != null)
        {
            Vector3Int tilePos = eventTilemap.WorldToCell(player.transform.position);
            Vector3 centerPos = eventTilemap.GetCellCenterWorld(tilePos);
            player.transform.position = centerPos;
            Debug.Log($"[StoryTile] Snapped train to center at {tilePos}");
        }

        var freezeController = player.GetComponent<TrainFreezeController>();
        if (freezeController != null)
            freezeController.FreezeTrain();

        StoryManager.Instance.OpenStoryUI(this, player);
    }

    public override void OnPlayerExit(GameObject player)
    {
        if (lastPlayer == player)
        {
            lastPlayer = null;
            lastTriggeredTile = null;
        }

        Debug.Log($"Player exited Story Tile: {name}");
    }
}
