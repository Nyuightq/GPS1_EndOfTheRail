using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
        {
            return;
        }

        // Determine max tiles for current phase
        float maxTiles = (dayCycle.CurrentTime == DayCycleScript.TimeState.Day)
            ? dayCycle.DayLength + dayCycle.DayLengthMod
            : dayCycle.NightLength;

        //Clamp the tiles moved
        float tilesMoved = Mathf.Clamp(dayCycle.TilesMoved, 0, maxTiles);

        //Update the slider based on tiles moved
        progressSlider.maxValue = maxTiles;
        progressSlider.value = tilesMoved;

        //Increase the counter by 1 after tiles reached the day count
        if (dayCycle.CurrentTime == DayCycleScript.TimeState.Day && !dayCount)
        {
            currentDay = dayCycle.GetDay();
            dayCount = true;
            DayCountText();
        }

        //Reset counting when the day starts
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
