// --------------------------------------------------------------
// Creation Date: 2025-10-13 22:23
// Author: ZQlie
// Description: -
// --------------------------------------------------------------
using UnityEngine;

[CreateAssetMenu(menuName = "Custom Tiles/Debug Damage Tile")]
public class DebugDamageTile : EventTile
{
    [Header("Damage Settings")]
    public int damageAmount = 100;

    public override void OnPlayerEnter(GameObject player)
    {
        Debug.Log($"Player stepped on DebugDamageTile! Dealing {damageAmount} HP.");

        // Find the player's status manager in the scene
        PlayerStatusManager playerStatus = FindObjectOfType<PlayerStatusManager>();
        if (playerStatus != null)
        {
            int newHp = Mathf.Max(0, playerStatus.Hp - damageAmount);
            playerStatus.UpdateCurrentHp(newHp);
            Debug.Log($"Player HP reduced to {newHp}");
        }
        else
        {
            Debug.LogWarning("No PlayerStatusManager found in the scene!");
        }
    }

    public override void OnPlayerExit(GameObject player)
    {
        // No action on exit
    }
}
