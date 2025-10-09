// --------------------------------------------------------------
// Creation Date: 2025-10-09 03:13
// Author: User
// Description: -
// --------------------------------------------------------------
using UnityEngine;

public static class GridFunc
{
    public static Vector3Int makeVec3Int(Vector3 tilePos)
    {
        return Vector3Int.FloorToInt(tilePos);
    }
}
