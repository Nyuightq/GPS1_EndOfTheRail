using Unity.VisualScripting;
using UnityEngine.Tilemaps;
using UnityEngine;

public class TrainMovement : MonoBehaviour
{
    [Header("regular movement")]
    [SerializeField] private float moveSpeed = 2.0f;
    [Header("lerp movement option")]
    [SerializeField] private bool lerpMovement = false;
    [SerializeField] private float lerpAmount = 0.1f;

    public GameObject gridManager;
    public GameObject dayCycleManager;

    private RailGridScript gridScript;
    private DayCycleScript dayCycleScript;

    private TileBase previousBiomeTile = null;
    private Tilemap biomeTilemap;
    private Tilemap eventTilemap; // NEW: Cache the event tilemap

    private bool moving = false;
    private Vector2 direction;
    private Vector3Int tilePos;
    private Vector3Int targetTile;
    private RailData prevRail = null;

    private float baseSpeed;
    private bool speedModified = false;
    private bool lerpTemporarilyDisabled = false;

    // NEW: Track if we've already triggered event tile for current position
    private Vector3Int? lastEventTilePos = null;

    private void Awake()
    {
        baseSpeed = moveSpeed;
    }

    public void ApplySpeedModifier(float reduction)
    {
        if (speedModified) return;
        moveSpeed = Mathf.Max(0.1f, moveSpeed - reduction);
        speedModified = true;
        lerpTemporarilyDisabled = true;
        lerpMovement = false;
        Debug.Log($"Train speed modified to {moveSpeed}, lerp disabled");
    }

    public void ResetSpeedModifier()
    {
        if (!speedModified) return;
        moveSpeed = baseSpeed;
        speedModified = false;

        if (lerpTemporarilyDisabled)
        {
            lerpMovement = true;
            lerpTemporarilyDisabled = false;
        }
        Debug.Log($"Train speed reset to {moveSpeed}, lerp re-enabled");
    }

    public Vector3Int GetTilePos()
    {
        return tilePos;
    }

    public Vector2 GetForwardDirection()
    {
        RailGridScript grid = gridManager.GetComponent<RailGridScript>();
        if (grid != null && grid.railAtPos(tilePos))
        {
            RailData rd = grid.GetRailAtPos(tilePos);
            if (rd != null)
                return rd.directionOut;
        }
        return Vector2.right;
    }

    void Start()
    {
        gridScript = gridManager.GetComponent<RailGridScript>();
        dayCycleScript = dayCycleManager.GetComponent<DayCycleScript>();
        biomeTilemap = gridManager.GetComponentInChildren<Tilemap>();
        
        // NEW: Find EventTilemap specifically
        GameObject eventTilemapObj = GameObject.Find("EventTilemap");
        if (eventTilemapObj != null)
        {
            eventTilemap = eventTilemapObj.GetComponent<Tilemap>();
            Debug.Log("EventTilemap found and cached");
        }
        else
        {
            Debug.LogWarning("EventTilemap not found!");
        }
        
        tilePos = Vector3Int.FloorToInt(transform.position);
        transform.position = gridScript.snapToGrid(transform.position);
    }

    private Vector3Int Vec2ToInt(Vector2 v)
    {
        return new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), 0);
    }

    void Update()
    {
        if (!moving)
        {
            if (gridScript.railAtPos(tilePos))
            {
                RailData currentRail = gridScript.railDataMap[tilePos];

                if (currentRail.railType != RailData.railTypes.end && currentRail.railType != RailData.railTypes.rest)
                {
                    Vector2 chosenDir;

                    if (prevRail != null)
                    {
                        chosenDir = currentRail.directionOut;
                    }
                    else
                    {
                        chosenDir = currentRail.directionOut;
                    }

                    targetTile = tilePos + Vec2ToInt(chosenDir);
                    prevRail = currentRail;
                    moving = true;
                }
            }
        }
        else
        {
            if (gridScript.railAtPos(targetTile))
            {
                Vector3 targetTilePos = gridScript.snapToGrid(targetTile);

                if (lerpMovement)
                {
                    transform.position = Vector3.Lerp(transform.position, targetTilePos, lerpAmount);
                    float x = Util.Approach(transform.position.x, targetTilePos.x, 0.001f);
                    float y = Util.Approach(transform.position.y, targetTilePos.y, 0.001f);
                    transform.position = new Vector2(x, y);
                }
                else
                {
                    float x = Util.Approach(transform.position.x, targetTilePos.x, moveSpeed * Time.deltaTime);
                    float y = Util.Approach(transform.position.y, targetTilePos.y, moveSpeed * Time.deltaTime);
                    transform.position = new Vector2(x, y);
                }
            }

            if (Vector3.Distance(transform.position, gridScript.snapToGrid(targetTile)) < 0.001f)
            {
                if (moving)
                {
                    dayCycleScript.addTilesMoved(1);

                    CrystalDeteriorates deteriorator = GetComponent<CrystalDeteriorates>();
                    if (deteriorator != null)
                    {
                        deteriorator.OnTileMoved();
                    }
                }

                transform.position = gridScript.snapToGrid(targetTile);
                tilePos = Vector3Int.FloorToInt(gridManager.GetComponent<Grid>().WorldToCell(targetTile));

                // NEW: Check for event tile at CURRENT position (we just arrived)
                if (eventTilemap != null)
                {
                    TileBase eventTile = eventTilemap.GetTile(tilePos);
                    if (eventTile is EventTile eTile)
                    {
                        if (!lastEventTilePos.HasValue || lastEventTilePos.Value != tilePos)
                        {
                            lastEventTilePos = tilePos;
                            
                            // Force snap to exact center before triggering event
                            transform.position = eventTilemap.GetCellCenterWorld(tilePos);
                            
                            Debug.Log($"Train arrived at event tile {tilePos} - stopping in middle");
                            eTile.OnPlayerEnter(gameObject);
                        }
                    }
                }

                // Check for biome tile enter/exit
                if (biomeTilemap != null)
                {
                    TileBase currentBiomeTile = biomeTilemap.GetTile(tilePos);

                    if (previousBiomeTile != currentBiomeTile)
                    {
                        if (previousBiomeTile is EventTile prevTile)
                        {
                            prevTile.OnPlayerExit(gameObject);
                            Debug.Log("Train exited biome tile.");
                        }

                        if (currentBiomeTile is EventTile newTile)
                        {
                            newTile.OnPlayerEnter(gameObject);
                            Debug.Log("Train entered biome tile.");
                        }

                        previousBiomeTile = currentBiomeTile;
                    }
                }

                moving = false;

                // Check if collided to Rest point
                if (gridScript.railAtPos(tilePos))
                {
                    RailData currentRail = gridScript.railDataMap[tilePos];
                    if (currentRail.railType == RailData.railTypes.rest)
                    {
                        Debug.Log("Phase - Plan");
                        GameStateManager.SetPhase(Phase.Plan);
                        gridScript.refreshRoute();
                        GetComponent<TrainMovement>().enabled = false;
                    }
                }
            }
        }
    }

    // NEW: Check if there's an event tile at a specific position
    private bool HasEventTileAtPosition(Vector3Int position)
    {
        if (eventTilemap != null)
        {
            TileBase tile = eventTilemap.GetTile(position);
            bool hasEventTile = tile is EventTile;
            
            if (hasEventTile)
            {
                Debug.Log($"Found EventTile at {position}: {tile.name}");
            }
            
            return hasEventTile;
        }
        return false;
    }
}