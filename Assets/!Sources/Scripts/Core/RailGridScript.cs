// --------------------------------------------------------------
// Creation Date: 2025-10-02 22:56
// Author: User
// Description: -
// --------------------------------------------------------------
using System.Collections.Generic;
using Unity.VisualScripting;
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

    public RailLine line;

    public HashSet<Vector3Int> railEnd = new HashSet<Vector3Int>();

    public RailData(railTypes railType = railTypes.normal)
    {
        this.railType = railType;
    }

    public void setLine(RailLine line) { this.line = line; }

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

public class RailLine
{
   public List<Vector3Int> line = new List<Vector3Int>();
}

public class RailGridScript : MonoBehaviour
{

    private Grid grid;
    [SerializeField] private Tilemap railTileMap;
    [SerializeField] private Tile defaultRail;
    [SerializeField] private Tile startPointSprite;
    [SerializeField] private Tile endPointSprite;

    [SerializeField] private GameObject train;


    public Vector3Int startPoint;
    public Vector3Int endPoint;

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
        startPoint = findPoint(railDataMap,RailData.railTypes.start);
        endPoint = findPoint(railDataMap,RailData.railTypes.end);

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && validatePath(startPoint))
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

    public bool validatePath(Vector3Int startPoint)
    {
        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int(1,0,0),
            new Vector3Int(-1,0,0),
            new Vector3Int(0,1,0),
            new Vector3Int(0,-1,0)
        };

        foreach (Vector3Int direction in directions)
        {
            Vector3Int tile = startPoint + direction;
            if (railAtPos(tile))
            {
                List<Vector3Int> lineList = railDataMap[tile].line.line;
                if (lineList != null)
                {
                    if (railDataMap[lineList[0]].railType == RailData.railTypes.start && railDataMap[lineList[^1]].railType == RailData.railTypes.end)
                    {
                        railDataMap[lineList[0]].directionOut = new Vector2(lineList[1].x - lineList[0].x, lineList[1].y - lineList[0].y);
                        railDataMap[lineList[^2]].directionOut = new Vector2(lineList[^1].x - lineList[^2].x, lineList[^1].y - lineList[^2].y);
                        return true;
                    }
                }
            }
        }
        return false;
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

    private bool isRailEnd(Vector3Int tilePos)
    {
        RailData rail = railDataMap[tilePos];
        Vector3Int connectedTileIngoing = tilePos + Vector3Int.FloorToInt(new Vector3(rail.directionIn.x,rail.directionIn.y,0));
        Vector3Int connectedTileOutgoing = tilePos + Vector3Int.FloorToInt(new Vector3(rail.directionOut.x, rail.directionOut.y, 0));
        return !(railAtPos(connectedTileOutgoing) && railAtPos(connectedTileIngoing));
    }

    public Vector3Int findPoint(Dictionary<Vector3Int,RailData> railDataMap, RailData.railTypes railType)
    {
        if(railDataMap.Count == 0) Debug.Log("nothing in yet");

        foreach (KeyValuePair<Vector3Int, RailData> rail in railDataMap)
        {
            if(rail.Value.railType == railType)
            {
                Debug.Log(rail.Key);
                return rail.Key;
            }
            else
            {
                Debug.Log("cant find shit");
            }
        }
        return new Vector3Int(500,500,500);
    }

    public bool hasConnection(Vector3Int tilePos)
    {
        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int(1,0,0),
            new Vector3Int(-1,0,0),
            new Vector3Int(0,1,0),
            new Vector3Int(0,-1,0)
        };

        foreach (Vector3Int direction in directions)
        {
            if (railAtPos(tilePos+direction))
            {
                return true;
            }
        }
        return false;
    }
    public bool onConnection(Vector3Int tileSource, Vector3Int tilePos)
    {
        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int(1,0,0),
            new Vector3Int(-1,0,0),
            new Vector3Int(0,1,0),
            new Vector3Int(0,-1,0)
        };

        foreach(Vector3Int direction in directions)
        {
            if(tilePos == tileSource+direction)
            {
                return true;
            }
        }
        return false;
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

                    if (tile == startPointSprite)
                    {
                        railDataMap[tilePos] = new RailData(RailData.railTypes.start);
                    }
                    else if(tile == endPointSprite)
                    {
                        railDataMap[tilePos] = new RailData(RailData.railTypes.end);
                    }
                }
            }
        }
    }
}
