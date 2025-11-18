using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OnSpeedToggle : MonoBehaviour
{
    public static OnSpeedToggle Instance;
    [SerializeField] private TMP_Text speedButtonText;
    [SerializeField] private Sprite speedOneSprite;
    [SerializeField] private Sprite speedTwoSprite;
    [SerializeField] private Image speedSpriteImage;

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
        SpeedMultiplier = isToggled ? 2f : 1f;
        Instance.speedSpriteImage.sprite = Instance.isToggled ? speedTwoSprite : speedOneSprite;
    }
}
