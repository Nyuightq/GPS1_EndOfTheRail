// --------------------------------------------------------------
// Creation Date: 2025-10-04 12:24
// Author: User
// Description: -
// --------------------------------------------------------------
using Unity.VisualScripting;
using UnityEngine;


public class TrainMovement : MonoBehaviour
{
    [Header("regular movement")]
    [SerializeField] private float moveSpeed = 0.02f;
    [Header("lerp movement option")]
    [SerializeField] private bool lerpMovement = false;
    [SerializeField] private float lerpAmount = 0.1f;

    public GameObject gridManager;
    public GameObject dayCycleManager;

    private RailGridScript gridScript;
    private DayCycleScript dayCycleScript;

    private bool moving = false;

    //private bool reversed = false;
    private Vector2 direction;
    private Vector3Int tilePos;
    private Vector3Int targetTile;
    private RailData prevRail = null;
    void Start()
    {
        gridScript = gridManager.GetComponent<RailGridScript>();
        dayCycleScript = dayCycleManager.GetComponent<DayCycleScript>();

        tilePos = Vector3Int.FloorToInt(transform.position);

        transform.position = gridScript.snapToGrid(transform.position);
    }

    private Vector3Int Vec2ToInt(Vector2 v)
    {
        return new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), 0);
    }
    void Update()
    {
        if(!moving)
        {
            if (gridScript.railAtPos(tilePos))
            {
                RailData currentRail = gridScript.railDataMap[tilePos];

                // decide which exit to take
                Vector2 prevRailDir = Vector2.zero, currentRailDir = Vector2.zero;
                Vector2 chosenDir;

                if (currentRail.railType != RailData.railTypes.end)
                {
                    if (prevRail != null)
                    {

                        chosenDir = currentRail.directionOut;
                    }
                    else
                    {
                        chosenDir = currentRail.directionOut;
                    }

                    // convert chosenDir to integer tile offset and set target
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
                    float x = Util.Approach(transform.position.x, targetTilePos.x, moveSpeed);
                    float y = Util.Approach(transform.position.y, targetTilePos.y, moveSpeed);
                    transform.position = new Vector2(x, y);
                }
            }

            if (Vector3.Distance(transform.position, gridScript.snapToGrid(targetTile)) < 0.001f)
            {
                if(moving)dayCycleScript.addTilesMoved(1);
                transform.position = gridScript.snapToGrid(targetTile); // snap cleanly
                tilePos = Vector3Int.FloorToInt(gridManager.GetComponent<Grid>().WorldToCell(targetTile));
                moving = false;
            }
        }
    }   
}
