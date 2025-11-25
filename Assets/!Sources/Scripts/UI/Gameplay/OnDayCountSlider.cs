using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
public class DayCountSlider : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DayCycleScript dayCycle;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TextMeshProUGUI dayCounterText;

    private int currentDay = 0;
    private bool dayCount = false;

    private void Start()
    {
        if (dayCycle == null)
        {
            dayCycle = FindObjectOfType<DayCycleScript>();
        }

        if (progressSlider == null)
        {
            progressSlider = GetComponent<Slider>();
        }

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
        }

        DayCountText();
    }
    private void Update()
    {
        if (dayCycle == null || progressSlider == null)
            return;

        // Determine max tiles for current phase
        float maxTiles = (dayCycle.CurrentTime == DayCycleScript.TimeState.Day)
            ? dayCycle.DayLength + dayCycle.DayLengthMod
            : dayCycle.NightLength;

        float tilesMoved = Mathf.Clamp(dayCycle.TilesMoved, 0, maxTiles);

        progressSlider.maxValue = maxTiles;
        // --- DOTWEEN APPLY  ----------------------------------------
        // Animate slider value
        if (!Mathf.Approximately(progressSlider.value, tilesMoved))
        {
            progressSlider.DOKill();
            progressSlider.DOValue(tilesMoved, 0.25f)
                .SetEase(Ease.OutCubic);
        }
        // ------------------------------------------------------------
        if (dayCycle.CurrentTime == DayCycleScript.TimeState.Day && !dayCount)
        {
            currentDay = dayCycle.GetDay();
            dayCount = true;
            DayCountText();
        }

        if (dayCycle.CurrentTime == DayCycleScript.TimeState.Night)
        {
            dayCount = false;
        }
    }


    private void DayCountText()
    {
        if (dayCounterText != null)
        {
            dayCounterText.text = $"Day {currentDay}";
        }
    }
}
