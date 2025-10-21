// --------------------------------------------------------------
// Creation Date: 2025-10-08 15:14
// Author: ZQlie
// Description: -
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.Tilemaps;

public class RailPlacementValidator : MonoBehaviour
{
    [SerializeField] private Tilemap eventTilemap;

    public bool CanPlaceRail(Vector2 position)
    {
        if (eventTilemap == null)
        {
            Debug.LogWarning("EventTilemap not assigned in RailPlacementValidator!");
            return true; // allow placement if no tilemap assigned
        }

        Vector3Int cellPos = eventTilemap.WorldToCell(position);
        TileBase tile = eventTilemap.GetTile(cellPos);

        if (tile is NonTraversableTile)
        {
            Debug.Log("Cannot build rail here — NonTraversableTile detected!");
            return false;
        }

        return true;
    }
}
