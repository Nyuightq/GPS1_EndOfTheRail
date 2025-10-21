// --------------------------------------------------------------
// Creation Date: 2025-10-16 21:23
// Author: ZQlie
// Description: -
// --------------------------------------------------------------
using UnityEngine;

[CreateAssetMenu(menuName = "Tiles/BiomeSwampTile")]
public class BiomeSwampTile : EventTile
{
    [Header("Tile Effects")]
    [SerializeField] private int movementBonus = 2;       // adds extra movement to DayCycle
    [SerializeField] private float speedReduction = 0.5f; // fraction or amount to reduce speed
    [SerializeField] private Sprite tileSprite;

    private void OnValidate()
    {
        if (tileSprite != null)
            this.sprite = tileSprite;
    }

    // When train enters this tile
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
        dayCycle.addTilesMoved(movementBonus - 1);
        Debug.Log($"BiomeSwampTile triggered! Added {movementBonus} tile movement bonus.");
    }

    train.ApplySpeedModifier(speedReduction); // disables lerp internally
}


    public override void OnPlayerExit(GameObject player)
    {
        TrainMovement train = player.GetComponent<TrainMovement>();
        if (train != null)
        {
            train.ResetSpeedModifier();
        }
    }
}
