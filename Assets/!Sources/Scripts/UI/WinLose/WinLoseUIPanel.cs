// --------------------------------------------------------------
// Creation Date: 2025-10-31
// Description: Displays win/lose UI panel in result scene
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class WinLoseUIPanel : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private RunStatsData runStatsData; // Reference to ScriptableObject
    
    [Header("UI Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Win Panel Stats Text")]
    [SerializeField] private TextMeshProUGUI winHpText;
    [SerializeField] private TextMeshProUGUI winCrystalHpText;
    [SerializeField] private TextMeshProUGUI winScrapsText;
    [SerializeField] private TextMeshProUGUI winDaysText;
    [SerializeField] private TextMeshProUGUI winCombatsText;

    [Header("Lose Panel Stats Text")]
    [SerializeField] private TextMeshProUGUI loseReasonText;
    [SerializeField] private TextMeshProUGUI loseScrapsText;
    [SerializeField] private TextMeshProUGUI loseDaysText;
    [SerializeField] private TextMeshProUGUI loseCombatsText;

    [Header("Buttons")]
    [SerializeField] private Button replayButtonWin;
    [SerializeField] private Button replayButtonLose;
    [SerializeField] private Button mainMenuButtonWin;
    [SerializeField] private Button mainMenuButtonLose;

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameplaySceneName = "GameplayScene";

    private void Start()
    {
        Debug.Log("[WinLoseUIPanel] Start called");
        
        // Check if ScriptableObject is assigned
        if (runStatsData == null)
        {
            Debug.LogError("[WinLoseUIPanel] RunStatsData is NULL! Assign it in the Inspector!");
            return;
        }
        
        Debug.Log($"[WinLoseUIPanel] RunStatsData loaded - Win: {runStatsData.didPlayerWin}, " +
                  $"HP: {runStatsData.finalHp}/{runStatsData.finalMaxHp}, Days: {runStatsData.daysPassed}");
        
        // Setup button listeners
        if (replayButtonWin != null)
            replayButtonWin.onClick.AddListener(ReplayGame);
        if (replayButtonLose != null)
            replayButtonLose.onClick.AddListener(ReplayGame);
        
        if (mainMenuButtonWin != null)
            mainMenuButtonWin.onClick.AddListener(LoadMainMenu);
        if (mainMenuButtonLose != null)
            mainMenuButtonLose.onClick.AddListener(LoadMainMenu);

        // Display appropriate panel based on win/lose
        DisplayResults();
    }

    private void DisplayResults()
    {
        if (runStatsData == null)
        {
            Debug.LogError("RunStatsData ScriptableObject is not assigned! Cannot display results.");
            return;
        }

        bool won = runStatsData.didPlayerWin;

        if (won)
        {
            if (winPanel != null) winPanel.SetActive(true);
            if (losePanel != null) losePanel.SetActive(false);
            DisplayWinStats();
        }
        else
        {
            if (winPanel != null) winPanel.SetActive(false);
            if (losePanel != null) losePanel.SetActive(true);
            DisplayLoseStats();
        }
    }

    private void DisplayWinStats()
    {
        Debug.Log("[WinLoseUIPanel] DisplayWinStats called");
        
        if (winHpText != null)
        {
            winHpText.text = $"HP: {runStatsData.finalHp}/{runStatsData.finalMaxHp}";
            Debug.Log($"Set HP text: {winHpText.text}");
        }
        else
            Debug.LogWarning("[WinLoseUIPanel] winHpText is NULL!");
        
        if (winCrystalHpText != null)
        {
            winCrystalHpText.text = $"Crystal HP: {runStatsData.finalCrystalHp}/{runStatsData.finalMaxCrystalHp}";
            Debug.Log($"Set Crystal HP text: {winCrystalHpText.text}");
        }
        else
            Debug.LogWarning("[WinLoseUIPanel] winCrystalHpText is NULL!");
        
        if (winScrapsText != null)
        {
            winScrapsText.text = $"Scraps: {runStatsData.finalScraps}";
            Debug.Log($"Set Scraps text: {winScrapsText.text}");
        }
        else
            Debug.LogWarning("[WinLoseUIPanel] winScrapsText is NULL!");
        
        if (winDaysText != null)
        {
            winDaysText.text = $"Days Survived: {runStatsData.daysPassed}";
            Debug.Log($"Set Days text: {winDaysText.text}");
        }
        else
            Debug.LogWarning("[WinLoseUIPanel] winDaysText is NULL!");
        
        if (winCombatsText != null)
        {
            winCombatsText.text = $"Combats Faced: {runStatsData.GetTotalCombats()}";
            Debug.Log($"Set Combats text: {winCombatsText.text}");
        }
        else
            Debug.LogWarning("[WinLoseUIPanel] winCombatsText is NULL!");

        Debug.Log($"[WinLoseUIPanel] Displayed Win Stats - HP: {runStatsData.finalHp}/{runStatsData.finalMaxHp}, " +
                  $"Days: {runStatsData.daysPassed}, Combats: {runStatsData.GetTotalCombats()}");
    }

    private void DisplayLoseStats()
    {
        if (loseReasonText != null)
            loseReasonText.text = $"Lost Reason: {runStatsData.loseReason}";
        
        if (loseScrapsText != null)
            loseScrapsText.text = $"Scraps: {runStatsData.finalScraps}";
        
        if (loseDaysText != null)
            loseDaysText.text = $"Days Survived: {runStatsData.daysPassed}";
        
        if (loseCombatsText != null)
            loseCombatsText.text = $"Combats Faced: {runStatsData.GetTotalCombats()}";

        Debug.Log($"[WinLoseUIPanel] Displayed Lose Stats - Reason: {runStatsData.loseReason}, " +
                  $"Days: {runStatsData.daysPassed}, Combats: {runStatsData.GetTotalCombats()}");
    }

private void ReplayGame()
{
    Debug.Log("[WinLoseUIPanel] Restarting game...");

    // Clear ScriptableObject stats for new run
    if (runStatsData != null)
        runStatsData.ClearStats();

    // Safely clean up any persistent (DontDestroyOnLoad) managers
    CleanupPersistentManagers();

    // Reset timescale in case it’s still frozen
    Time.timeScale = 1f;

    // Reload gameplay scene cleanly
    if (!string.IsNullOrEmpty(gameplaySceneName))
    {
        Debug.Log($"[WinLoseUIPanel] Loading Gameplay Scene: {gameplaySceneName}");
        SceneManager.LoadScene(gameplaySceneName);
    }
    else
    {
        Debug.LogWarning("[WinLoseUIPanel] Gameplay scene name not set! Reloading index 0 instead.");
        SceneManager.LoadScene(0);
    }
}


    private void LoadMainMenu()
    {
        Debug.Log($"Loading Main Menu: {mainMenuSceneName}");

        // Optionally clear stats when returning to menu
        if (runStatsData != null)
            runStatsData.ClearStats();

        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("Main menu scene name not set!");
        }
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