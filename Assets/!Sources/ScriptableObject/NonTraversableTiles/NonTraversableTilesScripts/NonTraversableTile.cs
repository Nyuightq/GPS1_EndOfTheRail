using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Custom Tiles/NonTraversableTile")]
public class NonTraversableTile : Tile
{
    // Optional: add a tooltip or description for designers
    [TextArea]
    public string note = "Player cannot build rails on this tile.";

    // A property you can check later if needed
    public bool isTraversable = false;

    // Optional: you could override GetTileData if you want to show special visuals
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);
    }
}
