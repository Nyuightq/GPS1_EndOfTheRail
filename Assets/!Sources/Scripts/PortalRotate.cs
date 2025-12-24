// --------------------------------------------------------------
// Creation Date: 2025-12-24 21:45
// Author: ZQlie
// Description: -
// --------------------------------------------------------------
using UnityEngine;

public class PortalRotate : MonoBehaviour
{
    public float degreesPerSecond = 2.0f;
    void Update()
    {
        transform.Rotate(0, 0, degreesPerSecond * Time.deltaTime);
    }
}
