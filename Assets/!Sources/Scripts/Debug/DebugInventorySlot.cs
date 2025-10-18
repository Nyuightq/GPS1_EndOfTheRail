// --------------------------------------------------------------
// Creation Date: #CREATIONDATE#
// Author: #DEVELOPER#
// Description: -
// --------------------------------------------------------------
using TMPro;
using UnityEngine;

public class DebugInventorySlot : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI label;

    public void setInfo(int x, int y)
    {
        label.text = $"{x},{y}";
    }
}
