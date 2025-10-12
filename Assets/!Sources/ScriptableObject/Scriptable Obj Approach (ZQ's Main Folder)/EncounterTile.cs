// --------------------------------------------------------------
// Creation Date: 2025-10-13 00:03
// Author: ZQlie
// Description: -
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Custom Tiles/Encounter Tile")]
public class EncounterTile : EventTile
{
    [Header("Visuals")]
    public Tile tileVisual; // Drag a Tile asset for this Encounter tile

    [Header("Spawn Settings")]
    [Range(0f, 1f)] public float spawnChance = 0.25f; // chance per tile each night

    public override void OnPlayerEnter(GameObject player)
    {
        Debug.Log("Player stepped on an Encounter Tile!");

        // Freeze train (optional)
        TrainFreezeController freezeController = FindObjectOfType<TrainFreezeController>();
        if (freezeController != null)
            freezeController.FreezeTrain();

        // Start combat
        if (CombatManager.Instance != null)
            CombatManager.Instance.StartCombat();
        else
            Debug.LogWarning("No CombatManager found in scene!");
    }

    public override void OnPlayerExit(GameObject player)
    {
        // Handled automatically by CombatManager
    }
}
