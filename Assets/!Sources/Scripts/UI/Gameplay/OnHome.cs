using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class OnHome : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject homePanel;

    [Header("Pause Control")]
    [SerializeField] private OnPausePlay pausePlayScript;

    private bool isHomePressed = false;

    public void Start()
    {
        homePanel.SetActive(false);
    }

    public void OnHomeButton()
    {
        isHomePressed = !isHomePressed;

        homePanel.SetActive(true);

        // Toggle pause/play whenever Home button is pressed
        if (pausePlayScript != null)
        {
            pausePlayScript.OnPausePlayButton();
        }
    }

    public void OnResumeButton()
    {
        homePanel.SetActive(false);
        
        // Resume game
        if (pausePlayScript != null)
        {
            pausePlayScript.OnPausePlayButton();
        }
    }

    public void OnExitToMainMenuButton()
    {
        //Unpause the time before switching to Main Menu scene
        Time.timeScale = 1f;

        SceneManager.LoadScene("MainMenu");

        Debug.Log("Back to square 1~");
    }
}