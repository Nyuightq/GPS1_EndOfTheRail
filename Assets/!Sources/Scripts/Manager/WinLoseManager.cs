// --------------------------------------------------------------
// Creation Date: 2025-10-31
// Description: Handles win/lose conditions - shows cutscenes then in-scene results
// --------------------------------------------------------------

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WinLoseManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStatusManager playerStatus;
    [SerializeField] private TrainMovement trainMovement;
    [SerializeField] private RailGridScript gridManager;
    [SerializeField] private DayCycleScript dayCycle;
    [SerializeField] private CombatManager combatManager;
    
    [Header("Result Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    
    [Header("Cutscene System")]
    [SerializeField] private GameObject cutscenePanel; // Panel containing the button
    [SerializeField] private Button cutsceneButton; // The clickable button
    [SerializeField] private Image cutsceneImage; // Image component on the button
    [SerializeField] private Sprite[] winCutscenes; // Array of win cutscene sprites
    [SerializeField] private Sprite[] loseCutscenes; // Array of lose cutscene sprites
    
    [Header("Inventory Canvas")]
    [SerializeField] private Canvas inventoryCanvas;
    [SerializeField] private RectTransform winPanelInventoryAnchor;
    [SerializeField] private RectTransform losePanelInventoryAnchor;
    [SerializeField] private bool showInventoryInResults = true;
    
    [Header("Action Buttons")]
    [SerializeField] private Button replayButtonWin;
    [SerializeField] private Button replayButtonLose;
    [SerializeField] private Button mainMenuButtonWin;
    [SerializeField] private Button mainMenuButtonLose;
    
    [Header("Win Panel Stats Text")]
    [SerializeField] private TMPro.TextMeshProUGUI winHpText;
    [SerializeField] private TMPro.TextMeshProUGUI winCrystalHpText;
    [SerializeField] private TMPro.TextMeshProUGUI winScrapsText;
    [SerializeField] private TMPro.TextMeshProUGUI winDaysText;
    [SerializeField] private TMPro.TextMeshProUGUI winCombatsText;
    [SerializeField] private TMPro.TextMeshProUGUI winTotalScrapsText;
    [SerializeField] private TMPro.TextMeshProUGUI winTotalTilesText;

    [Header("Lose Panel Stats Text")]
    [SerializeField] private TMPro.TextMeshProUGUI loseReasonText;
    [SerializeField] private TMPro.TextMeshProUGUI loseScrapsText;
    [SerializeField] private TMPro.TextMeshProUGUI loseDaysText;
    [SerializeField] private TMPro.TextMeshProUGUI loseCombatsText;
    [SerializeField] private TMPro.TextMeshProUGUI loseTotalScrapsText;
    [SerializeField] private TMPro.TextMeshProUGUI loseTotalTilesText; 

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Game State")]
    public bool isGameOver = false;

    private bool _initialized = false;
    private RectTransform inventoryRectTransform;
    private Transform inventoryOriginalParent;
    private Vector3 inventoryOriginalPosition;
    private Vector3 inventoryOriginalScale;
    private int inventoryOriginalSortingOrder;
    
    // Cutscene state
    private int currentCutsceneIndex = 0;
    private Sprite[] currentCutsceneArray;
    private bool isPlayingWinCutscene = false;
    private string loseReason = "";

    private IEnumerator Start()
    {
        yield return null;
        _initialized = true;

        // Hide all panels initially
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (cutscenePanel != null) cutscenePanel.SetActive(false);

        // Store inventory canvas original state
        if (inventoryCanvas != null)
        {
            inventoryRectTransform = inventoryCanvas.GetComponent<RectTransform>();
            if (inventoryRectTransform != null)
            {
                inventoryOriginalParent = inventoryRectTransform.parent;
                inventoryOriginalPosition = inventoryRectTransform.localPosition;
                inventoryOriginalScale = inventoryRectTransform.localScale;
                inventoryOriginalSortingOrder = inventoryCanvas.sortingOrder;
                Debug.Log("[WinLoseManager] Stored inventory canvas original state");
            }
        }

        // Setup action buttons
        if (replayButtonWin != null)
            replayButtonWin.onClick.AddListener(ReloadScene);
        if (replayButtonLose != null)
            replayButtonLose.onClick.AddListener(ReloadScene);
        if (mainMenuButtonWin != null)
            mainMenuButtonWin.onClick.AddListener(LoadMainMenu);
        if (mainMenuButtonLose != null)
            mainMenuButtonLose.onClick.AddListener(LoadMainMenu);
        
        // Setup cutscene button
        if (cutsceneButton != null)
            cutsceneButton.onClick.AddListener(OnCutsceneButtonClicked);

        TryAssignReferences();
    }

    private void Update()
    {
        if (!_initialized || isGameOver) return;

        TryAssignReferences();

        if (trainMovement == null || playerStatus == null || gridManager == null)
            return;

        CheckHealthCondition();
        CheckWinCondition();
    }

    private void TryAssignReferences()
    {
        if (trainMovement == null)
        {
            trainMovement = FindFirstObjectByType<TrainMovement>();
            if (trainMovement != null)
                Debug.Log($"[WinLoseManager] Found TrainMovement: {trainMovement.name}");
        }
        
        if (playerStatus == null)
        {
            playerStatus = FindFirstObjectByType<PlayerStatusManager>();
            if (playerStatus != null)
                Debug.Log($"[WinLoseManager] Found PlayerStatusManager: {playerStatus.name}");
        }
        
        if (gridManager == null)
        {
            gridManager = FindFirstObjectByType<RailGridScript>();
            if (gridManager != null)
                Debug.Log($"[WinLoseManager] Found RailGridScript: {gridManager.name}");
        }
        
        if (dayCycle == null)
        {
            dayCycle = FindFirstObjectByType<DayCycleScript>();
            if (dayCycle != null)
                Debug.Log($"[WinLoseManager] Found DayCycleScript: {dayCycle.name}");
        }
        
        if (combatManager == null)
        {
            combatManager = CombatManager.Instance;
            if (combatManager != null)
                Debug.Log($"[WinLoseManager] Found CombatManager.Instance");
        }
    }

    private void CheckHealthCondition()
    {
        if (playerStatus == null) return;

        if (playerStatus.Hp <= 0)
        {
            TriggerLose("Train Destroyed");
        }
        else if (playerStatus.CrystalHp <= 0)
        {
            TriggerLose("Crystal crumbled");
        }
    }

    private void CheckWinCondition()
    {
        if (trainMovement == null || gridManager == null) return;

        Grid grid = gridManager.GetComponent<Grid>();
        if (grid == null) return;

        Vector3Int currentTile = grid.WorldToCell(trainMovement.transform.position);
        RailData currentRail = gridManager.GetRailAtPos(currentTile);

        if (currentRail != null && currentRail.railType == RailData.railTypes.end)
        {
            if (playerStatus != null && 
                playerStatus.Hp > 0 && 
                playerStatus.CrystalHp > 0)
            {
                TriggerWin();
            }
        }
    }

    public void TriggerWin()
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 0f;

        Debug.Log("Player Won! Starting win cutscene...");

        isPlayingWinCutscene = true;
        StartCutsceneSequence(winCutscenes);
    }

    private void TriggerLose(string reason = "Unknown")
    {
        if (isGameOver) return;

        isGameOver = true;
        loseReason = reason;

        if (trainMovement != null)
            trainMovement.enabled = false;

        Debug.Log($"Player Lost! Reason: {reason}. Starting lose cutscene...");
        Time.timeScale = 0f;

        isPlayingWinCutscene = false;
        StartCutsceneSequence(loseCutscenes);
    }

    private void StartCutsceneSequence(Sprite[] cutscenes)
    {
        currentCutsceneArray = cutscenes;
        currentCutsceneIndex = 0;

        // Check if cutscenes exist
        if (currentCutsceneArray == null || currentCutsceneArray.Length == 0)
        {
            Debug.LogWarning("[WinLoseManager] No cutscenes found, skipping to results panel");
            ShowResultsPanel();
            return;
        }

        // Show first cutscene
        ShowCutscene(currentCutsceneIndex);
    }

    private void ShowCutscene(int index)
    {
        if (cutscenePanel == null || cutsceneImage == null)
        {
            Debug.LogError("[WinLoseManager] Cutscene panel or image not assigned!");
            ShowResultsPanel();
            return;
        }

        // Display the cutscene
        cutsceneImage.sprite = currentCutsceneArray[index];
        cutscenePanel.SetActive(true);

        Debug.Log($"[WinLoseManager] Showing cutscene {index + 1}/{currentCutsceneArray.Length}");
    }

    private void OnCutsceneButtonClicked()
    {
        currentCutsceneIndex++;

        // Check if there are more cutscenes
        if (currentCutsceneIndex < currentCutsceneArray.Length)
        {
            ShowCutscene(currentCutsceneIndex);
        }
        else
        {
            // All cutscenes finished, show results panel
            cutscenePanel.SetActive(false);
            ShowResultsPanel();
        }
    }

    private void ShowResultsPanel()
    {
        if (isPlayingWinCutscene)
        {
            // Show win panel
            DisplayWinStats();
            if (winPanel != null) winPanel.SetActive(true);
            if (losePanel != null) losePanel.SetActive(false);

            if (showInventoryInResults)
                AttachInventoryToPanel(winPanelInventoryAnchor);
            
            Debug.Log("[WinLoseManager] Showing win panel after cutscene");
        }
        else
        {
            // Show lose panel
            DisplayLoseStats(loseReason);
            if (winPanel != null) winPanel.SetActive(false);
            if (losePanel != null) losePanel.SetActive(true);

            if (showInventoryInResults)
                AttachInventoryToPanel(losePanelInventoryAnchor);
            
            Debug.Log("[WinLoseManager] Showing lose panel after cutscene");
        }
    }

    private void DisplayWinStats()
    {
        if (playerStatus == null) return;

        int days = dayCycle != null ? dayCycle.GetDay() : 0;
        int totalCombats = combatManager != null ? 
            (combatManager.totalCombatsFaced + combatManager.totalEncountersFaced) : 0;
        int totalTilesMoved = dayCycle != null ? dayCycle.GetTotalTilesMovedAllTime() : 0;
        int totalScrapsAcquired = playerStatus.TotalScrapsAcquired;

        if (winHpText != null)
            winHpText.text = $"HP: {playerStatus.Hp}/{playerStatus.MaxHp}";
        
        if (winCrystalHpText != null)
            winCrystalHpText.text = $"Crystal HP: {playerStatus.CrystalHp}/{playerStatus.MaxCrystalHp}";
        
        if (winScrapsText != null)
            winScrapsText.text = $"Leftover Scraps: {playerStatus.Scraps}";
        
        if (winTotalScrapsText != null)
            winTotalScrapsText.text = $"Total Scraps Acquired: {totalScrapsAcquired}";
        
        if (winTotalTilesText != null)
            winTotalTilesText.text = $"Total Tiles Moved: {totalTilesMoved}";
        
        if (winDaysText != null)
            winDaysText.text = $"Days Survived: {days}";
        
        if (winCombatsText != null)
            winCombatsText.text = $"Combats Faced: {totalCombats}";

        Debug.Log($"[WinLoseManager] Win stats - Days: {days}, Combats: {totalCombats}, Tiles: {totalTilesMoved}, Scraps: {totalScrapsAcquired}");
    }

    private void DisplayLoseStats(string reason)
    {
        if (playerStatus == null) return;

        int days = dayCycle != null ? dayCycle.GetDay() : 0;
        int totalCombats = combatManager != null ? 
            (combatManager.totalCombatsFaced + combatManager.totalEncountersFaced) : 0;
        int totalTilesMoved = dayCycle != null ? dayCycle.GetTotalTilesMovedAllTime() : 0;
        int totalScrapsAcquired = playerStatus.TotalScrapsAcquired;

        if (loseReasonText != null)
            loseReasonText.text = $"{reason}";
        
        if (loseScrapsText != null)
            loseScrapsText.text = $"Leftover Scraps: {playerStatus.Scraps}";
        
        if (loseTotalScrapsText != null)
            loseTotalScrapsText.text = $"Total Scraps Acquired: {totalScrapsAcquired}";
        
        if (loseTotalTilesText != null)
            loseTotalTilesText.text = $"Total Tiles Moved: {totalTilesMoved}";
        
        if (loseDaysText != null)
            loseDaysText.text = $"Days Survived: {days}";
        
        if (loseCombatsText != null)
            loseCombatsText.text = $"Combats Faced: {totalCombats}";

        Debug.Log($"[WinLoseManager] Lose stats - Reason: {reason}, Days: {days}, Tiles: {totalTilesMoved}, Scraps: {totalScrapsAcquired}");
    }

    private void AttachInventoryToPanel(RectTransform anchorPoint)
    {
        if (inventoryCanvas == null || inventoryRectTransform == null || anchorPoint == null)
        {
            Debug.LogWarning("[WinLoseManager] Cannot attach inventory - missing references");
            return;
        }

        inventoryRectTransform.SetParent(anchorPoint, false);
        inventoryRectTransform.localPosition = Vector3.zero;
        inventoryRectTransform.localScale = new Vector3(0.8f, 0.8f, 1f);
        
        inventoryCanvas.gameObject.SetActive(true);
        inventoryCanvas.sortingOrder = 100;
        
        Debug.Log($"[WinLoseManager] Inventory canvas attached to {anchorPoint.name} with 0.8 scale");
    }

    private void RestoreInventoryCanvas()
    {
        if (inventoryCanvas == null || inventoryRectTransform == null || inventoryOriginalParent == null)
        {
            Debug.LogWarning("[WinLoseManager] Cannot restore inventory - missing original state");
            return;
        }

        inventoryRectTransform.SetParent(inventoryOriginalParent, false);
        inventoryRectTransform.localPosition = inventoryOriginalPosition;
        inventoryRectTransform.localScale = inventoryOriginalScale;
        inventoryCanvas.sortingOrder = inventoryOriginalSortingOrder;
        
        Debug.Log("[WinLoseManager] Inventory canvas restored to original state");
    }

    private void ReloadScene()
    {
        Debug.Log("[WinLoseManager] Reloading scene for new game...");

        RestoreInventoryCanvas();
        CleanupAllManagers();
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void LoadMainMenu()
    {
        Debug.Log("[WinLoseManager] Loading main menu...");
        
        RestoreInventoryCanvas();
        CleanupAllManagers();
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("[WinLoseManager] Main menu scene name not set!");
        }
    }

    private void CleanupAllManagers()
    {
        Debug.Log("[WinLoseManager] Cleaning up all singleton managers...");

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

        Debug.Log("[WinLoseManager] All managers cleaned up successfully");
    }

    private void OnDestroy()
    {
        if (replayButtonWin != null)
            replayButtonWin.onClick.RemoveListener(ReloadScene);
        if (replayButtonLose != null)
            replayButtonLose.onClick.RemoveListener(ReloadScene);
        if (mainMenuButtonWin != null)
            mainMenuButtonWin.onClick.RemoveListener(LoadMainMenu);
        if (mainMenuButtonLose != null)
            mainMenuButtonLose.onClick.RemoveListener(LoadMainMenu);
        if (cutsceneButton != null)
            cutsceneButton.onClick.RemoveListener(OnCutsceneButtonClicked);
    }
}