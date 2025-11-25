using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class OnDayToNight : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DayCycleScript dayCycle;
    [SerializeField] private Light2D dayNightLight;

    [Header("Light Settings")]
    [SerializeField] private float dayIntensity = 10f;
    [SerializeField] private float nightIntensity = 0.4f;

    [Header("Day Night Transition Duration")]
    [SerializeField] private float transitionDuration = 3f;

    private bool dayToNightTween = false;
    private bool nightToDayTween = false;
    
    private void Start()
    {
        if (dayCycle == null)
        {
            dayCycle = FindObjectOfType<DayCycleScript>();
        }

        if (dayNightLight == null)
        {
            dayNightLight = FindObjectOfType<Light2D>();
        }
    }

    private void Update()
    {
        if (dayCycle == null || dayNightLight == null)
        {
            return;
        }

        float maxTiles = dayCycle.DayLength + dayCycle.DayLengthMod;
        float tilesMoved = Mathf.Clamp(dayCycle.TilesMoved, 0, maxTiles);

        float progression = tilesMoved / maxTiles;

        //Start the tweening process at 3/4 of the day
        //which is 65% of the day is progressed
        if (!dayToNightTween && progression >= 0.65f &&
            dayCycle.CurrentTime == DayCycleScript.TimeState.Day)
        {
            //Day to night (this is visually progression, not tween)
            //dayNightLight.intensity = Mathf.Lerp(dayIntensity, nightIntensity, tilesMoved / maxTiles);

            //Day to night (UPDATED, USES DOTWEEN)
            dayToNightTween = true;
            nightToDayTween = false;

            //Kill the previous tweening so it doesn't overlap with previous actions
            DOTween.Kill(dayNightLight);

            //Dotween.To(getter, setter, endValue, duration)
            DOTween.To(
                () => dayNightLight.intensity,
                x => dayNightLight.intensity = x,
                nightIntensity, transitionDuration).
                SetEase(Ease.InOutSine).
                SetTarget(dayNightLight);
        }
        /*else
        {
            //Night to day
            dayNightLight.intensity = nightIntensity;
        }
        */

        float nightToDayDuration = 4f;

        //Night to day
        if (!nightToDayTween && progression >= 0.65f &&
            dayCycle.CurrentTime == DayCycleScript.TimeState.Night)
        {
            nightToDayTween = true;
            dayToNightTween = false;

            //Kill the previous tweening so it doesn't overlap with previous actions
            DOTween.Kill(dayNightLight);

            //Dotween.To(getter, setter, endValue, duration)
            DOTween.To(
                () => dayNightLight.intensity,
                x => dayNightLight.intensity = x,
                dayIntensity, nightToDayDuration).
                SetEase(Ease.InOutSine).
                SetTarget(dayNightLight);
        }
    }

    /// <summary>
    /// Forces the lighting back to day intensity. Used when skipping night phase.
    /// </summary>
    public void ForceResetToDay()
    {
        if (dayNightLight != null)
        {
            //Kill any running tweens of this light
            DOTween.Kill(dayNightLight);
            
            dayNightLight.intensity = dayIntensity;
            Debug.Log($"[OnDayToNight] Light intensity forced to day value: {dayIntensity}");
        }
    }
}