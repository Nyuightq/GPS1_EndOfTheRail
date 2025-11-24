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

        _rectScraps.DOKill();

        float height = 10f;     // small upward jump
        float rotate = 30f;     // small Z rotation
        // 彈跳動畫：放大 + 稍微向上 + 輕微旋轉
        _rectScraps.DOLocalMoveY(height, 0.15f).SetEase(Ease.OutQuad);
        _rectScraps.DORotate(new Vector3(0, 0, rotate), 0.15f).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // 回到原始狀態
                _rectScraps.DOScale(1f, 0.15f).SetEase(Ease.OutQuad);
                _rectScraps.DOLocalMoveY(0f, 0.15f).SetEase(Ease.OutQuad);
                _rectScraps.DORotate(Vector3.zero, 0.15f).SetEase(Ease.OutQuad);
            });



        while (t < duration)
        {
            t += Time.deltaTime;
            _t_currentScraps = Mathf.RoundToInt(Mathf.Lerp(start, target, t / duration));
            scrapsText.text = _t_currentScraps.ToString();

            yield return null;
        }
        _t_currentScraps = target;
        scrapsText.text = target.ToString();
    }

    // ---------------------------
    // HP
    // ---------------------------
    private void UpdateHpUI(int newHp, int maxHp)
    {
        hpSlider.maxValue = maxHp;

        if (hpRoutine != null) StopCoroutine(hpRoutine);
        hpRoutine = StartCoroutine(AnimateHp(newHp, maxHp));
    }

    private IEnumerator AnimateHp(int target, int maxHp)
    {
        int start = _t_currentHp;
        float duration = 0.3f;
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            _t_currentHp = Mathf.RoundToInt(Mathf.Lerp(start, target, t / duration));

            hpSlider.value = _t_currentHp;
            hpText.text = _t_currentHp + "/" + maxHp;
            yield return null;
        }

        _t_currentHp = target;
        hpSlider.value = target;
        hpText.text = target + "/" + maxHp;
    }

    // ---------------------------
    // CRYSTAL HP
    // ---------------------------
    private void UpdateCrystalHpUI(int newHp, int maxHp)
    {
        crystalHpSlider.maxValue = maxHp;

        if (crystalRoutine != null) StopCoroutine(crystalRoutine);
        crystalRoutine = StartCoroutine(AnimateCrystalHp(newHp, maxHp));
    }

    private IEnumerator AnimateCrystalHp(int target, int maxHp)
    {
        int start = _t_currentCrystalHp;
        float duration = 0.3f;
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            _t_currentCrystalHp = Mathf.RoundToInt(Mathf.Lerp(start, target, t / duration));

            crystalHpSlider.value = _t_currentCrystalHp;
            crystalHpText.text = _t_currentCrystalHp + "/" + maxHp;
            yield return null;
        }

        _t_currentCrystalHp = target;
        crystalHpSlider.value = target;
        crystalHpText.text = target + "/" + maxHp;
    }
}
