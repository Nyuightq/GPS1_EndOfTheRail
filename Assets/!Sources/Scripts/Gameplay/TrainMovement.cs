using Unity.VisualScripting;
using UnityEngine.Tilemaps;
using UnityEngine;

public class TrainMovement : MonoBehaviour
{
    [Header("regular movement")]
    [SerializeField] private float moveSpeed = 3.0f; //it was 2.0f
    [Header("lerp movement option")]
    [SerializeField] private bool lerpMovement = false;
    public RestPointManager restPointManager;
    [SerializeField] private float lerpAmount = 4.0f; //it was 0.1f

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
    private bool wasOnRestPoint = false;

    private void Awake()
    {
        baseSpeed = moveSpeed;

        restPointManager = FindFirstObjectByType<RestPointManager>();
        if (restPointManager == null)
            Debug.LogError("No RestPointManager found before train spawn!");
        else
            Debug.Log("Found RestPointManager: " + restPointManager.name);

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

            if (restPointManager == null)
            {
                restPointManager = FindFirstObjectByType<RestPointManager>();
                if (restPointManager == null)
                {
                    Debug.LogError("RestPointManager is NULL! Please assign it in the inspector.");
                    return;
                }
            }
        // Stop all updates when the game is paused
        if (Mathf.Approximately(Time.timeScale, 0f) && moving)
        {
            return;
        }

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
                    //transform.position = Vector3.Lerp(transform.position, targetTilePos, lerpAmount);
                    //transform.position = Vector3.Lerp(transform.position, targetTilePos, lerpAmount * OnSpeedToggle.SpeedMultiplier);
                    transform.position = Vector3.Lerp(transform.position, targetTilePos, lerpAmount * Time.deltaTime * 50f * OnSpeedToggle.SpeedMultiplier);

                    float x = Util.Approach(transform.position.x, targetTilePos.x, 0.001f);
                    float y = Util.Approach(transform.position.y, targetTilePos.y, 0.001f);
                    transform.position = new Vector2(x, y);
                }
                else
                {
                    //float x = Util.Approach(transform.position.x, targetTilePos.x, moveSpeed * Time.deltaTime);
                    //float y = Util.Approach(transform.position.y, targetTilePos.y, moveSpeed * Time.deltaTime);
                    float x = Util.Approach(transform.position.x, targetTilePos.x, moveSpeed * Time.deltaTime * OnSpeedToggle.SpeedMultiplier);
                    float y = Util.Approach(transform.position.y, targetTilePos.y, moveSpeed * Time.deltaTime * OnSpeedToggle.SpeedMultiplier);

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

                // Check if collided to Rest point
                moving = false;

                // Check if we LEFT a rest point (we were on one, now we're not)
                bool isCurrentlyOnRestPoint = gridScript.IsRestTile(tilePos);

                if (wasOnRestPoint && !isCurrentlyOnRestPoint && restPointManager != null)
                {
                    Debug.Log("Train left Rest Point - hiding UI");
                    restPointManager.OnRestPointExited();
                }

                wasOnRestPoint = isCurrentlyOnRestPoint;

                // Check if collided to Rest point
                if (gridScript.IsRestTile(tilePos))
                {
                    Debug.Log("Reached Rest Point - Entering Plan Phase");
                    
                    if (restPointManager == null)
                    {
                        Debug.LogError("RestPointManager is NULL! Please assign it in the inspector.");
                    }
                    else
                    {
                        Debug.Log("Calling restPointManager.OnRestPointEntered()");
                        restPointManager.OnRestPointEntered(this);
                    }
                    
                    GameStateManager.SetPhase(Phase.Plan);
                    gridScript.refreshRoute();
                    enabled = false;
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