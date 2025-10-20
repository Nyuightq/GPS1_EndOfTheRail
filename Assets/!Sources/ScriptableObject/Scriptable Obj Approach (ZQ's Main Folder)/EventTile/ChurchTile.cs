using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Custom Tiles/ChurchTile")]
public class ChurchTile : EventTile
{
    [Header("Healing Settings")]
    public int healAmount = 20;

    private static Tilemap eventTilemap;

    public override void OnPlayerEnter(GameObject player)
    {
        Debug.Log("Player entered Church Tile");

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
            
            Debug.Log($"[ChurchTile] Snapped train to center at {tilePos}");
        }

        // Freeze the train AFTER snapping to center
        TrainFreezeController freezeController = player.GetComponent<TrainFreezeController>();
        if (freezeController != null)
            freezeController.FreezeTrain();

        // Open Church UI
        ChurchManager.Instance.OpenChurchUI(player);
    }

    public override void OnPlayerExit(GameObject player)
    {
        Debug.Log("Player exited Church Tile");
        // Optionally close UI on exit if desired
        // ChurchManager.Instance.CloseChurchUI();
    }
}