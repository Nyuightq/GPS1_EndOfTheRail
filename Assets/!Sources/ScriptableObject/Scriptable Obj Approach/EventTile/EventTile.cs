using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class EventTile : Tile
{
    public abstract void OnPlayerEnter(GameObject player);

    // new exit method (default does nothing)
    public virtual void OnPlayerExit(GameObject player) { }
}
