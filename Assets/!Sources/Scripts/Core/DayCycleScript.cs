// --------------------------------------------------------------
// Creation Date: 2025-10-12 01:49
// Author: User
// Description: -
// --------------------------------------------------------------
// --------------------------------------------------------------
// Creation Date: 2025-10-12 01:49
// Author: User
// Description: Handles tile-based day/night transitions + UI overlay
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;

public class DayCycleScript : MonoBehaviour
{
    [Header("Debug fields (for viewing only)")]
    [SerializeField] private int tilesMoved = 0;
    [SerializeField] private int day = 0;
    [SerializeField] private TimeState currentTime = TimeState.Day;

    [Header("Cycle Settings")]
    [SerializeField] private int dayLength = 20;
    [SerializeField] private int nightLength = 10;
    [SerializeField] private int dayLengthMod = 0;

    [Header("UI Settings")]
    [SerializeField] private GameObject nightPanel; // ← assign your UI panel here

    private enum TimeState { Day, Night }

    public void setTilesMoved(int val) { tilesMoved = val; }
    public void addTilesMoved(int val) { tilesMoved += val; } 
    public int getTilesMoved() { return tilesMoved; }

    private void Start()
    {
        // Ensure the night panel starts disabled
        if (nightPanel != null)
            nightPanel.SetActive(false);
    }

    private void Update()
    {
        switch (currentTime)
        {
            case TimeState.Day:
                if (tilesMoved >= dayLength + dayLengthMod)
                {
                    currentTime = TimeState.Night;
                    tilesMoved = 0;

                    if (nightPanel != null)
                        nightPanel.SetActive(true);

                    Debug.Log("🌙 Night has begun!");
                }
                break;

            case TimeState.Night:
                if (tilesMoved >= nightLength)
                {
                    currentTime = TimeState.Day;
                    tilesMoved = 0;
                    day += 1;

                    if (nightPanel != null)
                        nightPanel.SetActive(false);

                    Debug.Log("☀️ Day has begun!");
                }
                break;
        }
    }
}

