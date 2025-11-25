using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OnPausePlay : MonoBehaviour
{
    [SerializeField] private Image pausePlayImage;
    [SerializeField] private Sprite pauseSprite;
    [SerializeField] private Sprite playSprite;

    [SerializeField] private GameObject pausePlayPanel;

    [SerializeField] private MonoBehaviour[] scriptsToPause;
    private TrainMovement trainMovement;

    private bool _isToggled = false;
    private bool _isPaused = false;
    private bool _isOnSettings = false;
    public bool IsPaused => _isPaused;
    public bool IsOnSettings => _isOnSettings;
    public delegate void OnTogglePauseEvent(bool isPausing);
    public event OnTogglePauseEvent onTogglePauseEvent;

    public void Start()
    {
        pausePlayPanel.SetActive(false);
        
        // FindTrainMovementToPause();
    }

    private void FindTrainMovementToPause()
    {
        if (trainMovement == null)
        {
            trainMovement = FindAnyObjectByType<TrainMovement>();
        }

        if (trainMovement != null)
        {
            PauseTrain(_isPaused);
        }
    }

    private void PauseTrain(bool pausing = true)
    {
        if (trainMovement == null) return;
        trainMovement.enabled = !pausing;
    }

    public void OnSettingButton(bool pausing = true)
    {
        GameStateManager.SetPause(pausing);
        if (_isToggled == false)
        {
            _isPaused = pausing;
            onTogglePauseEvent?.Invoke(_isPaused);
        }
        
        PauseProcess();
    }

    public void OnPausePlayButton()
    {
        if (pausePlayImage == null)
        {
            return;
        }
        
        _isToggled = !_isToggled;
        _isPaused = !_isPaused;
        onTogglePauseEvent?.Invoke(_isPaused);

        PauseProcess();

        pausePlayImage.sprite = _isToggled ? playSprite : pauseSprite;
    }

    private void PauseProcess()
    {
        bool PAUSETIME = false;

        if (_isPaused)
        {
            if (PAUSETIME) Time.timeScale = 0f; //Pause the game

            if (pausePlayPanel != null)
            {
                pausePlayPanel.SetActive(true);
            }

            foreach(var script in scriptsToPause)
            {
                if(script != null)
                {
                    script.enabled = false;
                }
            }
        }
        else
        {
            if (PAUSETIME) Time.timeScale = 1f; //Play the game

            if (pausePlayPanel != null)
            {
                pausePlayPanel.SetActive(false);
            }

            foreach(var script in scriptsToPause)
            {
                if (script != null)
                {
                    script.enabled = true;
                }
            }
        }
    }

    public void ResetPauseState()
    {
        _isPaused = false;
        _isToggled = false;

        pausePlayImage.sprite = pauseSprite; // Make sure it shows the pause icon again
        if (pausePlayPanel != null)
            pausePlayPanel.SetActive(false);

        foreach (var script in scriptsToPause)
        {
            if (script != null)
                script.enabled = true;
        }
    }

}
