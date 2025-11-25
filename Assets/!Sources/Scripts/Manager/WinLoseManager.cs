// --------------------------------------------------------------
// Creation Date: 2025-10-31
// Description: Handles win/lose conditions - shows in-scene results
// --------------------------------------------------------------

// Add counter for successful merge
// Pending counter for Item Acquired
// Perhaps Scraps Spended for Expanding Inventory + Scraps Spended for buying items + total scraps acquired

using System.Collections;
using UnityEngine;
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
    
    [Header("Inventory Canvas")]
    [SerializeField] private Canvas inventoryCanvas; // Reference to the InventoryCanvas
    [SerializeField] private RectTransform winPanelInventoryAnchor; // Anchor point in Win Panel
    [SerializeField] private RectTransform losePanelInventoryAnchor; // Anchor point in Lose Panel
    [SerializeField] private bool showInventoryInResults = true; // Toggle inventory display
    
    [Header("Action Buttons")]
    [SerializeField] private UnityEngine.UI.Button replayButtonWin;
    [SerializeField] private UnityEngine.UI.Button replayButtonLose;
    [SerializeField] private UnityEngine.UI.Button mainMenuButtonWin;
    [SerializeField] private UnityEngine.UI.Button mainMenuButtonLose;
    
    [Header("Win Panel Stats Text")]
    [SerializeField] private TMPro.TextMeshProUGUI winHpText;
    [SerializeField] private TMPro.TextMeshProUGUI winCrystalHpText;
    [SerializeField] private TMPro.TextMeshProUGUI winScrapsText;
    [SerializeField] private TMPro.TextMeshProUGUI winDaysText;
    [SerializeField] private TMPro.TextMeshProUGUI winCombatsText;

    [Header("Lose Panel Stats Text")]
    [SerializeField] private TMPro.TextMeshProUGUI loseReasonText;
    [SerializeField] private TMPro.TextMeshProUGUI loseScrapsText;
    [SerializeField] private TMPro.TextMeshProUGUI loseDaysText;
    [SerializeField] private TMPro.TextMeshProUGUI loseCombatsText;

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

    private IEnumerator Start()
    {
        yield return null; // Wait one frame for dynamic objects to spawn
        _initialized = true;

        // Hide all panels initially
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

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

        TryAssignReferences();
    }

    private void Update()
    {
        if (!_initialized || isGameOver) return;

        // Re-assign if still missing
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
            TriggerLose("HP reached 0");
        }
        else if (playerStatus.CrystalHp <= 0)
        {
            TriggerLose("Crystal HP reached 0");
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

        Debug.Log("Player Won! Showing Win Panel...");

        // Display win panel with stats
        DisplayWinStats();
        
        if (winPanel != null) winPanel.SetActive(true);
        if (losePanel != null) losePanel.SetActive(false);

        // Attach inventory to win panel
        if (showInventoryInResults)
            AttachInventoryToPanel(winPanelInventoryAnchor);
    }

    private void TriggerLose(string reason = "Unknown")
    {
        if (isGameOver) return;

        isGameOver = true;

        if (trainMovement != null)
            trainMovement.enabled = false;

        Debug.Log($"Player Lost! Reason: {reason}. Showing Lose Panel...");
        Time.timeScale = 0f;

        // Display lose panel with stats
        DisplayLoseStats(reason);
        
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(true);

        // Attach inventory to lose panel
        if (showInventoryInResults)
            AttachInventoryToPanel(losePanelInventoryAnchor);
    }

    private void DisplayWinStats()
    {
        if (playerStatus == null) return;

        int days = dayCycle != null ? dayCycle.GetDay() : 0;
        int totalCombats = combatManager != null ? 
            (combatManager.totalCombatsFaced + combatManager.totalEncountersFaced) : 0;

        if (winHpText != null)
            winHpText.text = $"HP: {playerStatus.Hp}/{playerStatus.MaxHp}";
        
        if (winCrystalHpText != null)
            winCrystalHpText.text = $"Crystal HP: {playerStatus.CrystalHp}/{playerStatus.MaxCrystalHp}";
        
        if (winScrapsText != null)
            winScrapsText.text = $"Scraps: {playerStatus.Scraps}";
        
        if (winDaysText != null)
            winDaysText.text = $"Days Survived: {days}";
        
        if (winCombatsText != null)
            winCombatsText.text = $"Combats Faced: {totalCombats}";

        Debug.Log($"[WinLoseManager] Win stats displayed - Days: {days}, Combats: {totalCombats}");
    }

    private void DisplayLoseStats(string reason)
    {
        if (playerStatus == null) return;

        int days = dayCycle != null ? dayCycle.GetDay() : 0;
        int totalCombats = combatManager != null ? 
            (combatManager.totalCombatsFaced + combatManager.totalEncountersFaced) : 0;

        if (loseReasonText != null)
            loseReasonText.text = $"Reason: {reason}";
        
        if (loseScrapsText != null)
            loseScrapsText.text = $"Scraps: {playerStatus.Scraps}";
        
        if (loseDaysText != null)
            loseDaysText.text = $"Days Survived: {days}";
        
        if (loseCombatsText != null)
            loseCombatsText.text = $"Combats Faced: {totalCombats}";

        Debug.Log($"[WinLoseManager] Lose stats displayed - Reason: {reason}, Days: {days}");
    }

    /// <summary>
    /// Attaches the inventory canvas to a specific anchor point in the result panel
    /// </summary>
    private void AttachInventoryToPanel(RectTransform anchorPoint)
    {
        if (inventoryCanvas == null || inventoryRectTransform == null || anchorPoint == null)
        {
            Debug.LogWarning("[WinLoseManager] Cannot attach inventory - missing references");
            return;
        }

        // Reparent inventory to the anchor point
        inventoryRectTransform.SetParent(anchorPoint, false);
        
        // Reset local position/scale to match anchor
        inventoryRectTransform.localPosition = Vector3.zero;
        inventoryRectTransform.localScale = Vector3.one;
        
        // Ensure inventory canvas is visible and on top
        inventoryCanvas.gameObject.SetActive(true);
        inventoryCanvas.sortingOrder = 100; // High value to ensure visibility
        
        Debug.Log($"[WinLoseManager] Inventory canvas attached to {anchorPoint.name}");
    }

    /// <summary>
    /// Restores inventory canvas to its original parent and state
    /// </summary>
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

        // Restore inventory canvas before cleanup
        RestoreInventoryCanvas();

        // Clean up ALL singleton managers before reload
        CleanupAllManagers();

        // Reset timescale
        Time.timeScale = 1f;

        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void LoadMainMenu()
    {
        Debug.Log("[WinLoseManager] Loading main menu...");
        
        // Restore inventory canvas before cleanup
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

    private void OnDestroy()
    {
        // Clean up button listeners
        if (replayButtonWin != null)
            replayButtonWin.onClick.RemoveListener(ReloadScene);
        if (replayButtonLose != null)
            replayButtonLose.onClick.RemoveListener(ReloadScene);
        if (mainMenuButtonWin != null)
            mainMenuButtonWin.onClick.RemoveListener(LoadMainMenu);
        if (mainMenuButtonLose != null)
            mainMenuButtonLose.onClick.RemoveListener(LoadMainMenu);
    }
}