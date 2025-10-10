using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerTileChecker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap eventTilemap; // assign your Tilemap here

    private Vector3Int lastCell;       // track last cell the player was in
    private TileBase lastTile;         // track last tile the player was on

    private void Start()
    {
        // if not assigned in Inspector (e.g. prefab)
        if (eventTilemap == null)
        {
            // Try to find it in the scene (under GridManager)
            eventTilemap = GameObject.Find("EventTilemap")?.GetComponent<Tilemap>();
        }

        if (eventTilemap == null)
        {
            Debug.LogError("EventTilemap not found in scene!");
            return;
        }
    }

    private void Update()
    {
        if (eventTilemap == null) return;

        // Get the cell position the player is standing on
        Vector3Int currentCell = eventTilemap.WorldToCell(transform.position);

        // Only check if we've moved to a new cell
        if (currentCell != lastCell)
        {
            // ---- EXIT OLD TILE ----
            if (lastTile is EventTile lastEventTile)
            {
                Debug.Log($"Player exited {lastTile.name}");
                lastEventTile.OnPlayerExit(gameObject);
            }

            // ---- ENTER NEW TILE ----
            TileBase currentTile = eventTilemap.GetTile(currentCell);
            if (currentTile is EventTile newEventTile)
            {
                Debug.Log($"Player entered {currentTile.name}");
                newEventTile.OnPlayerEnter(gameObject);
            }

            // update trackers
            lastCell = currentCell;
            lastTile = currentTile;
        }
    }
}
