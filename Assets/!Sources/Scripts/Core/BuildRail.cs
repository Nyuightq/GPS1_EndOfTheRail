// --------------------------------------------------------------
// Creation Date: 2025-10-03 01:58
// Author: User
// Description: -
// --------------------------------------------------------------
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class BuildRails : MonoBehaviour
{
    [SerializeField] GameObject gridManager;
    [SerializeField] private Sprite railPreview;
    [SerializeField] private float previewTransparency;
    [SerializeField] private LayerMask railLayer;
    [SerializeField] private Tile defaultTile;

    private Grid grid;
    private RailGridScript gridScript;
    private Vector2 mouse;
    private SpriteRenderer previewRenderer;

    void Start()
    {
        gridScript = gridManager.GetComponent<RailGridScript>();
        previewRenderer = gameObject.AddComponent<SpriteRenderer>();
        previewRenderer.sortingLayerName = "Ui";
        previewRenderer.sprite = railPreview;
    }

    private void Awake()
    {
        grid = gridManager.GetComponent<Grid>();
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

        if (!onRail) previewRenderer.color = Color.cyan; else previewRenderer.color = Color.red;

        Color c = previewRenderer.color;
        c.a = previewTransparency;
        previewRenderer.color = c;



        if(Input.GetMouseButton(0) && !onRail)
        {
            Debug.Log("WOO THERES A TILE THAT SPAWNED!!!");

            RailData data = new RailData();
            GameManager.spawnTile(tilePos, defaultTile, data);
        }
    }


}
