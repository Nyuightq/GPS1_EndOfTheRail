// --------------------------------------------------------------
// Creation Date: 2025-10-02 22:56
// Author: User
// Description: -
// --------------------------------------------------------------
using System.Collections.Generic;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RailData
{
    public Vector2 directionIn;
    public Vector2 directionOut;
    public Tile tile;

    public enum directionType
    {
        Incoming,
        Outgoing
    }

    public void setDirection(Vector2 dir,directionType dirType)
    {
        switch(dirType)
        {
            case directionType.Incoming:
                directionIn = dir;break;
            case directionType.Outgoing:
                directionOut = dir; break;
        }
    }

    public int getConnection()
    {
        int right = ((directionIn == Vector2.right || directionOut == Vector2.right)? 1 : 0);
        int left = ((directionIn == Vector2.left || directionOut == Vector2.left) ? 1 : 0);
        int up = ((directionIn == Vector2.up || directionOut == Vector2.up) ? 1 : 0);
        int down = ((directionIn == Vector2.down || directionOut == Vector2.down) ? 1 : 0);
        int connection = right + (left * 2) + (up * 4) + (down * 8);
        return connection;
    }
}

public class RailGridScript : MonoBehaviour
{

    private Grid grid;
    [SerializeField] private Tilemap railTileMap;
    [SerializeField] private Tile defaultRail;
    public Dictionary<Vector3Int,RailData> railDataMap = new Dictionary<Vector3Int,RailData>();

    void Awake()
    {
        grid = GetComponent<Grid>();
    }

    private void OnEnable()
    {
        GameManager.onSpawnTile += spawnTile;
    }

    private void OnDisable()
    {
        GameManager.onSpawnTile -= spawnTile;
    }

    void Start()
    {
       
    }

    public Vector3 snapToGrid(Vector3 worldPos)
    {
        Vector3Int cellPos = grid.WorldToCell(worldPos);
        return grid.GetCellCenterWorld(cellPos);
    }

    public bool railAtPos(Vector3Int tilePos)
    {
        return railDataMap.ContainsKey(tilePos);
    }

    private void spawnTile(Vector3Int tilePos, Tile sprite, RailData data)
    {
        if (sprite == null)
        {
            Debug.LogWarning($"⚠️ Tried to place NULL sprite at {tilePos}, RailData connection {data.getConnection()}");
        }
        else
        {
            railTileMap.SetTile(tilePos, sprite);
            railDataMap[tilePos] = data;
        }
    }

    void Update()
    {
        
    }
}
