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

    void Update()
    {
        // Check for ESC key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isHomePressed = !isHomePressed;

            if (homePanel != null)
            {
                homePanel.SetActive(isHomePressed);
            }

            // Toggle pause/play whenever Home button is pressed
            if (pausePlayScript != null)
            {
                pausePlayScript.OnSettingButton(true);
            }
        }
    }

    public void OnHomeButton()
    {
        //isHomePressed = !isHomePressed;

        homePanel.SetActive(true);

        // Toggle pause/play whenever Home button is pressed
        if (pausePlayScript != null)
        {
            pausePlayScript.OnSettingButton(true);
        }
    }

    public void OnResumeButton()
    {
        homePanel.SetActive(false);
        
        // Resume game
        if (pausePlayScript != null)
        {
            pausePlayScript.OnSettingButton(false);
        }
    }

    public void OnExitToMainMenuButton()
    {
        //Unpause the time before switching to Main Menu scene
        Time.timeScale = 1f;
        CleanupPersistentManagers();
        SceneManager.LoadScene("MainMenu");

        Debug.Log("Back to square 1~");
    }

    private void CleanupPersistentManagers()
    {
        Debug.Log("[WinLoseUIPanel] Cleaning up persistent managers before replay...");

        // Safely destroy CombatManager
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.DestroyInstance();
            Debug.Log("[WinLoseUIPanel] Destroyed old CombatManager instance.");
        }

        // Safely destroy GameStateManager
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.DestroyInstance();
            Debug.Log("[WinLoseUIPanel] Destroyed old GameStateManager instance.");
        }
        
        // Safely destroy RewardManager
        if (RewardManager.Instance != null)
        {
            RewardManager.Instance.DestroyInstance();
            Debug.Log("[WinLoseUIPanel] Destroyed old RewardManager instance.");
        }

        // Safely destroy TransactionManager
        if (TransactionManager.Instance != null)
        {
            TransactionManager.Instance.DestroyInstance();
            Debug.Log("[WinLoseUIPanel] Destroyed old TransactionManager instance.");
        }

        // Safely destroy ChurchManager
        if (ChurchManager.Instance != null)
        {
            ChurchManager.Instance.DestroyInstance();
            Debug.Log("[WinLoseUIPanel] Destroyed old ChurchManager instance.");
        }

        // Safely destroy StoryManager
        if (StoryManager.Instance != null)
        {
            StoryManager.Instance.DestroyInstance();
            Debug.Log("[WinLoseUIPanel] Destroyed old StoryManager instance.");
        }
        
        // Destroy RestPointManager? (No instance)
        // Destroy DayNightCycleManager? (No instance)
        // WinLoseManager Should not need DestroyInstance()

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.DestroyInstance();
            Debug.Log("[WinLoseUIPanel] Destroyed old SoundManager instance.");
        }

        // Destroy other managers as needed — for example:
        // if (AudioManager.Instance != null) { Destroy(AudioManager.Instance.gameObject); AudioManager.Instance = null; }
        // if (UIManager.Instance != null) { Destroy(UIManager.Instance.gameObject); UIManager.Instance = null; }
        // etc.

        Debug.Log("[WinLoseUIPanel] Cleanup complete.");
    }
}