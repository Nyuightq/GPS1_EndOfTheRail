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
    public enum railTypes {normal, start, end, rest, disabled}
    public enum directionType { Incoming, Outgoing }

    public railTypes railType = railTypes.normal;
    public Vector2 directionIn;
    public Vector2 directionOut;
    private Vector3Int pos;
    public Vector3Int Pos => pos;
    public Tile tile;

    public RailLine line;

    public HashSet<Vector3Int> railEnd = new HashSet<Vector3Int>();

    public RailData(Vector3Int pos, railTypes railType = railTypes.normal)
    {
        this.pos = pos;
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

        updateVisual();
    }

    private void updateVisual()
    {
        // Don’t update visuals if this is a special rail
        if (railType != railTypes.normal) return;

        string tileName = $"rail_{getConnection()}";
        Tile selectedTile = null;

        foreach (Tile t in ConnectRails.tileSet)
        {
            if (t.name == tileName)
            {
                selectedTile = t;
                break;
            }
        }

        if (selectedTile == null) selectedTile = ConnectRails.tileSet[0];

        GameManager.spawnTile(pos, selectedTile, this);
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
    [SerializeField] private GameObject dayCycleManager;

    [SerializeField] private Tilemap railTileMap;
    [SerializeField] private Tile defaultRail;
    [SerializeField] private Tile startPointSprite;
    [SerializeField] private Tile endPointSprite;
    [SerializeField] private Tile restPointSprite;

    [SerializeField] private GameObject train;

    public Vector3Int startPoint;
    public Vector3Int endPoint;
    private GameObject _trainRef;

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
        railDataMap = registerRails();
        startPoint = findPoint(railDataMap,RailData.railTypes.start);
        endPoint = findPoint(railDataMap,RailData.railTypes.end);

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && validatePath(startPoint))
        {
            if (_trainRef == null) // Initial first train
            {
                foreach (var rail in railDataMap)
                {
                    Vector3Int pos = rail.Key;
                    RailData data = rail.Value;
                    if (data.railType == RailData.railTypes.start)
                    {
                        Vector3 worldPos = snapToGrid(pos);
                        _trainRef = Instantiate(train, worldPos, Quaternion.identity);
                        TrainMovement tm = _trainRef.GetComponent<TrainMovement>();
                        tm.gridManager = gameObject;
                        tm.dayCycleManager = dayCycleManager;
                        break;
                    }
                }
            }
            else
            {
                _trainRef.GetComponent<TrainMovement>().enabled = true;
            }

            GameStateManager.SetPhase(Phase.Travel);
        }
    }

    public void refreshRoute()
    {
        Dictionary<Vector3Int, RailData> new_RailDataMap = new Dictionary<Vector3Int, RailData>();
        new_RailDataMap = registerRails();
        // List<Vector3Int> linelist = railDataMap;
        
        Vector3Int c_endPoint = findEndPoint(startPoint);
        if (c_endPoint == startPoint) Debug.Log("Fuckyou");

        new_RailDataMap[startPoint].railType = RailData.railTypes.disabled;
        new_RailDataMap[c_endPoint].railType = RailData.railTypes.start;

        startPoint = c_endPoint;
        
        railDataMap = new_RailDataMap;
        
    }
    public Vector3Int findEndPoint(Vector3Int startPoint)
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
                Debug.Log("Last rail type: " + railDataMap[lineList[^1]].railType);
                if (lineList != null)
                {
                    if (railDataMap[lineList[0]].railType == RailData.railTypes.start && (railDataMap[lineList[^1]].railType == RailData.railTypes.end || railDataMap[lineList[^1]].railType == RailData.railTypes.rest))
                    {
                        return railDataMap[lineList[^1]].Pos;
                    }
                }

            }
        }
        return startPoint;
    }

    public void travelCheck()
    {
    if (validatePath(startPoint))
    {
        if (_trainRef == null) // Initial first train
        {
            foreach (var rail in railDataMap)
            {
                Vector3Int pos = rail.Key;
                RailData data = rail.Value;
                if (data.railType == RailData.railTypes.start)
                {
                    Vector3 worldPos = snapToGrid(pos);
                    _trainRef = Instantiate(train, worldPos, Quaternion.identity);
                    TrainMovement tm = _trainRef.GetComponent<TrainMovement>();
                    tm.gridManager = gameObject;
                    tm.dayCycleManager = dayCycleManager;
                    
                    // ADD THIS: Find and assign RestPointManager
                    RestPointManager rpm = FindObjectOfType<RestPointManager>();
                    if (rpm != null)
                    {
                        tm.restPointManager = rpm;
                        Debug.Log("RestPointManager assigned to spawned train");
                    }
                    else
                    {
                        Debug.LogError("RestPointManager not found in scene!");
                    }
                    
                    break;
                }
            }
        }
        else
        {
            _trainRef.GetComponent<TrainMovement>().enabled = true;
        }

        GameStateManager.SetPhase(Phase.Travel);
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
                Debug.Log("Last rail type: "
                + railDataMap[lineList[^1]].directionIn + " "
                + railDataMap[lineList[^1]].directionOut + " "
                + railDataMap[lineList[^1]].line + " Next is [^2]"
                + railDataMap[lineList[^2]].directionIn + " "
                + railDataMap[lineList[^2]].directionOut + " "
                + railDataMap[lineList[^2]].line + " "
                + railDataMap[lineList[^2]]
                );
                if (lineList != null)
                {
                    if (railDataMap[lineList[0]].railType == RailData.railTypes.start && (railDataMap[lineList[^1]].railType == RailData.railTypes.end || railDataMap[lineList[^1]].railType == RailData.railTypes.rest))
                    {
                        Debug.Log("Go");
                        railDataMap[lineList[0]].setDirection(new Vector2(lineList[1].x - lineList[0].x, lineList[1].y - lineList[0].y), RailData.directionType.Outgoing);
                        railDataMap[lineList[1]].setDirection(new Vector2(lineList[0].x - lineList[1].x, lineList[0].y - lineList[1].y), RailData.directionType.Incoming);
                        railDataMap[lineList[^2]].setDirection(new Vector2(lineList[^1].x - lineList[^2].x, lineList[^1].y - lineList[^2].y), RailData.directionType.Outgoing);
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

    //checks if there is a rail at position
    public bool railAtPos(Vector3Int tilePos)
    {
        return railDataMap.ContainsKey(tilePos);
    }

    //gets the rail data at the position
    public RailData GetRailAtPos(Vector3Int tilePos)
    {
        if (railDataMap.TryGetValue(tilePos, out RailData data))
        {
            return data;
        }

        return null;
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

    //used to find the coordinates of unique tiles (start,end)
    public Vector3Int findPoint(Dictionary<Vector3Int,RailData> railDataMap, RailData.railTypes railType)
    {
        if(railDataMap.Count == 0) Debug.Log("nothing in yet");

        foreach (KeyValuePair<Vector3Int, RailData> rail in railDataMap)
        {
            if(rail.Value.railType == railType)
            {
                //Debug.Log(rail.Key);
                return rail.Key;
            }
            else
            {
               // Debug.Log("cant find shit");
            }
        }
        return new Vector3Int(500,500,500);
    }

    //returns true if the tile has any connections
    public bool hasConnection(Vector3Int tilePos)
    {
        List<Vector3Int> adjacentList = getAdjacentTiles(tilePos);
        foreach (Vector3Int adjacent in adjacentList)
        {
            if (railAtPos(adjacent)) return true;
        }
        return false;
    }

    //finds if tilePos is adjacent/connected to tileSource
    public bool onConnection(Vector3Int tileSource, Vector3Int tilePos)
    {
        List<Vector3Int> adjacentList = getAdjacentTiles(tileSource);
        foreach(Vector3Int adjacent in adjacentList)
        {
            if(adjacent == tilePos) return true;
        }

        return false;
    }

    //returns a list of the adjacent tiles to the tilepos
    public List<Vector3Int> getAdjacentTiles(Vector3Int tilepos)
    {
        return new List<Vector3Int>
        {
            tilepos + new Vector3Int(1,0,0),
            tilepos + new Vector3Int(-1,0,0),
            tilepos + new Vector3Int(0,1,0),
            tilepos + new Vector3Int(0,-1,0)
        };
    }

    //register the special tiles (start,end,rest) into the dataMap at the start of the game
    private Dictionary<Vector3Int,RailData> registerRails()
    {
        Dictionary<Vector3Int,RailData> compiling_railDataMap = new Dictionary<Vector3Int, RailData>();
        BoundsInt bounds = railTileMap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                if (railTileMap.HasTile(tilePos))
                {
                    TileBase tile = railTileMap.GetTile(tilePos);

                    if (tile == startPointSprite)
                    {
                        compiling_railDataMap[tilePos] = new RailData(tilePos, RailData.railTypes.start);
                    }
                    else if (tile == endPointSprite)
                    {
                        compiling_railDataMap[tilePos] = new RailData(tilePos, RailData.railTypes.end);
                    }
                    else if (tile == restPointSprite)
                    {
                        compiling_railDataMap[tilePos] = new RailData(tilePos, RailData.railTypes.rest);
                    }
                }
            }
        }

        return compiling_railDataMap;
    }

    public void SpawnCombatTile(CombatTile combatTileSO)
    {
        List<Vector3Int> eligibleTiles = new List<Vector3Int>();

        // Collect all "normal" rails only
        foreach (var kvp in railDataMap)
        {
            RailData data = kvp.Value;
            if (data.railType == RailData.railTypes.normal)
            {
                eligibleTiles.Add(kvp.Key);
            }
        }

        if (eligibleTiles.Count == 0)
        {
            Debug.LogWarning("No eligible rails to spawn combat tile!");
            return;
        }

        // Pick a random rail
        Vector3Int randomPos = eligibleTiles[Random.Range(0, eligibleTiles.Count)];

        // Optional: visually change the tile or mark it
        Tile combatVisual = ScriptableObject.Instantiate(combatTileSO).tileVisual;
        if (combatVisual != null)
        {
            railTileMap.SetTile(randomPos, combatVisual);
        }

        // Store this for later if you want to revert after night
        Debug.Log($"Combat tile spawned at {randomPos}");
    }

    public bool IsRestTile(Vector3Int tilePos)
    {
        if (railDataMap.TryGetValue(tilePos, out RailData data))
        {
            return data.railType == RailData.railTypes.rest;
        }
        return false;
    }

}