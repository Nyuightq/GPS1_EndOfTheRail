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
    public enum railTypes {normal, start, end}
    public enum directionType { Incoming, Outgoing }

    public railTypes railType = railTypes.normal;
    public Vector2 directionIn;
    public Vector2 directionOut;
    public Tile tile;

    public RailData(railTypes railType = railTypes.normal)
    {
        this.railType = railType;
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
    [SerializeField] private Tile startPoint;
    [SerializeField] private Tile endPoint;

    [SerializeField] private GameObject train;

    public Dictionary<Vector3Int,RailData> railDataMap = new Dictionary<Vector3Int,RailData>();

    void Awake()
    {
        grid = GetComponent<Grid>();
    }

    private void OnEnable()
    {
        GameManager.onSpawnTile += spawnTile;
        GameManager.onDestroyTile += destroyTile;
    }

    private void OnDisable()
    {
        GameManager.onSpawnTile -= spawnTile;
        GameManager.onDestroyTile -= destroyTile;
    }

    void Start()
    {
        registerRails();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            foreach(var rail in railDataMap)
            {
                Vector3Int pos = rail.Key;
                RailData data = rail.Value;
                if(data.railType == RailData.railTypes.start)
                {
                    Vector3 worldPos = snapToGrid(pos);
                    GameObject trainIns = Instantiate(train,worldPos,Quaternion.identity);
                    TrainMovement tm = trainIns.GetComponent<TrainMovement>();

                    tm.gridManager = gameObject;
                    break;
                }
            }
        }
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
        railTileMap.SetTile(tilePos, sprite);
        railDataMap[tilePos] = data;
    }

    private void destroyTile(Vector3Int tilePos)
    {
        railTileMap.SetTile(tilePos, null);
        if(railDataMap.ContainsKey(tilePos)) railDataMap.Remove(tilePos);
    }

    private void registerRails()
    {
        BoundsInt bounds = railTileMap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for(int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                if(railTileMap.HasTile(tilePos))
                {
                    TileBase tile = railTileMap.GetTile(tilePos);

                    if (tile == startPoint)
                    {
                        railDataMap[tilePos] = new RailData(RailData.railTypes.start);
                    }
                    else if(tile == endPoint)
                    {
                        railDataMap[tilePos] = new RailData(RailData.railTypes.end);
                    }
                }
            }
        }
    }
}
