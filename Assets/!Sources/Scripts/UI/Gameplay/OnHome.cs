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
        //Check for ESC key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isHomePressed = !isHomePressed;

            if (homePanel.activeSelf)
            {
                OnResumeButton();
            }
            else
            {
                OnHomeButton();
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
        CleanupAllManagers();
        SceneManager.LoadScene("MainMenu");

        Debug.Log("Back to square 1~");
    }

    private void CleanupAllManagers()
    {
        Debug.Log("[WinLoseManager] Cleaning up all singleton managers...");

        // Destroy all singleton managers to ensure clean reload
        if (CombatManager.Instance != null)
        {
            Destroy(CombatManager.Instance.gameObject);
            Debug.Log("[WinLoseManager] Destroyed CombatManager");
        }

        if (GameStateManager.Instance != null)
        {
            Destroy(GameStateManager.Instance.gameObject);
            Debug.Log("[WinLoseManager] Destroyed GameStateManager");
        }

        if (RewardManager.Instance != null)
        {
            Destroy(RewardManager.Instance.gameObject);
            Debug.Log("[WinLoseManager] Destroyed RewardManager");
        }

        if (TransactionManager.Instance != null)
        {
            Destroy(TransactionManager.Instance.gameObject);
            Debug.Log("[WinLoseManager] Destroyed TransactionManager");
        }

        if (ChurchManager.Instance != null)
        {
            Destroy(ChurchManager.Instance.gameObject);
            Debug.Log("[WinLoseManager] Destroyed ChurchManager");
        }

        if (EngineerManager.Instance != null)
        {
            Destroy(EngineerManager.Instance.gameObject);
            Debug.Log("[WinLoseManager] Destroyed EngineerManager");
        }

        if (SoundManager.Instance != null)
        {
            Destroy(SoundManager.Instance.gameObject);
            Debug.Log("[WinLoseManager] Destroyed SoundManager");
        }

        

        // Add any other singleton managers here
        // if (InventoryItemManager.Instance != null) { Destroy(...); }

        Debug.Log("[WinLoseManager] All managers cleaned up successfully");
    }
}