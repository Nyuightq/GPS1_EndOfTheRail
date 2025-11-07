using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal; 

public class OnDayProgressSlider : MonoBehaviour
{
    [SerializeField] private DayCycleScript dayCycle; // assign in Inspector
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Light2D globalLight;

    [Header("Light Settings")]
    [SerializeField] private float dayIntensity = 10.0f;
    [SerializeField] private float nightIntensity = 0.15f;

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

        if (globalLight == null)
        {
            globalLight = FindObjectOfType<Light2D>();
        }

        progressSlider.minValue = 0f;
    }

    private void Update()
    {
        if (dayCycle == null || progressSlider == null || globalLight == null)
        {
            return;
        }

        float maxValue;
        float currentValue;
        float t;

        // Choose correct max value based on time of day
        if (dayCycle.CurrentTime == DayCycleScript.TimeState.Night)
        {
            maxValue = dayCycle.DayLength + dayCycle.DayLengthMod;
            currentValue = Mathf.Clamp(dayCycle.TilesMoved, 0, maxValue);
            t = currentValue / maxValue;

            //Fade from night to day
            globalLight.intensity = Mathf.Lerp(nightIntensity, dayIntensity, t);
        }
        else if (dayCycle.CurrentTime == DayCycleScript.TimeState.Day)
        {
            maxValue = dayCycle.NightLength;
            currentValue = Mathf.Clamp(dayCycle.TilesMoved, 0, maxValue);
            t = currentValue / maxValue;

            //Fade from day to night
            globalLight.intensity = Mathf.Lerp(dayIntensity, nightIntensity, t);
        }
        else
        {
            t = 0f;
        }

        // Update the slider fill
        progressSlider.maxValue = 15f;
        progressSlider.value = t;
    }
}
