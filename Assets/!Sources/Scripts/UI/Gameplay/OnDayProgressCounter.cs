using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OnDayProgressCounter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider dayProgressSlider;   // Assign your slider here
    [SerializeField] private TextMeshProUGUI dayCounterText;        // Optional: UI text to display the count

    [Header("Settings")]
    [SerializeField] private int currentDay = 0;

    private bool dayCounted = false;

    private void Start()
    {
        UpdateDayCounterText();
    }

    private void Update()
    {
        if (dayProgressSlider == null)
            return;

        // When slider reaches the end (1.0), trigger counter ONCE
        if (dayProgressSlider.value >= 1f && !dayCounted)
        {
            currentDay++;
            dayCounted = true;
            UpdateDayCounterText();
            Debug.Log($"New day reached! Current day: {currentDay}");
        }

        // Reset when slider returns to 0 (start of new day/night cycle)
        if (dayProgressSlider.value <= 0f && dayCounted)
        {
            dayCounted = false;
        }
    }

    private void UpdateDayCounterText()
    {
        if (dayCounterText != null)
        {
            dayCounterText.text = $"Day {currentDay}";
        }
    }
}
