using UnityEngine;

[CreateAssetMenu(menuName = "Custom Tiles/Transaction Tile")]
public class TransactionTile : EventTile
{
public override void OnPlayerEnter(GameObject player)
{
    Debug.Log("Player entered Transaction Tile");
    TransactionManager.Instance.OpenTransactionUI(player);

    // Freeze the train
    TrainFreezeController freezeController = player.GetComponent<TrainFreezeController>();
    if (freezeController != null)
        freezeController.FreezeTrain();
}


    public override void OnPlayerExit(GameObject player)
    {
        Debug.Log("Player exited Transaction Tile");
        TransactionManager.Instance.CloseTransactionUI();
    }
}
