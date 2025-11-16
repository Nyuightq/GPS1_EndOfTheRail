// --------------------------------------------------------------
// Creation Date: 2025-10-22 11:25
// Author: nyuig
// Description: Attach to GameGeneral_UI > SidePanel > StatsGroup #
//              Used to update player attributes UI layer only (Scraps, Train, Crystals)
// --------------------------------------------------------------
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerStatus : MonoBehaviour
{
    [Header("Scraps Component")]
    [SerializeField] private TMP_Text scrapsText;
    [Header("Health Component")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text hpText;
    // [SerializeField] private TMP_Text currentHpText;
    // [SerializeField] private TMP_Text maxHpText;

    [Header("Crystal Component")]
    [SerializeField] private Slider crystalHpSlider;
    [SerializeField] private TMP_Text crystalHpText;
    // [SerializeField] private TMP_Text currentCrystalHpText;
    // [SerializeField] private TMP_Text maxCrystalHpText;
    [Header("Script Component")]
    [SerializeField] private PlayerStatusManager playerStatus;

    private void OnEnable()
    {
        if (playerStatus == null)
        {
            Debug.LogWarning("UI_PlayerStatus.cs: StatsGroup's PlayerStatusManager is not referenced.");
            return;
        }
        
        playerStatus.OnHpChanged += UpdateHpUI;
        playerStatus.OnScrapsChanged += UpdateScrapsUI;
        playerStatus.OnCrystalHpChanged += UpdateCrystalHpUI;
    }

    private void OnDisable()
    {
        if (playerStatus == null)
        {
            Debug.LogWarning("UI_PlayerStatus.cs: StatsGroup's PlayerStatusManager is not referenced.");
            return;
        }
        
        playerStatus.OnHpChanged -= UpdateHpUI;
        playerStatus.OnScrapsChanged -= UpdateScrapsUI;
        playerStatus.OnCrystalHpChanged -= UpdateCrystalHpUI;
    }
    
    private void UpdateScrapsUI(int currentScraps)
    {
        scrapsText.text = currentScraps.ToString();
    }

    private void UpdateHpUI(int currentHp, int maxHp)
    {
        hpSlider.maxValue = maxHp;
        hpSlider.value = currentHp;

        hpText.text = currentHp + "/" + maxHp;
    }

    private void UpdateCrystalHpUI(int currentHp, int maxHp)
    {
        crystalHpSlider.maxValue = maxHp;
        crystalHpSlider.value = currentHp;

        crystalHpText.text = currentHp + "/" + maxHp;
    }
}