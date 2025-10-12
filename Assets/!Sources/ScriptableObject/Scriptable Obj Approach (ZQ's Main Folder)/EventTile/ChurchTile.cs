using UnityEngine;

[CreateAssetMenu(menuName = "Custom Tiles/ChurchTile")]
public class ChurchTile : EventTile
{
    [Header("Healing Settings")]
    public int healAmount = 20;

    public override void OnPlayerEnter(GameObject player)
    {
        Debug.Log("Player entered Church Tile");

        // Open Church UI
        ChurchManager.Instance.OpenChurchUI(player);

        // Freeze the train while UI is open
        TrainFreezeController freezeController = player.GetComponent<TrainFreezeController>();
        if (freezeController != null)
            freezeController.FreezeTrain();
    }

    public override void OnPlayerExit(GameObject player)
    {
        Debug.Log("Player exited Church Tile");
        // Optionally close UI on exit if desired
        // ChurchManager.Instance.CloseChurchUI();
    }
}
