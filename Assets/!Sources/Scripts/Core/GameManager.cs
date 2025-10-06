// --------------------------------------------------------------
// Creation Date: 2025-10-02 23:37
// Author: Ysaac
// Description: This is a singleton that handles most events in the game
// --------------------------------------------------------------
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set;}

    public static event Action<Vector3Int, Tile,RailData> onSpawnTile;
    public static event Action<Vector3Int> onDestroyTile;

    void Awake()
    {
        if(instance != null && instance != gameObject)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(instance);
    }

    public static void spawnTile(Vector3Int cellPos,Tile sprite ,RailData data)
    {
        onSpawnTile?.Invoke(cellPos,sprite,data);
    }

    public static void DestroyTile(Vector3Int cellPos)
    {
        onDestroyTile?.Invoke(cellPos);
    }
}
