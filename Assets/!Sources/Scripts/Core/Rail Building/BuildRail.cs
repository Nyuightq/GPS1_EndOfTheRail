// --------------------------------------------------------------
// Creation Date: 2025-10-03 01:58
// Author: User
// Description: -
// --------------------------------------------------------------
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class BuildRails : MonoBehaviour
{
    [Header("tileMaps/Grid Manager")]
    [SerializeField] private GameObject gridManager;
    [SerializeField] private Tile defaultTile;
    [SerializeField] private Tilemap eventTilemap;
    [Header("rail preview")]
    [SerializeField] private Sprite railPreview;
    [SerializeField] private float previewTransparency;
    [SerializeField] private LayerMask railLayer;

    [SerializeField] private InputActionReference mouseHoldReference;

    private Grid grid;
    private RailGridScript gridScript;
    private Vector2 mouse;
    private SpriteRenderer previewRenderer;

    private RailLine railLine;
    private bool canBuild = false;
    private bool isHolding = false;
    private Vector3Int? _lastBuiltRail = null;

    private void OnMouseHoldPerformed(InputAction.CallbackContext ctx)
    {
        isHolding = true;
    }
    private void OnMouseHoldCanceled(InputAction.CallbackContext ctx)
    {
        isHolding = false;
    }

    #region Unity Life Cycle
    private void Awake()
    {
        grid = gridManager.GetComponent<Grid>();
        gridScript = gridManager.GetComponent<RailGridScript>();
        previewRenderer = gameObject.AddComponent<SpriteRenderer>();
        previewRenderer.sortingLayerName = "Ui";
        previewRenderer.sprite = railPreview;
    }

    private void OnEnable()
    {
        gridScript.OnRefreshRoute += RefreshBuildRail;
    }

    private void OnDisable()
    {
        gridScript.OnRefreshRoute -= RefreshBuildRail;
    }

    void Update()
    {
        if (gridScript == null) return;

        // =====================
        // Variables Declaration 
        // =====================
        Vector3Int tilePos = ReadMousePointingSnapPoint();
        bool onRail = gridScript.railAtPos(tilePos);
        if (_lastBuiltRail != null)
        {
            railLine = gridScript.railDataMap[(Vector3Int)_lastBuiltRail].line;
        }

        TileBase eventTile = eventTilemap.GetTile(tilePos);
        bool isNonTraversable = eventTile != null /*eventTile is NonTraversableTile || eventTile is NonTraversableRuleTile*/ || gridScript.railAtPosIsDisabled(tilePos); // First condition of nonTraversable tile

        if (Input.GetMouseButton(0)) isHolding = true; else isHolding = false;

        bool emptyStart = !gridScript.hasConnection(gridScript.startPoint);
        bool validConnection = IsValidConnection(tilePos);
        canBuild = !onRail && !isNonTraversable && validConnection;
        // =====================
        // Variables Declaration 
        // =====================
        RenderPreviewRail(onRail, isNonTraversable);

        // =====================
        // Handle Building Input and check is buildable
        // =====================
        if (isHolding && canBuild)
        {
            if (gridScript.onConnection(gridScript.startPoint, tilePos) && emptyStart)
            {
                railLine = new RailLine();
                railLine.line.Add(gridScript.startPoint);
            }
            if (emptyStart) Debug.Log("Empty Start");
            RailData data = new RailData(tilePos);

            GameManager.spawnTile(tilePos, defaultTile, data);
            gridScript.railDataMap[tilePos].setLine(railLine);
            railLine.line.Add(tilePos);

            Debug.Log($"Tile {tilePos} assigned to line {railLine.GetHashCode()}");

            if (emptyStart)
            {
                HandleStartTileConnection(tilePos);
            }
            else
            {
                HandlePreviousTileConnection(tilePos);
            }
            _lastBuiltRail = tilePos;

            Debug.Log($"Tile {tilePos} assigned to line {gridScript.railDataMap[tilePos].directionIn}, {gridScript.railDataMap[tilePos].directionOut}");

            //register rest point tile
            HandleRestTileConnection(tilePos);
            HandleEndTileConnection(tilePos);
        }

        HandleDeleteInput(tilePos);
    }
    #endregion
    
    private void RefreshBuildRail()
    {
        // The rail building related data is reset due to Phase state is change and make the BuildRail being disabled and enable
        // Might still have some problem happens if keep it here.
        _lastBuiltRail = null;
        railLine = null;
    }

    #region Mouse Preview & Build State
    private Vector3Int ReadMousePointingSnapPoint()
    {
        mouse = Mouse.current.position.ReadValue();

        Vector3 worldMouse = Camera.main.ScreenToWorldPoint(mouse);
        worldMouse.z = 0;

        Vector3 snapPoint = gridScript.snapToGrid(worldMouse);
        previewRenderer.transform.position = snapPoint; // Update preview renderer position

        return Vector3Int.FloorToInt(snapPoint);
    }

    private void RenderPreviewRail(bool isOnRail, bool isNonTraversable)
    {
        if (!isOnRail && !isNonTraversable && canBuild)
        {
            previewRenderer.color = Color.cyan;
        }
        else
        {
            previewRenderer.color = Color.red;
        }

        Color c = previewRenderer.color;
        c.a = previewTransparency;
        previewRenderer.color = c;
    }
    #endregion
    // ==================
    #region Rail Building
    private bool IsValidConnection(Vector3Int tilePos)
    {
        bool emptyStart = !gridScript.hasConnection(gridScript.startPoint);

        // Check if starting from start point
        if (gridScript.onConnection(gridScript.startPoint, tilePos) && emptyStart)
        {
            return true;
        }

        if (railLine == null) return false;
        // Check if continuing an existing line safely
        if (railLine?.line != null && railLine?.line.Count > 0)
        {
            Vector3Int lastTile = (Vector3Int)_lastBuiltRail;
            RailData lastData = gridScript.railDataMap[lastTile];

            bool isExtendable = lastData.railType != RailData.railTypes.end && lastData.railType != RailData.railTypes.rest;

            if (gridScript.onConnection(lastTile, tilePos) && isExtendable)
            {
                return true;
            }
        }

        return false;
    }

    private void HandleStartTileConnection(Vector3Int tilePos)
    {
        bool emptyStart = !gridScript.hasConnection(gridScript.startPoint);
        if (gridScript.onConnection(gridScript.startPoint, tilePos))
        {
            Vector2 dirFromStart = new Vector2(tilePos.x - gridScript.startPoint.x, tilePos.y - gridScript.startPoint.y);
            Vector2 dirToStart = new Vector2(gridScript.startPoint.x - tilePos.x, gridScript.startPoint.y - tilePos.y);

            gridScript.railDataMap[gridScript.startPoint].setDirection(dirFromStart, RailData.directionType.Outgoing);
            gridScript.railDataMap[tilePos].setDirection(dirToStart, RailData.directionType.Incoming);
        }
    }
    
    private void HandlePreviousTileConnection(Vector3Int tilePos)
    {
        foreach (Vector3Int adjacent in gridScript.getAdjacentTiles(tilePos))
        {
            if (gridScript.railAtPos(adjacent))
            {
                RailData adjacentData = gridScript.railDataMap[adjacent];
                if (adjacentData.line == railLine && adjacent == _lastBuiltRail)
                {
                    Vector2 dirToAdjacent = new Vector2(adjacent.x - tilePos.x, adjacent.y - tilePos.y);
                    Vector2 dirFromAdjacent = new Vector2(tilePos.x - adjacent.x, tilePos.y - adjacent.y);

                    gridScript.railDataMap[adjacent].setDirection(dirFromAdjacent, RailData.directionType.Outgoing);
                    gridScript.railDataMap[tilePos].setDirection(dirToAdjacent, RailData.directionType.Incoming);
                    return;
                }
            }
        }
    }

    private void HandleRestTileConnection(Vector3Int tilePos)
    {
        foreach (var Rail in gridScript.railDataMap)
        {
            if (Rail.Value.railType == RailData.railTypes.rest && gridScript.onConnection(Rail.Key, tilePos))
            {
                railLine.line.Add(Rail.Key);
                Vector3Int restTile = railLine.line[^1];
                Vector3Int prevTile = railLine.line[^2];

                Vector2 dirToRest = new Vector2(restTile.x - prevTile.x, restTile.y - prevTile.y);
                Vector2 dirFromRest = -new Vector2(restTile.x - prevTile.x, restTile.y - prevTile.y);

                gridScript.railDataMap[restTile].setDirection(dirFromRest, RailData.directionType.Incoming);
                gridScript.railDataMap[prevTile].setDirection(dirToRest, RailData.directionType.Outgoing);
                return;
            }
        }
    }

    private void HandleEndTileConnection(Vector3Int tilePos)
    {
        if (gridScript.onConnection(gridScript.endPoint, tilePos))
        {
            railLine.line.Add(gridScript.endPoint);
            Vector3Int prevRail = railLine.line[^2];
            Vector3Int currentRail = railLine.line[^1];
            Vector2 newDir = new Vector2(currentRail.x - prevRail.x, currentRail.y - prevRail.y);
            gridScript.railDataMap[railLine.line[^2]].setDirection(newDir, RailData.directionType.Outgoing);
        }
    }
    #endregion
    // ==================
    #region Rail Deletetion
    /// <summary>
    /// Handles deletion of the last placed rail when right-clicking.
    /// </summary>
    private void HandleDeleteInput(Vector3Int tilePos)
    {
        if (!Input.GetMouseButton(1)) return;
        DeleteCertainRail(tilePos);
    }

    private void DeleteCertainRail(Vector3Int tilePos)
    {
        Vector3Int lastTile = tilePos;

        if (!gridScript.railDataMap.ContainsKey(lastTile)) return;

        RailData lastData = gridScript.railDataMap[lastTile];
        RailLine currentLine = lastData.line;

        if (currentLine == null || currentLine.line.Count == 0) return;

        // record IncomingDirection（ComingFromDirection）
        Vector3Int? previousRail = GetSingleConnectedRail(lastTile, true);
        Vector3Int? nextRail = GetSingleConnectedRail(lastTile, false);
        Debug.Log("Origin " + lastTile + " PreviousRail: " + previousRail + " NextRail: " + nextRail);

        // delete last Rail Tile
        GameManager.DestroyTile(lastTile);
        currentLine.line.Remove(lastTile);

        // Loop until nextRail is clear
        while (nextRail != null)
        {
            Vector3Int nextRailVector3Int = (Vector3Int)nextRail;
            RailData checkingRail = gridScript.railDataMap[nextRailVector3Int];
            bool isNextRailSpecial = checkingRail.railType == RailData.railTypes.rest || checkingRail.railType == RailData.railTypes.end;

            if (isNextRailSpecial)
            {
                nextRail = null;
                currentLine.line.Remove(nextRailVector3Int);
            }
            else
            {
                nextRail = GetSingleConnectedRail(nextRailVector3Int, false);
                GameManager.DestroyTile(nextRailVector3Int);
                currentLine.line.Remove(nextRailVector3Int);
            }
        }

        Debug.Log($"Deleted tile: {lastTile}");

        // Use ComingFromDirection to find previous tile connected to deleted tile
        if (previousRail.HasValue)
        {
            _lastBuiltRail = previousRail;
            Debug.Log($"_lastBuiltRail set to previous connected tile: {_lastBuiltRail}");
        }
        else
        {
            _lastBuiltRail = null;
        }

        // Remove the relation of lastBuiltRail connected with rest/end tile
        if (currentLine.line.Count > 0)
        {
            Vector3Int tailTile = currentLine.line[^1];

            if (gridScript.railDataMap.ContainsKey(tailTile))
            {
                RailData tailData = gridScript.railDataMap[tailTile];
                if (tailData.railType == RailData.railTypes.rest || tailData.railType == RailData.railTypes.end)
                {
                    Debug.Log($"Removed trailing {tailData.railType} tile at {tailTile}");
                    currentLine.line.Remove(tailTile);
                }
            }
        }
    }
    #endregion

    /// <summary>
    /// Returns the connected rail position based on the tile's IncomingDirection.
    /// </summary>
    private Vector3Int? GetSingleConnectedRail(Vector3Int currentTile, bool isDirectionIn = true)
    {
        if (!gridScript.railDataMap.ContainsKey(currentTile)) return null;

        RailData data = gridScript.railDataMap[currentTile];
        Vector2? targetDir = isDirectionIn ?
            data.directionIn :
            data.directionOut;

        if (!targetDir.HasValue) return null;

        Vector2 dir = targetDir.Value;
        Vector3Int nextTargetTilePos = new Vector3Int(
            currentTile.x + (int)dir.x,
            currentTile.y + (int)dir.y,
            currentTile.z
        );

        if (gridScript.railDataMap.ContainsKey(nextTargetTilePos))
        {
            if (currentTile == nextTargetTilePos) return null;
            return nextTargetTilePos;
        }

        return null;
    }
}