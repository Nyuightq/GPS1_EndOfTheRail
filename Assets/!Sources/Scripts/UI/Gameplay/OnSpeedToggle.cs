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

    private bool _isToggled = false;
    private OnPausePlay onPausePlay;

    // Global speed multiplier accessible to all other scripts
    public static float SpeedMultiplier = 1f;

    private void Start()
    {
        Instance = this;
        onPausePlay = FindFirstObjectByType<OnPausePlay>();

        onPausePlay.onTogglePauseEvent += OnChangeSpeedMultiplier;
    }

    private void OnChangeSpeedMultiplier(bool pausing = false)
    {
        SpeedMultiplier = _isToggled ? 2f : 1f;
        if( pausing == true ) SpeedMultiplier = 0f;
    }

    public void OnSpeedButton()
    {
        Instance._isToggled = !Instance._isToggled;
        
        OnChangeSpeedMultiplier(onPausePlay.IsPaused);
        Instance.speedSpriteImage.sprite = Instance._isToggled ? speedTwoSprite : speedOneSprite;
    }
}
