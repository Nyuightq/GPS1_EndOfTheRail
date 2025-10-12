using UnityEngine;

[CreateAssetMenu(menuName = "Custom Tiles/Combat Tile")]
public class CombatTile : EventTile
{
    public override void OnPlayerEnter(GameObject player)
    {
        Debug.Log("Player stepped on a Combat Tile!");

        // Freeze train using TrainFreezeController
        TrainFreezeController freezeController = FindObjectOfType<TrainFreezeController>();
        if (freezeController != null)
            freezeController.FreezeTrain();

        // Start combat
        CombatManager.Instance.StartCombat();
    }

    public override void OnPlayerExit(GameObject player)
    {
        // Optional: handled automatically by CombatManager when it closes
    }
}
