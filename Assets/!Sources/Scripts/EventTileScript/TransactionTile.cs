using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Custom Tiles/Transaction Tile")]
public class TransactionTile : EventTile
{
    private static Tilemap eventTilemap;
    private static float lastTriggerTime = -10f;
    private const float debounceDuration = 0.25f;

    public override void OnPlayerEnter(GameObject player)
    {
        // Prevent triggering if UI is active or cooldown in progress
        if (TransactionManager.Instance != null &&
            (TransactionManager.Instance.IsTransactionUIActive || TransactionManager.Instance.IsCooldownActive))
        {
            Debug.Log("[TransactionTile] Ignored trigger because Transaction UI is open or cooldown active.");
            return;
        }

        // Standard debounce
        if (Time.time - lastTriggerTime < debounceDuration)
        {
            Debug.Log("[TransactionTile] Ignored duplicate trigger (debounce).");
            return;
        }
        lastTriggerTime = Time.time;

        Debug.Log("Player entered Transaction Tile");

        // Find and cache EventTilemap
        if (eventTilemap == null)
        {
            GameObject tilemapObj = GameObject.Find("EventTilemap");
            if (tilemapObj != null)
                eventTilemap = tilemapObj.GetComponent<Tilemap>();
        }

        // Snap train to exact center of this tile
        if (eventTilemap != null)
        {
            Vector3Int tilePos = eventTilemap.WorldToCell(player.transform.position);
            Vector3 centerPos = eventTilemap.GetCellCenterWorld(tilePos);
            player.transform.position = centerPos;

            Debug.Log($"[TransactionTile] Snapped train to center at {tilePos}");
        }

        // Freeze the train AFTER snapping
        TrainFreezeController freezeController = player.GetComponent<TrainFreezeController>();
        if (freezeController == null)
            freezeController = Object.FindFirstObjectByType<TrainFreezeController>();

        if (freezeController != null)
            freezeController.FreezeTrain();

        SoundManager.Instance.PlaySFX("SFX_EventWindowPopup");
        Debug.Log($"SFX_EventWindowPopup");

        // Open Transaction UI
        TransactionManager.Instance?.OpenTransactionUI(player);
    }

    public override void OnPlayerExit(GameObject player)
    {
        SoundManager.Instance.PlaySFX("SFX_ButtonOnCancel");
        Debug.Log($"SFX_ButtonOnCancel");

        Debug.Log("Player exited Transaction Tile");
    }
}
