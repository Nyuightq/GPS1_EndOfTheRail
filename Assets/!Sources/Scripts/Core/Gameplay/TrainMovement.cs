using Unity.VisualScripting;
using UnityEngine.Tilemaps;
using UnityEngine;

public class TrainMovement : MonoBehaviour
{
    [Header("regular movement")]
    [SerializeField] private float moveSpeed = 3.0f;
    [Header("lerp movement option")]
    [SerializeField] private bool lerpMovement = false;
    public RestPointManager restPointManager;
    [SerializeField] private float lerpAmount = 4.0f;

    public GameObject gridManager;
    public GameObject dayCycleManager;

    private RailGridScript gridScript;
    private DayCycleScript dayCycleScript;
    private TrainAnimationController animController;

    private TileBase previousBiomeTile = null;
    private Tilemap biomeTilemap;
    private Tilemap eventTilemap;

    private bool moving = false;
    private Vector2 direction;
    private Vector3Int tilePos;
    private Vector3Int targetTile;
    private RailData prevRail = null;

    private float baseSpeed;
    private bool speedModified = false;
    private bool lerpTemporarilyDisabled = false;

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

        animController = GetComponent<TrainAnimationController>();
        if (animController == null)
        {
            Debug.LogWarning("TrainAnimationController not found! Adding component...");
            animController = gameObject.AddComponent<TrainAnimationController>();
        }
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

        if (animController != null && gridScript.railAtPos(tilePos))
        {
            RailData currentRail = gridScript.railDataMap[tilePos];
            animController.UpdateDirection(currentRail.directionOut);
        }
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

                    if (animController != null)
                    {
                        animController.UpdateDirection(chosenDir);
                    }
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
                    float speedFactor = lerpAmount * Time.deltaTime * 50f * OnSpeedToggle.SpeedMultiplier;
                    transform.position = Vector3.Lerp(transform.position, targetTilePos, speedFactor);

                    float x = Util.Approach(transform.position.x, targetTilePos.x, 0.001f * OnSpeedToggle.SpeedMultiplier);
                    float y = Util.Approach(transform.position.y, targetTilePos.y, 0.001f * OnSpeedToggle.SpeedMultiplier);
                    transform.position = new Vector2(x, y);
                }
                else
                {
                    float x = Util.Approach(transform.position.x, targetTilePos.x, moveSpeed * Time.deltaTime * OnSpeedToggle.SpeedMultiplier);
                    float y = Util.Approach(transform.position.y, targetTilePos.y, moveSpeed * Time.deltaTime * OnSpeedToggle.SpeedMultiplier);

                    transform.position = new Vector2(x, y);
                }
            }

            if (Vector3.Distance(transform.position, gridScript.snapToGrid(targetTile)) < 0.001f)
            {
                if (moving)
                {
                    SoundManager.Instance.PlaySFX("SFX_TrainMovement");
                    
                    // Calculate tile movement cost based on biome
                    int tilesToAdd = 1; // default
                    
                    if (biomeTilemap != null)
                    {
                        TileBase currentBiomeTile = biomeTilemap.GetTile(tilePos);
                        
                        // Pilgrim Route: 0 tiles (always, day or night)
                        if (currentBiomeTile is PilgrimRouteTile)
                        {
                            tilesToAdd = 0;
                            Debug.Log("On PilgrimRouteTile: tile movement = 0");
                        }
                        // Swamp: 2 tiles during day only, 1 tile during night
                        else if (currentBiomeTile is BiomeSwampTile)
                        {
                            if (dayCycleScript.IsDayTime)
                            {
                                tilesToAdd = 2;
                                Debug.Log("On BiomeSwampTile (DAY): tile movement = 2");
                            }
                            else
                            {
                                tilesToAdd = 1;
                                Debug.Log("On BiomeSwampTile (NIGHT): tile movement = 1 (normal)");
                            }
                        }
                    }
                    
                    dayCycleScript.addTilesMoved(tilesToAdd);

                    CrystalDeteriorates deteriorator = GetComponent<CrystalDeteriorates>();
                    if (deteriorator != null)
                    {
                        deteriorator.OnTileMoved();
                    }
                }

                transform.position = gridScript.snapToGrid(targetTile);
                tilePos = Vector3Int.FloorToInt(gridManager.GetComponent<Grid>().WorldToCell(targetTile));

                if (eventTilemap != null)
                {
                    TileBase eventTile = eventTilemap.GetTile(tilePos);
                    if (eventTile is EventTile eTile)
                    {
                        if (!lastEventTilePos.HasValue || lastEventTilePos.Value != tilePos)
                        {
                            lastEventTilePos = tilePos;
                            transform.position = eventTilemap.GetCellCenterWorld(tilePos);
                            Debug.Log($"Train arrived at event tile {tilePos} - stopping in middle");
                            eTile.OnPlayerEnter(gameObject);
                        }
                    }
                }

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

                bool isCurrentlyOnRestPoint = gridScript.IsRestTile(tilePos);

                if (wasOnRestPoint && !isCurrentlyOnRestPoint && restPointManager != null)
                {
                    Debug.Log("Train left Rest Point - hiding UI");
                    restPointManager.OnRestPointExited();
                }

                wasOnRestPoint = isCurrentlyOnRestPoint;

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