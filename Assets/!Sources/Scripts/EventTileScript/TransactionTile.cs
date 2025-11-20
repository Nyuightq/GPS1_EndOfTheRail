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
            Debug.Log("[TransactionTile] Ignored: Transaction UI is open or cooldown active.");
            return;
        }

        // Standard debounce to prevent rapid re-triggers
        if (Time.time - lastTriggerTime < debounceDuration)
        {
            Debug.Log("[TransactionTile] Ignored: Debounce active.");
            return;
        }
        lastTriggerTime = Time.time;

        Debug.Log("[TransactionTile] Player entered Transaction Tile");

        // Find and cache EventTilemap if not already cached
        if (eventTilemap == null)
        {
            GameObject tilemapObj = GameObject.Find("EventTilemap");
            if (tilemapObj != null)
            {
                eventTilemap = tilemapObj.GetComponent<Tilemap>();
            }
            else
            {
                Debug.LogWarning("[TransactionTile] EventTilemap not found! Cannot snap player.");
            }
        }

        // Snap player to exact center of this tile
        if (eventTilemap != null)
        {
            Vector3Int tilePos = eventTilemap.WorldToCell(player.transform.position);
            Vector3 centerPos = eventTilemap.GetCellCenterWorld(tilePos);
            player.transform.position = centerPos;

            Debug.Log($"[TransactionTile] Snapped player to tile center at {tilePos}");
        }

        // Freeze the train AFTER snapping position
        TrainFreezeController freezeController = player.GetComponent<TrainFreezeController>();
        if (freezeController == null)
        {
            freezeController = Object.FindFirstObjectByType<TrainFreezeController>();
        }

        if (freezeController != null)
        {
            freezeController.FreezeTrain();
            Debug.Log("[TransactionTile] Train frozen.");
        }
        else
        {
            Debug.LogWarning("[TransactionTile] TrainFreezeController not found!");
        }

        // Play SFX if available
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX("SFX_EventWindowPopup");
            Debug.Log("[TransactionTile] Played SFX_EventWindowPopup");
        }

        // Open Transaction UI
        if (TransactionManager.Instance != null)
        {
            TransactionManager.Instance.OpenTransactionUI(player);
        }
        else
        {
            Debug.LogError("[TransactionTile] TransactionManager.Instance is null!");
        }
    }

    public override void OnPlayerExit(GameObject player)
    {
        // Play exit SFX
        if (SoundManager.Instance != null)
        {
            //SoundManager.Instance.PlaySFX("SFX_ButtonOnCancel");
            Debug.Log("[TransactionTile] Played SFX_ButtonOnCancel");
        }

        Debug.Log("[TransactionTile] Player exited Transaction Tile");
    }
}