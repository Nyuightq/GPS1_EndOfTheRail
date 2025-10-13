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
    [SerializeField] private Tilemap eventTilemap; //NEW
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

    private void OnMouseHoldPerformed(InputAction.CallbackContext ctx)
    {
        isHolding = true;
    }
    private void OnMouseHoldCanceled(InputAction.CallbackContext ctx)
    {
        isHolding = false;
    }
    private void Awake()
    {
        grid = gridManager.GetComponent<Grid>();
        gridScript = gridManager.GetComponent<RailGridScript>();
        previewRenderer = gameObject.AddComponent<SpriteRenderer>();
        previewRenderer.sortingLayerName = "Ui";
        previewRenderer.sprite = railPreview;
    }

    void Update()
    {
        mouse = Mouse.current.position.ReadValue();

        Vector3 worldMouse = Camera.main.ScreenToWorldPoint(mouse);
        worldMouse.z = 0;

        Vector3 snapPoint = gridScript.snapToGrid(worldMouse);
        previewRenderer.transform.position = snapPoint;

        Vector3Int tilePos = Vector3Int.FloorToInt(snapPoint);

        bool onRail = gridScript.railAtPos(tilePos);


        TileBase eventTile = eventTilemap.GetTile(tilePos); //NEW
        bool isNonTraversable = eventTile is NonTraversableTile;//NEW

        if (!onRail && !isNonTraversable && canBuild/*NEW*/) previewRenderer.color = Color.cyan; else previewRenderer.color = Color.red;

        Color c = previewRenderer.color;
        c.a = previewTransparency;
        previewRenderer.color = c;

        if (Input.GetMouseButton(0)) isHolding = true; else isHolding = false;

        if (gridScript.railAtPos(tilePos)) railLine = gridScript.railDataMap[tilePos].line;

        bool emptyStart = !gridScript.hasConnection(gridScript.startPoint);
        bool validConnection = false;

        // Safety checks for nulls
        if (gridScript != null)
        {
            // Check if starting from start point
            if (gridScript.onConnection(gridScript.startPoint, tilePos) && emptyStart)
            {
                validConnection = true;
            }
            // Check if continuing an existing line safely
            else if (railLine != null && railLine.line != null && railLine.line.Count > 0)
            {
                
                Vector3Int lastTile = railLine.line[^1];
                RailData lastData = gridScript.railDataMap[lastTile];

                bool canExtend = lastData.railType != RailData.railTypes.end;

                Debug.Log(gridScript.railDataMap[lastTile].railType);
                if (gridScript.onConnection(lastTile, tilePos) && canExtend)
                {
                    validConnection = true;
                    //Debug.Log("valid");
                }
                else if (canExtend)
                {
                    // Only check if tilePos is directly next to the last tile
                    if (gridScript.onConnection(lastTile, tilePos))
                    {
                        validConnection = true;
                    }
                }
            }
        }

        canBuild = (!onRail && !isNonTraversable && validConnection);

        //building
        if (isHolding && canBuild)
        {
            //Debug.Log("WOO THERES A TILE THAT SPAWNED!!!");
            if (gridScript.onConnection(gridScript.startPoint, tilePos) && emptyStart)
            {
                railLine = new RailLine();
                railLine.line.Add(gridScript.startPoint);
            }
            RailData data = new RailData(tilePos);

            GameManager.spawnTile(tilePos, defaultTile, data);
            gridScript.railDataMap[tilePos].setLine(railLine);

            Debug.Log($"Tile {tilePos} assigned to line {railLine.GetHashCode()}");
            railLine.line.Add(tilePos);

            foreach (Vector3Int adjacent in gridScript.getAdjacentTiles(tilePos))
            {
                if(gridScript.railAtPos(adjacent))
                {
                    RailData adjacentData = gridScript.railDataMap[adjacent];
                    if (adjacentData.line == railLine && adjacent == railLine.line[^2])
                    {
                        Vector2 dirToAdjacent = new Vector2(adjacent.x-tilePos.x,adjacent.y-tilePos.y);
                        Vector2 dirFromAdjacent = new Vector2(tilePos.x - adjacent.x, tilePos.y - adjacent.y);

                        gridScript.railDataMap[adjacent].setDirection(dirFromAdjacent, RailData.directionType.Outgoing);
                        gridScript.railDataMap[tilePos].setDirection(dirToAdjacent, RailData.directionType.Incoming);
                        break;
                    }
                }
            }

            //register rest point tile
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
                    break;
                }
            }

            //register end tile
            if (gridScript.onConnection(gridScript.endPoint, tilePos))
            {
                railLine.line.Add(gridScript.endPoint);
                Vector3Int prevRail = railLine.line[^2];
                Vector3Int currentRail = railLine.line[^1];
                Vector2 newDir = new Vector2(currentRail.x-prevRail.x,currentRail.y-prevRail.y);
                gridScript.railDataMap[railLine.line[^2]].setDirection(newDir,RailData.directionType.Outgoing);
            }
        }

        if (Input.GetMouseButtonDown(1))//&& onRail && gridScript.railDataMap[tilePos].railType == RailData.railTypes.normal)
        {
            if(railLine != null && railLine.line.Count > 1)
            {
                if (gridScript.railDataMap[railLine.line[^1]].railType == RailData.railTypes.normal)
                {
                    GameManager.DestroyTile(railLine.line[^1]);
                    railLine.line.Remove(railLine.line[^1]);
                }
                else
                {
                    if (gridScript.railDataMap[railLine.line[^2]].railType == RailData.railTypes.normal)
                    {
                        GameManager.DestroyTile(railLine.line[^2]);
                        railLine.line.Remove(railLine.line[^2]);
                    }
                    railLine.line.Remove(railLine.line[^1]);
                }
            } 
        }
    }

}
