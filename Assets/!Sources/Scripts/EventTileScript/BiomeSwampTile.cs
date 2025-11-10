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
            // Only apply debuff if it's currently DAYTIME
            if (dayCycle.IsDayTime)
            {
                dayCycle.addTilesMoved(movementBonus - 1);
                Debug.Log($"BiomeSwampTile triggered during DAY! Added {movementBonus} tile movement bonus.");
            }
            else
            {
                Debug.Log("BiomeSwampTile entered at NIGHT — no movement bonus applied.");
            }
            SoundManager.Instance.PlaySFX("SFX_TrainMovement_Swamp");
        }

        // Train speed still reduced (visual slow-down but faster tile progress)
        train.ApplySpeedModifier(speedReduction);
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

