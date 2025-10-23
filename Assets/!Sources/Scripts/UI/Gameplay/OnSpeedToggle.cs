using UnityEngine;
using TMPro;

public class OnSpeedToggle : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speedButtonText;
    [SerializeField] private string firstText = "N";
    [SerializeField] private string secondText = "X2";

    private bool isToggled = false;
    private bool isPaused = false;

    public void OnSpeedButton()
    {
        isToggled = !isToggled;
        speedButtonText.text = isToggled ? secondText : firstText;
    }
}
