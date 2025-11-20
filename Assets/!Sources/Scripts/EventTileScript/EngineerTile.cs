using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "EngineerTile", menuName = "Custom Tiles/EngineerTile")]
public class EngineerTile : EventTile
{
    private static Tilemap eventTilemap;

    public override void OnPlayerEnter(GameObject player)
    {
        // Prevent triggering if UI is active or cooldown in progress
        if (EngineerManager.Instance != null &&
            (EngineerManager.Instance.IsEngineerUIActive || EngineerManager.Instance.IsCooldownActive))
        {
            Debug.Log("[EngineerTile] Ignored trigger because Engineer UI is open or cooldown active.");
            return;
        }

        Debug.Log($"[EngineerTile] Player entered Engineer Tile");

        // Play entry sound
        if (SoundManager.Instance != null)
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
        if (freezeController == null)
            freezeController = Object.FindFirstObjectByType<TrainFreezeController>();
            
        if (freezeController != null)
        {
            freezeController.FreezeTrain();
            Debug.Log("[EngineerTile] Train frozen.");
        }

        // Open Engineer UI through the EngineerManager
        if (EngineerManager.Instance != null)
        {
            EngineerManager.Instance.OpenEngineerUI(player);
        }
    }

    public override void OnPlayerExit(GameObject player)
    {
        Debug.Log("[EngineerTile] Player exited Engineer Tile");
        
        if (SoundManager.Instance != null)
        {
            
        }
            //SoundManager.Instance.PlaySFX("SFX_ButtonOnCancel");
    }
}