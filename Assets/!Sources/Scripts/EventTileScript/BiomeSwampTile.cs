// --------------------------------------------------------------
// Creation Date: 2025-10-16 21:23
// Author: ZQlie
// Description: A biome tile that accelerates day progression (day only) and slows the train
// --------------------------------------------------------------
using UnityEngine;

[CreateAssetMenu(menuName = "Tiles/BiomeSwampTile")]
public class BiomeSwampTile : EventTile
{
    [Header("Tile Effects")]
    [SerializeField] private float speedReduction = 0.5f; // amount to reduce train speed
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

        // NOTE: Tile movement cost (2 during day, 1 during night) is now handled in TrainMovement.cs
        // No need to modify dayCycleScript here

        DayCycleScript dayCycle = train.dayCycleManager.GetComponent<DayCycleScript>();
        if (dayCycle != null)
        {
            if (dayCycle.IsDayTime)
            {
                Debug.Log("BiomeSwampTile: Entered during DAY - tile movement will be 2 (handled in TrainMovement)");
            }
            else
            {
                Debug.Log("BiomeSwampTile: Entered during NIGHT - tile movement will be 1 (normal)");
            }
        }

        // Apply speed reduction (visual slow-down)
        train.ApplySpeedModifier(speedReduction);
        SoundManager.Instance.PlaySFX("SFX_TrainMovement_Swamp");
    }

    public override void OnPlayerExit(GameObject player)
    {
        TrainMovement train = player.GetComponent<TrainMovement>();
        if (train != null)
        {
            train.ResetSpeedModifier();
            Debug.Log("BiomeSwampTile: Train speed reset to base.");
        }
    }
}