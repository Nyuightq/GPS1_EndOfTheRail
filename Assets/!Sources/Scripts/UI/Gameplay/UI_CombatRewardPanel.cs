// --------------------------------------------------------------
// Creation Date: 2025-11-03 22:02
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class UI_CombatRewardPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private Button nextButton;

    private Action onRewardComplete;

    private void OnEnable()
    {
        // Pending
        //SoundManager.Instance.PlaySFX("");
        //Debug.Log($""); 
    }

    private void OnDisable()
    {

    }
    
    public void Setup(int amount, Action onComplete)
    {
        onRewardComplete = onComplete;
        rewardText.text = "+ " + amount.ToString() + " scraps!";

        gameObject.SetActive(true);

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnNextButtonClicked);
    }

    private void OnNextButtonClicked()
    {
        gameObject.SetActive(false);
        onRewardComplete?.Invoke();
    }
}