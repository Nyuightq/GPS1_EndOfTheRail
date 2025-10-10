// --------------------------------------------------------------
// Creation Date: 2025-10-04 12:24
// Author: User
// Description: -
// --------------------------------------------------------------
using Unity.VisualScripting;
using UnityEngine;

public class TrainMovement : MonoBehaviour
{
    public GameObject gridManager;
    [SerializeField] float moveSpeed;
    RailGridScript gridScript;
    bool moving = false;
    //private bool reversed = false;
    Vector2 direction;
    Vector3Int tilePos;
    Vector3Int targetTile;
    private RailData prevRail = null;
    void Start()
    {
        gridScript = gridManager.GetComponent<RailGridScript>();
        tilePos = Vector3Int.FloorToInt(transform.position);

        transform.position = gridScript.snapToGrid(transform.position);
    }

    private Vector3Int Vec2ToInt(Vector2 v)
    {
        return new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), 0);
    }

    private bool AreOpposite(Vector2 a, Vector2 b, float tol = 0.01f)
    {
        // returns true if a ≈ -b
        return Mathf.Abs(a.x + b.x) < tol && Mathf.Abs(a.y + b.y) < tol;
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

                //Debug.Log(reversed);

                if (prevRail != null)
                {
                    //if (reversed)
                    //{
                    //    prevRailDir = prevRail.directionOut;
                    //    currentRailDir = currentRail.directionOut;
                    //}
                    //else
                    //{
                    //    prevRailDir = prevRail.directionOut;
                    //    currentRailDir = currentRail.directionIn;
                    //}

                    //if (reversed && currentRail.directionOut != prevRail.directionOut) reversed = false;

                    //if (AreOpposite(prevRailDir, currentRailDir))
                    //{
                    //    // We confirmed prevRail points into this tile, so continue forwards (use outgoing)
                    //    chosenDir = currentRail.directionOut;
                    //    reversed = false;
                    //}
                    //else
                    //{
                    //    // either prevRail missing or doesn't point in -> fallback to incoming (or choose outgoing if that's your intended behavior)
                    //    chosenDir = currentRail.directionIn;
                    //    reversed = true;
                    //}
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
        else
        {
            if (gridScript.railAtPos(targetTile))
            {
                transform.position = Vector3.Lerp(transform.position, gridScript.snapToGrid(targetTile), 0.1f);
            }
            if (Vector3.Distance(transform.position, gridScript.snapToGrid(targetTile)) < 0.001f)
            {
                transform.position = gridScript.snapToGrid(targetTile); // snap cleanly
                tilePos = Vector3Int.FloorToInt(gridManager.GetComponent<Grid>().WorldToCell(targetTile));
                moving = false;
            }
        }
    }   
}
