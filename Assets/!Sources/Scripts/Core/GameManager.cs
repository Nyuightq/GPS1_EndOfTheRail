// --------------------------------------------------------------
// Creation Date: 2025-10-02 17:46
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using UnityEngine;

public enum Phases
{
    Planning,
    Travel,
    Combat,
}
public class GameManager : MonoBehaviour
{
    // public CycleManager cycleManagerRef;
    private Phases phase;
    private int scraps;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
