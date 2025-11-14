using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "EngineerTile", menuName = "Custom Tiles/EngineerTile")]
public class EngineerTile : EventTile
{
    private static Tilemap eventTilemap;
    private static float lastTriggerTime = -10f;
    private const float debounceDuration = 0.25f; // Prevents rapid retriggering (Remark: Debounce doesnt work, still retriggers twice)

    public override void OnPlayerEnter(GameObject player)
    {
        // Debounce check — ignore if still cooling down
        if (Time.time - lastTriggerTime < debounceDuration)
            return;
        lastTriggerTime = Time.time;

        Debug.Log($"[EngineerTile] Player entered Engineer Tile");

        // Play entry sound
        SoundManager.Instance.PlaySFX("SFX_EngineerEnter");

        // Find and cache EventTilemap if not already set
        if (eventTilemap == null)
        {
            GameObject tilemapObj = GameObject.Find("EventTilemap");
            if (tilemapObj != null)
                eventTilemap = tilemapObj.GetComponent<Tilemap>();
        }

        // Snap train to exact center of tile
        if (eventTilemap != null)
        {
            Vector3Int tilePos = eventTilemap.WorldToCell(player.transform.position);
            Vector3 centerPos = eventTilemap.GetCellCenterWorld(tilePos);
            player.transform.position = centerPos;
            Debug.Log($"[EngineerTile] Snapped train to center at {tilePos}");
        }

        // Freeze train movement while UI is open
        TrainFreezeController freezeController = player.GetComponent<TrainFreezeController>();
        if (freezeController != null)
        {
            freezeController.FreezeTrain();
            Debug.Log("[EngineerTile] Train frozen.");
        }

        // Open Engineer UI through the EngineerManager
        EngineerManager.Instance.OpenEngineerUI(player);
    }

    public override void OnPlayerExit(GameObject player)
    {
        Debug.Log("[EngineerTile] Player exited Engineer Tile");
        SoundManager.Instance.PlaySFX("SFX_ButtonOnCancel");
    }
}
