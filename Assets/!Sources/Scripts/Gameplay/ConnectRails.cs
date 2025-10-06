// --------------------------------------------------------------
// Creation Date: 2025-10-03 07:50
// Author: Black Ninja std
// Description: -
// --------------------------------------------------------------
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using static UnityEditor.ShaderGraph.Internal.Texture2DShaderProperty;

public class ConnectRails : MonoBehaviour
{
    [SerializeField] GameObject gridManager;
    [SerializeField] Tile[] tiles;
    [SerializeField] private InputActionReference mouseHoldReference;
    [SerializeField] private float mouseDirThreshold = 0.3f;
    private Vector2 mouse;
    private Vector2 mouseDelta;
    private Vector2 prevMousePos;
    private Vector3Int prevTile;
    private Vector3Int currentTile;
    private RailGridScript gridScript;
    private Vector3Int cursorGridPos;
    //private Vector2 direction;
    
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

        mouseDelta = (mouse - prevMousePos).normalized;

        if (currentTile == prevTile && gridScript.railAtPos(currentTile))
        {
            if (mouseDelta.magnitude > mouseDirThreshold)
            {
                Vector2 dir = GetDominantDirection(mouseDelta);
                changeRail(currentTile, dir, false);
            }
        }

        if (currentTile != prevTile && gridScript.railAtPos(currentTile))
        {
            Vector2 direction = new Vector2(currentTile.x - prevTile.x, currentTile.y - prevTile.y);
            changeRail(prevTile, direction, false);

            Vector2 directionPrev = new Vector2(prevTile.x - currentTile.x, prevTile.y - currentTile.y);
            changeRail(currentTile, directionPrev, true);

            prevTile = currentTile;
        }

        prevMousePos = mouse;
    }

    private Vector2 GetDominantDirection(Vector2 delta)
    {
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return delta.x > 0 ? Vector2.right : Vector2.left;
        else
            return delta.y > 0 ? Vector2.up : Vector2.down;
    }

    private void setStartingPoint(InputAction.CallbackContext context)
    {
        mouse = Mouse.current.position.ReadValue();
        Vector3 WorldMouse = Camera.main.ScreenToWorldPoint(mouse);

        cursorGridPos = Vector3Int.FloorToInt(gridScript.snapToGrid(WorldMouse));
        prevTile = cursorGridPos;
    }

    private void changeRail(Vector3Int tilePos,Vector2 direction, bool incoming)
    {
        RailData currentRail;
        string tileName = null;

        if (gridScript.railDataMap.ContainsKey(tilePos))
        {
            currentRail = gridScript.railDataMap[tilePos];

            switch(incoming)
            {
                case true:
                    currentRail.setDirection(direction,RailData.directionType.Incoming); break;
                case false:
                    currentRail.setDirection(direction,RailData.directionType.Outgoing); break;
            }
            //Debug.Log(currentRail.getConnection());
            tileName = $"rail_{currentRail.getConnection()}";
        }
        Tile selectedTile = null;

        if (tileName != null)
        {
            foreach (Tile tile in tiles)
            {
                if (tile.name == tileName)
                {
                    selectedTile = tile;
                    break;
                }
            }
        }
        else selectedTile = tiles[0];

        RailData data = gridScript.railDataMap[tilePos];
        if (gridScript.railDataMap[tilePos].railType == RailData.railTypes.normal) GameManager.spawnTile(tilePos, selectedTile, data);
    }
}
