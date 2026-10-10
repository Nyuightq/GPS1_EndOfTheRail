// --------------------------------------------------------------
// Creation Date: 2026-10-10 22:01
// Author: ZQlie
// Description: -
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Custom Tiles/Open UI Tile")]
public class OpenUITile : EventTile
{
    [Header("UI Settings")]
    [Tooltip("Must exactly match a panelID in the TutorialUIManager in your scene.")]
    public string uiPanelID = "MovementTutorial";

    private static Tilemap eventTilemap;
    private static Vector3Int? lastTutorialTilePos;

    public override void OnPlayerEnter(GameObject player)
    {
        // 1. Cache EventTilemap
        if (eventTilemap == null)
        {
            GameObject tilemapObj = GameObject.Find("EventTilemap");
            if (tilemapObj != null)
                eventTilemap = tilemapObj.GetComponent<Tilemap>();
        }

        // 2. Snap to center and cache position
        if (eventTilemap != null)
        {
            Vector3Int tilePos = eventTilemap.WorldToCell(player.transform.position);
            lastTutorialTilePos = tilePos;

            Vector3 centerPos = eventTilemap.GetCellCenterWorld(tilePos);
            player.transform.position = centerPos;
        }

        // 3. Freeze train
        TrainFreezeController freezeController = player.GetComponent<TrainFreezeController>();
        if (freezeController != null)
            freezeController.FreezeTrain();

        // 4. Open UI
        if (TutorialUIManager.Instance != null)
        {
            TutorialUIManager.Instance.OpenPanel(uiPanelID);
        }

        // 5. Subscribe to deletion event
        TutorialUIManager.OnTutorialClosed -= DeleteTutorialTile;
        TutorialUIManager.OnTutorialClosed += DeleteTutorialTile;
    }

    private static void DeleteTutorialTile()
    {
        // Delete the tile so it doesn't trigger again when the train moves off it
        if (eventTilemap != null && lastTutorialTilePos.HasValue)
        {
            Vector3Int pos = lastTutorialTilePos.Value;
            if (eventTilemap.GetTile(pos) != null)
            {
                eventTilemap.SetTile(pos, null);
                Debug.Log($"Tutorial Tile deleted at {pos}");
            }
            lastTutorialTilePos = null;
        }

        TutorialUIManager.OnTutorialClosed -= DeleteTutorialTile;
    }
}