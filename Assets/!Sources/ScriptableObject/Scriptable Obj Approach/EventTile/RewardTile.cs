using UnityEngine;

[CreateAssetMenu(menuName = "Custom Tiles/Reward Tile")]
public class RewardTile : EventTile
{
public override void OnPlayerEnter(GameObject player)
{
    Debug.Log("Player entered Reward Tile");
    RewardManager.Instance.OpenRewardUI(player);

    // Freeze train movement
    TrainFreezeController freezeController = player.GetComponent<TrainFreezeController>();
    if (freezeController != null)
        freezeController.FreezeTrain();
}


    public override void OnPlayerExit(GameObject player)
    {
        Debug.Log("Player exited Reward Tile");
        RewardManager.Instance.CloseRewardUI();
    }
}
