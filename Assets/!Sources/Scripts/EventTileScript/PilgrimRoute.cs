// --------------------------------------------------------------
// Creation Date: 2025-10-16
// Author: Liew Zhi Qian
// Description: A special biome tile that prevents day progression and speeds up the train
// --------------------------------------------------------------
using UnityEngine;

[CreateAssetMenu(menuName = "Tiles/BiomePilgrimRouteTile")]
public class PilgrimRouteTile : EventTile
{
    [Header("Tile Effects")]
    [SerializeField] private float speedBonus = 0.5f;     // how much to *add* to train speed
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

        // NOTE: Tile movement cost (0) is now handled in TrainMovement.cs
        // This tile doesn't count toward day progression at all

        // Apply speed boost for visual effect
        train.ApplySpeedModifier(-speedBonus);
        Debug.Log($"PilgrimRouteTile: Train speed increased by {speedBonus}, tile movement = 0");
        
        SoundManager.Instance.PlaySFX("SFX_TrainMovement_Pilgrim");
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