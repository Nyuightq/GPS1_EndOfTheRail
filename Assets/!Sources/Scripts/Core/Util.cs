// --------------------------------------------------------------
// Creation Date: 2025-10-11 04:00
// Author: User
// Description: -
// --------------------------------------------------------------
using UnityEngine;

public static class Util
{
    public static float Approach(float pointA, float pointB, float val)
    {
        if(pointA < pointB)
        {
            pointA += val;
            if (pointA > pointB) return pointB;
        }
        else
        {
            pointA -= val;
            if (pointA < pointB) return pointB;
        }
        return pointA;
    }
}
