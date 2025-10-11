// --------------------------------------------------------------
// Creation Date: 2025-10-03 07:50
// Author: Black Ninja std
// Description: -
// --------------------------------------------------------------
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using static UnityEditor.ShaderGraph.Internal.Texture2DShaderProperty;

public class ConnectRails : MonoBehaviour
{
    [SerializeField] private GameObject gridManager;
    [SerializeField] private Tile[] tiles;
    [SerializeField] private InputActionReference mouseHoldReference;
    //[SerializeField] private float mouseDirThreshold = 0.3f;
    private Vector2 mouse;
    private Vector2 mouseDelta;
    private Vector2 prevMousePos;
    private Vector3Int prevTile;
    private Vector3Int currentTile;
    private RailGridScript gridScript;
    private Vector3Int cursorGridPos;

    public static Tile[] tileSet;
    //private Vector2 direction;

    private void Awake()
    {
        tileSet = tiles;
    }

    private void OnEnable()
    {
        mouseHoldReference.action.started += setStartingPoint;
    }

    private void OnDisable()
    {
        mouseHoldReference.action.started -= setStartingPoint;
    }

    void Start()
    {
        gridScript = gridManager.GetComponent<RailGridScript>();
    }
    
    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            settingDirection();
        }
    }

    private void settingDirection()
    {
        Vector2 mouse = Mouse.current.position.ReadValue();
        Vector3 worldMouse = Camera.main.ScreenToWorldPoint(mouse);
        worldMouse.z = 0f;

        currentTile = Vector3Int.FloorToInt(gridScript.snapToGrid(worldMouse));
       

        if (currentTile != prevTile && gridScript.railAtPos(currentTile) && gridScript.railDataMap[currentTile].directionIn == Vector2.zero)
        {
            Debug.Log("look");

            Vector2 direction = new Vector2(currentTile.x - prevTile.x, currentTile.y - prevTile.y);
            changeRail(prevTile, direction, false);

            Vector2 directionPrev = new Vector2(prevTile.x - currentTile.x, prevTile.y - currentTile.y);
            changeRail(currentTile, directionPrev, true);

            prevTile = currentTile;
        }

        prevMousePos = mouse;
    }

    private void connectLine(List<Vector3Int> line)
    {
        for(int i =0;i<line.Count;i++)
        {
            if(i!=0)
            {
                Vector2 direction = new Vector2(line[i].x - line[i-1].x, line[i].y - line[i-1].y);
                changeRail(line[i-1], direction, false);

                Vector2 directionPrev = new Vector2(line[i - 1].x - line[i].x, line[i - 1].y - line[i].y);
                changeRail(line[i], directionPrev, true);
            }
        }
    }

    private void setStartingPoint(InputAction.CallbackContext context)
    {
        mouse = Mouse.current.position.ReadValue();
        Vector3 WorldMouse = Camera.main.ScreenToWorldPoint(mouse);

        cursorGridPos = Vector3Int.FloorToInt(gridScript.snapToGrid(WorldMouse));
        prevTile = cursorGridPos;
    }

    public void changeRail(Vector3Int tilePos,Vector2 direction, bool incoming)
    {
        if (gridScript.railDataMap[tilePos].railType == RailData.railTypes.normal)
        {
            RailData currentRail;
            //string tileName = null;
            RailLine line = gridScript.railDataMap[tilePos].line;

            bool tileAtLineEnd; //checks for the last,second last && third last(if validated) in the rail line

            if (gridScript.railDataMap[line.line[^1]].railType == RailData.railTypes.normal)
            {
                tileAtLineEnd = (tilePos == line.line[^1] || tilePos == line.line[^2]);
            }
            else
            {
                tileAtLineEnd = (tilePos == line.line[^2] || tilePos == line.line[^3] || tilePos == line.line[^4]);
            }

            if (gridScript.railDataMap.ContainsKey(tilePos) && (gridScript.railDataMap[tilePos].directionIn == Vector2.zero || gridScript.railDataMap[tilePos].directionOut == Vector2.zero) && tileAtLineEnd)
            {
                currentRail = gridScript.railDataMap[tilePos];

                switch (incoming)
                {
                    case true:
                        currentRail.setDirection(direction, RailData.directionType.Incoming); break;
                    case false:
                        currentRail.setDirection(direction, RailData.directionType.Outgoing); break;
                }
            }
        }
    }
}
