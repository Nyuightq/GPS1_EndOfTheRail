using UnityEngine;
using TMPro;

public class OnSpeedToggle : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speedButtonText;
    [SerializeField] private string firstText = "N";
    [SerializeField] private string secondText = "X2";

    private bool isToggled = false;

    // Global speed multiplier accessible to all other scripts
    public static float SpeedMultiplier = 1f;
    public void OnSpeedButton()
    {
        isToggled = !isToggled;
        //speedButtonText.text = isToggled ? secondText : firstText;
        SpeedMultiplier = isToggled ? 4f : 1f; // <– core logic
        speedButtonText.text = isToggled ? secondText : firstText;
    }
}
