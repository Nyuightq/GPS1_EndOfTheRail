// --------------------------------------------------------------
// Creation Date: 2025-10-13
// Author: -
// Description: Custom Story Tile that triggers unique dialogue
// or story text when the player enters it.
// --------------------------------------------------------------
using UnityEngine;

[CreateAssetMenu(menuName = "Custom Tiles/StoryTile")]
public class StoryTile : EventTile
{
    [Header("Story Content")]
    [TextArea(3, 6)] 
    public string storyText;        // Unique text shown for this tile
    public Sprite storyImage;       // Optional: image to show

    public override void OnPlayerEnter(GameObject player)
    {
        Debug.Log($"Player entered Story Tile: {name}");

        // Get reference to TrainFreezeController (to stop train)
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
