using UnityEngine;
using UnityEngine.UI;

public class OnDayProgressSlider : MonoBehaviour
{
    [SerializeField] private DayCycleScript dayCycle; // assign in Inspector
    [SerializeField] private Slider progressSlider;

    private void Start()
    {
        if (dayCycle == null)
            dayCycle = FindObjectOfType<DayCycleScript>();

        if (progressSlider == null)
            progressSlider = GetComponent<Slider>();

        progressSlider.minValue = 0f;
    }

    private void Update()
    {
        if (dayCycle == null || progressSlider == null)
            return;

        float maxValue = 1f;
        float currentValue = 0f;

        // Choose correct max value based on time of day
        if (dayCycle.CurrentTime == DayCycleScript.TimeState.Day)
        {
            maxValue = dayCycle.DayLength + dayCycle.DayLengthMod;
            currentValue = dayCycle.TilesMoved;
        }
        else if (dayCycle.CurrentTime == DayCycleScript.TimeState.Night)
        {
            maxValue = dayCycle.NightLength;
            currentValue = dayCycle.TilesMoved;
        }

        // Update the slider fill
        progressSlider.maxValue = maxValue;
        progressSlider.value = currentValue;
    }
}
