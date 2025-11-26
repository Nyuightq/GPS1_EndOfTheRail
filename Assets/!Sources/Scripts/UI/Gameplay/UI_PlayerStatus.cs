// --------------------------------------------------------------
// Creation Date: 2025-10-22 11:25
// Author: nyuig
// Description: Attach to GameGeneral_UI > SidePanel > StatsGroup #
//              Used to update player attributes UI layer only (Scraps, Train, Crystals)
// --------------------------------------------------------------
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class UI_PlayerStatus : MonoBehaviour
{
    [Header("Scraps Component")]
    [SerializeField] private TMP_Text scrapsText;

    [Header("Health Component")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text hpText;

    [Header("Crystal Component")]
    [SerializeField] private Slider crystalHpSlider;
    [SerializeField] private TMP_Text crystalHpText;

    [Header("Script Component")]
    [SerializeField] private PlayerStatusManager playerStatus;

    private int _t_currentHp = 0;
    private int _t_currentScraps = 0;
    private int _t_currentCrystalHp = 0;
    private RectTransform _rectScraps;

    private Coroutine hpRoutine;
    private Coroutine scrapsRoutine;
    private Coroutine crystalRoutine;

    private void Start()
    {
        _rectScraps = scrapsText.GetComponentInParent<RectTransform>();
    }

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
            return;

        playerStatus.OnHpChanged -= UpdateHpUI;
        playerStatus.OnScrapsChanged -= UpdateScrapsUI;
        playerStatus.OnCrystalHpChanged -= UpdateCrystalHpUI;
    }

    // ---------------------------
    // SCRAPS
    // ---------------------------
    private void UpdateScrapsUI(int newScraps)
    {
        if (scrapsRoutine != null) StopCoroutine(scrapsRoutine);
        scrapsRoutine = StartCoroutine(AnimateScraps(newScraps));
    }

    private IEnumerator AnimateScraps(int target)
    {
        int start = _t_currentScraps;
        float duration = 0.3f;
        float t = 0;

        // Define the quick pulse animation parameters
        float pulseScale = 1.15f; // Scale up to 115%
        float pulseDuration = 0.04f; // Very fast pulse

        while (t < duration)
        {
            t += Time.deltaTime;
            int transitionValue = Mathf.RoundToInt(Mathf.Lerp(start, target, t / duration));
            if (transitionValue != _t_currentScraps)
            {
                _t_currentScraps = transitionValue;
                scrapsText.text = _t_currentScraps.ToString();

                // --- Repeated DoTween Pulse ---
                // Stop any previous scale animation that might be running.
                // DO NOT DOKill the entire RectTransform, as that would stop the number transition's original jump if it were running.
                _rectScraps.DOKill(); // Kills only tweens created on this object with the 'true' parameter (optional, but cleaner)
                _rectScraps.localScale = Vector3.one;
                
                // 1. Scale Up quickly
                _rectScraps.DOScale(pulseScale, pulseDuration)
                    .SetEase(Ease.OutCirc)
                    .OnComplete(() =>
                {
                    // 4. Simultaneously Scale Back Down and Move Back Down
                    _rectScraps.DOScale(1f, pulseDuration)
                        .SetEase(Ease.InCirc);
                });
                // ------------------------------
            }
            yield return null;
        }
        _t_currentScraps = target;
        scrapsText.text = target.ToString();
        _rectScraps.DOScale(1f, pulseDuration);
    }

    // ---------------------------
    // HP
    // ---------------------------
    private void UpdateHpUI(int newHp, int maxHp)
    {
        hpSlider.maxValue = maxHp;
        AnimateHp(newHp, maxHp);
    }
    private void AnimateHp(int target, int maxHp)
    {
        float duration = 0.3f;
        hpSlider.DOKill();

        hpSlider.DOValue(target, duration)
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                _t_currentCrystalHp = Mathf.RoundToInt(hpSlider.value);
                hpText.text = _t_currentCrystalHp + "/" + maxHp;
            })
            .OnComplete(() =>
            {
                _t_currentCrystalHp = target;
                hpText.text = target + "/" + maxHp;
            });
    }

    // ---------------------------
    // CRYSTAL HP
    // ---------------------------
    private void UpdateCrystalHpUI(int newHp, int maxHp)
    {
        crystalHpSlider.maxValue = maxHp;
        AnimateCrystalHp(newHp, maxHp);
    }

    private void AnimateCrystalHp(int target, int maxHp)
    {
        float duration = 0.3f;
        crystalHpSlider.DOKill();

        crystalHpSlider.DOValue(target, duration)
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                _t_currentCrystalHp = Mathf.RoundToInt(crystalHpSlider.value);
                crystalHpText.text = _t_currentCrystalHp + "/" + maxHp;
            })
            .OnComplete(() =>
            {
                _t_currentCrystalHp = target;
                crystalHpText.text = target + "/" + maxHp;
            });
    }
}
