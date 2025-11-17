// --------------------------------------------------------------
// Creation Date: 2025-10-12
// Author: ZQlie
// Description: Handles crystal HP deterioration as the train travels
// --------------------------------------------------------------
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(TrainMovement))]
public class CrystalDeteriorates : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DayCycleScript dayCycle;    // Optional: if you want to sync with DayCycle
    [Header("Deterioration Settings")]
    [SerializeField] private int baseTilesPerHPLoss = 1;    // How many tiles must be moved before losing HP

    private TrainMovement trainMovement;
    private PlayerStatusManager playerStatus;
    private int tilesMovedSinceLastHP = 0;

    public int TilesPerHPLoss
    {
        get
        {
            var q = new Query<BuffableStats>(BuffableStats.TilePerCrystalDamage, baseTilesPerHPLoss);
            PlayerStatusManager.mediator.PerformQuery(this, q);
            return Mathf.FloorToInt(q.value);
        }
    }

    private void Awake()
    {
        trainMovement = GetComponent<TrainMovement>();
        playerStatus = GameStateManager.Instance.playerStatus;
        if (trainMovement == null)
        {
            Debug.LogError("CrystalDeteriorates: No TrainMovement component found!");
        }
    }

    /// <summary>
    /// Called externally by TrainMovement whenever a tile movement is completed.
    /// </summary>
    public void OnTileMoved()
    {
        tilesMovedSinceLastHP++;

        if (tilesMovedSinceLastHP >= TilesPerHPLoss)
        {
            if (playerStatus != null)
            {
                playerStatus.CrystalTakeDamage(1);
                Debug.Log("[CrystalDeteriorates] Crystal loses 1 HP due to travel.");
            }
            else
            {
                Debug.LogWarning("[CrystalDeteriorates] Missing crystal reference!");
            }

            tilesMovedSinceLastHP = 0;
        }
    }
}
