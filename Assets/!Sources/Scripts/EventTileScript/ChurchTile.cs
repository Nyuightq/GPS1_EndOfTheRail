using UnityEngine;
using UnityEngine.Tilemaps;

//Remark: No debounce occurence (Remark: Debounce doesnt work, still retriggers twice)

[CreateAssetMenu(menuName = "Custom Tiles/ChurchTile")]
public class ChurchTile : EventTile
{
    [Header("Healing Settings")]
    public int healAmount = 20;

    private static Tilemap eventTilemap;
    private static float lastTriggerTime = -10f;
    private const float debounceDuration = 0.25f;

    public override void OnPlayerEnter(GameObject player)
    {
        // Prevent triggering if UI is active or cooldown in progress
        if (ChurchManager.Instance != null &&
            (ChurchManager.Instance.IsChurchUIActive || ChurchManager.Instance.IsCooldownActive))
        {
            Debug.Log("[ChurchTile] Ignored trigger because Church UI is open or cooldown active.");
            return;
        }

        // Standard debounce
        if (Time.time - lastTriggerTime < debounceDuration)
        {
            Debug.Log("[ChurchTile] Ignored duplicate trigger (debounce).");
            return;
        }
        lastTriggerTime = Time.time;

        Debug.Log("Player entered Church Tile");

        SoundManager.Instance.PlaySFX("SFX_EventWindowPopup");
        Debug.Log("SFX_EventWindowPopup");

        // Find and cache EventTilemap
        if (eventTilemap == null)
        {
            GameObject tilemapObj = GameObject.Find("EventTilemap");
            if (tilemapObj != null)
                eventTilemap = tilemapObj.GetComponent<Tilemap>();
        }

        // Snap train to exact center
        if (eventTilemap != null)
        {
            Vector3Int tilePos = eventTilemap.WorldToCell(player.transform.position);
            Vector3 centerPos = eventTilemap.GetCellCenterWorld(tilePos);
            player.transform.position = centerPos;
            Debug.Log($"[ChurchTile] Snapped train to center at {tilePos}");
        }

        // Freeze train AFTER snapping
        TrainFreezeController freezeController = player.GetComponent<TrainFreezeController>();
        if (freezeController == null)
            freezeController = Object.FindFirstObjectByType<TrainFreezeController>();
        if (freezeController != null)
            freezeController.FreezeTrain();

        // Open Church UI
        ChurchManager.Instance?.OpenChurchUI(player);
    }

    public override void OnPlayerExit(GameObject player)
    {
        // SoundManager.Instance.PlaySFX("SFX_ButtonOnCancel");
        Debug.Log("SFX_ButtonOnCancel");

        Debug.Log("Player exited Church Tile");
    }
}
