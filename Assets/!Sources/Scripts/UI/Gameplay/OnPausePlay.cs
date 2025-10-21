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

    private bool isToggled = false;
    private bool isPaused = false;

    public void Start()
    {
        pausePlayPanel.SetActive(false);
    }

    public void OnPausePlayButton()
    {
        if (pausePlayImage == null)
        {
            return;
        }
        
        isToggled = !isToggled;
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f; //Pause the game

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
            Time.timeScale = 1f; //Play the game

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

        pausePlayImage.sprite = isToggled ? playSprite : pauseSprite;
    }
}
