// --------------------------------------------------------------
// Creation Date: 2025-10-18 23:30
// Author: ZQlie
// Description: -
// --------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RewardTileHandler : MonoBehaviour
{
    public static RewardTileHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private List<(Tilemap, Vector3Int)> tilesToDelete = new();

    public void RegisterTileForDeletion(Tilemap map, Vector3Int pos)
    {
        tilesToDelete.Add((map, pos));
        Debug.Log($"[RewardTileHandler] Registered RewardTile at {pos}");
    }

    private void OnEnable()
    {
        RewardManager.OnRewardClosed += HandleRewardClosed;
    }

    private void OnDisable()
    {
        RewardManager.OnRewardClosed -= HandleRewardClosed;
    }

    private void HandleRewardClosed()
    {
        foreach (var (map, pos) in tilesToDelete)
        {
            if (map != null)
            {
                TileBase tile = map.GetTile(pos);
                if (tile != null && tile.GetType() == typeof(RewardTile))
                {
                    map.SetTile(pos, null);
                    Debug.Log($"[RewardTileHandler] RewardTile deleted at {pos}");
                }
            }
        }

        tilesToDelete.Clear();
    }
}
