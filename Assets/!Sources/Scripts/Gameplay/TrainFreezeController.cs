// --------------------------------------------------------------
// Creation Date: 2025-10-09 18:18
// Author: ZQlie
// Description: -
// --------------------------------------------------------------
using UnityEngine;

public class TrainFreezeController : MonoBehaviour
{
    private TrainMovement trainMovement;
    private bool isFrozen = false;

    private void Awake()
    {
        trainMovement = GetComponent<TrainMovement>();
        if (trainMovement == null)
            Debug.LogWarning("TrainMovement not found on this GameObject!");
    }

    private void OnEnable()
    {
        RewardManager.OnRewardClosed += ResumeTrain;
        TransactionManager.OnTransactionClosed += ResumeTrain;
        ChurchManager.OnChurchClosed += ResumeTrain;
        CombatManager.OnCombatClosed += ResumeTrain; 
        StoryManager.OnStoryClosed += ResumeTrain; 
        EngineerManager.OnEngineerClosed += ResumeTrain;
    }

    private void OnDisable()
    {
        RewardManager.OnRewardClosed -= ResumeTrain;
        TransactionManager.OnTransactionClosed -= ResumeTrain;
        ChurchManager.OnChurchClosed -= ResumeTrain;
        CombatManager.OnCombatClosed -= ResumeTrain; 
        StoryManager.OnStoryClosed -= ResumeTrain; 
        EngineerManager.OnEngineerClosed -= ResumeTrain;
    }

    public void FreezeTrain()
    {
        if (trainMovement != null)
        {
            isFrozen = true;
            trainMovement.enabled = false;
            Debug.Log("Train movement frozen.");
        }
    }

    public void ResumeTrain()
    {
        if (trainMovement != null && isFrozen)
        {
            trainMovement.enabled = true;
            isFrozen = false;
            Debug.Log("Train movement resumed.");
        }
    }
}



