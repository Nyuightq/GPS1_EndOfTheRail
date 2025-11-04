using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OnDayProgressCounter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DayCycleScript dayCycle;      // Reference to your DayCycleScript
    [SerializeField] private TextMeshProUGUI dayCounterText;

    private void Start()
    {
        if (dayCycle == null)
            dayCycle = FindObjectOfType<DayCycleScript>();

        UpdateDayCounterText();
    }

    private void Update()
    {
        if (dayCycle == null || dayCounterText == null)
            return;

        UpdateDayCounterText();
    }

    private void UpdateDayCounterText()
    {
        // Display the actual day count from DayCycleScript
        dayCounterText.text = $"Day {dayCycle.GetDay()}";
    }
}
