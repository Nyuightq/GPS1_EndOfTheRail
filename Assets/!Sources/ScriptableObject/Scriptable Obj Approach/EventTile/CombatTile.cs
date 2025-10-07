using UnityEngine;

[CreateAssetMenu(menuName = "Custom Tiles/Combat Tile")]
public class CombatTile : EventTile
{
    public override void OnPlayerEnter(GameObject player)
    {
        Debug.Log("Player stepped on a Combat Tile!");
        CombatManager.Instance.StartCombat(player);
    }

    public override void OnPlayerExit(GameObject player)
    {
        // Optional: Close UI when player leaves the tile
        CombatManager.Instance.EndCombat();
    }
}
