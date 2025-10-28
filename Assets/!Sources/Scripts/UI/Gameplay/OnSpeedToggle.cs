using UnityEngine;
using TMPro;

public class OnSpeedToggle : MonoBehaviour
{
    public static OnSpeedToggle Instance;
    [SerializeField] private TMP_Text speedButtonText;
    [SerializeField] private string firstText = "N";
    [SerializeField] private string secondText = "X2";

    private bool isToggled = false;

    // Global speed multiplier accessible to all other scripts
    public static float SpeedMultiplier = 1f;

    private void Start()
    {
        Instance = this;
    }
    public void OnSpeedButton()
    {
        Instance.isToggled = !Instance.isToggled;
        //speedButtonText.text = isToggled ? secondText : firstText;
        SpeedMultiplier = isToggled ? 4f : 1f; // <� core logic
        Instance.speedButtonText.text = Instance.isToggled ? secondText : firstText;
    }
}
