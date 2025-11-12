// --------------------------------------------------------------
// Creation Date: 2025-11-03 22:02
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class UI_CombatRewardPanel : UI_BaseEventPanel
{
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private Button nextButton;

    private Action onRewardComplete;

    public void Setup(int amount, Action onComplete)
    {
        // Pending
        //SoundManager.Instance.PlaySFX("");
        //Debug.Log($""); 
        onRewardComplete = onComplete;
        rewardText.text = "+ " + amount.ToString() + " scraps!";

        ShowEventPanel();

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnNextButtonClicked);
    }

    private void OnNextButtonClicked()
    {
        HideEventPanel();
        nextButton.onClick.RemoveAllListeners();
        onRewardComplete?.Invoke();
    }
}