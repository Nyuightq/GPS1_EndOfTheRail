// --------------------------------------------------------------
// Creation Date: 2025-10-16
// Author: Liew Zhi Qian
// Description: A special biome tile that slows day progression and speeds up the train
// --------------------------------------------------------------
using UnityEngine;

[CreateAssetMenu(menuName = "Tiles/BiomePilgrimRouteTile")]
public class PilgrimRouteTile : EventTile
{
    [Header("Tile Effects")]
    [SerializeField] private int dayReduction = 1;        // reduces day cycle progress
    [SerializeField] private float speedBonus = 0.5f;     // how much to *add* to train speed
    [SerializeField] private Sprite tileSprite;

    private void OnValidate()
    {
        if (tileSprite != null)
            this.sprite = tileSprite;
    }

public override void OnPlayerEnter(GameObject player)
{
    TrainMovement train = player.GetComponent<TrainMovement>();
    if (train == null)
    {
        Debug.LogWarning("TrainMovement not found on player!");
        return;
    }

    DayCycleScript dayCycle = train.dayCycleManager.GetComponent<DayCycleScript>();
    if (dayCycle != null)
    {
        dayCycle.addTilesMoved(-dayReduction);
        Debug.Log($"PilgrimRouteTile triggered! Day cycle slowed by {dayReduction} tile(s).");
    }

    train.ApplySpeedModifier(-speedBonus); // disables lerp internally
    Debug.Log($"PilgrimRouteTile: Train speed increased by {speedBonus}");
}


    public override void OnPlayerExit(GameObject player)
    {
        TrainMovement train = player.GetComponent<TrainMovement>();
        if (train != null)
        {
            train.ResetSpeedModifier();
            Debug.Log("PilgrimRouteTile: Train speed reset to base.");
        }
    }
}
