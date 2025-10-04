// --------------------------------------------------------------
// Creation Date: 2025-09-29 22:31
// Author: User
// Description: -
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.InputSystem;

public class buildRails : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject rail;
    [SerializeField] private Sprite railPreview;
    [SerializeField] private float previewTransparency;
    [SerializeField] private LayerMask railLayer;

    //[SerializeField] private PlayerInput input;

    private int cellSize = 1; //16 pixels per 1 unit
    private Vector2 mouse;
    private SpriteRenderer railRenderer;
    private Vector2? lastPlacedPoint=null;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        railRenderer = gameObject.AddComponent<SpriteRenderer>();
        railRenderer.sortingLayerName = "Ui";
        railRenderer.sprite = railPreview;

    }

    // Update is called once per frame
    void Update()
    {
        #region showing rail preview
        mouse = Mouse.current.position.ReadValue();

        Vector3 worldMouse = Camera.main.ScreenToWorldPoint(mouse);
        worldMouse.z = 0;

        //Vector3 snapPoint = snapToGrid(worldMouse);

        float snapX = Mathf.Round(worldMouse.x / cellSize);
        float snapY = Mathf.Round(worldMouse.y / cellSize);

        Vector2 snapPoint = new Vector2(snapX, snapY);

        railRenderer.transform.position = snapPoint;

     

        if (!Physics2D.OverlapPoint(snapPoint,railLayer)) railRenderer.color = Color.cyan; else railRenderer.color = Color.red;
        Color c = railRenderer.color;
        c.a = previewTransparency;
        railRenderer.color = c;
        #endregion

        #region placing rails
        if (Input.GetMouseButton(0))
        {
            //Collider2D railCol = Physics2D.OverlapPoint(snapPoint);
            //if (railCol == null)
            //{

            if ((snapPoint != lastPlacedPoint || lastPlacedPoint == null) && !Physics2D.OverlapPoint(snapPoint,railLayer))
            {
                GameObject railIns = Instantiate(rail, new Vector2(snapX, snapY), Quaternion.identity);
                railIns.GetComponent<RailScript>().setMousePos(snapPoint);
                railIns.GetComponent<RailScript>().setPrevMousePos(lastPlacedPoint);
                //railIns.transform.position = new Vector2(snapX, snapY);
                lastPlacedPoint = snapPoint;
                //}
            }
        }
        //else
        //{
        //    lastPlacedPoint = null; 
        //}
        #endregion

        #region destroying the rails
        if (Input.GetMouseButton(1))
        {
            Collider2D col = Physics2D.OverlapPoint(snapPoint, railLayer);
            if (col != null) Destroy(col.gameObject);
        }
        #endregion
    }
    private Vector3 snapToGrid(Vector3 worldPos)
    {
        Vector3Int cellPos = grid.WorldToCell(worldPos);
        return grid.GetCellCenterWorld(cellPos);
    }
}
