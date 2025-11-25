// --------------------------------------------------------------
// Creation Date: 2025-10-31
// Description: Handles win/lose conditions and triggers transitions
// --------------------------------------------------------------
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public class WinLoseManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStatusManager playerStatus;
    [SerializeField] private TrainMovement trainMovement;
    [SerializeField] private RailGridScript gridManager;
    [SerializeField] private DayCycleScript dayCycle;
    [SerializeField] private CombatManager combatManager;
    
    [Header("Data")]
    [SerializeField] private RunStatsData runStatsData; // Reference to ScriptableObject

    [Header("Scene Settings")]
    [SerializeField] private string resultSceneName = "ResultScene"; // Change this to your actual scene name
    [SerializeField] private float transitionDelay = 1f;
    // [SerializeField] private bool useEditorSceneLoading = true; // Enable for editor testing without Build Settings

    [Header("Game State")]
    public bool isGameOver = false;

    private bool _initialized = false;

    private IEnumerator Start()
    {
        yield return null; // Wait one frame for dynamic objects to spawn
        _initialized = true;

        // if (replayButtonWin != null)
        //     replayButtonWin.onClick.AddListener(ReloadScene);
        // if (replayButtonLose != null)
        //     replayButtonLose.onClick.AddListener(ReloadScene);

        // if (winPanel != null) winPanel.SetActive(false);
        // if (losePanel != null) losePanel.SetActive(false);

        // Try to auto-assign runtime-spawned references
        // if (trainMovement == null)
        //     trainMovement = FindFirstObjectByType<TrainMovement>();
        // if (playerStatus == null)
        //     playerStatus = FindFirstObjectByType<PlayerStatusManager>();
        // if (gridManager == null)
        //     gridManager = FindFirstObjectByType<RailGridScript>();

        // if (trainMovement == null)
        //     Debug.LogWarning("WinLoseManager could not find TrainMovement at start.");
        // else
        //     Debug.Log($"TrainMovement assigned: {trainMovement.name}");
        // Auto-assign runtime-spawned references
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
            // else
            //     Debug.LogWarning("[WinLoseManager] TrainMovement not found!");
        }
        
        if (playerStatus == null)
        {
            playerStatus = FindFirstObjectByType<PlayerStatusManager>();
            if (playerStatus != null)
                Debug.Log($"[WinLoseManager] Found PlayerStatusManager: {playerStatus.name}");
            else
                Debug.LogWarning("[WinLoseManager] PlayerStatusManager not found!");
        }
        
        if (gridManager == null)
        {
            gridManager = FindFirstObjectByType<RailGridScript>();
            if (gridManager != null)
                Debug.Log($"[WinLoseManager] Found RailGridScript: {gridManager.name}");
            else
                Debug.LogWarning("[WinLoseManager] RailGridScript not found!");
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
            Debug.Log("Player Lost! Reason: Player HP reached 0.");
            TriggerLose("HP reached 0");
        }
        else if (playerStatus.CrystalHp <= 0)
        {
            Debug.Log("Player Lost! Reason: Crystal HP reached 0.");
            TriggerLose("Crystal HP reached 0");
        }
    }

    private void CheckWinCondition()
    {
        if (trainMovement == null || gridManager == null) return;

        Grid grid = gridManager.GetComponent<Grid>();
        if (grid == null)
        {
            Debug.LogWarning("No Grid component found on RailGridScript GameObject.");
            return;
        }

        Vector3Int currentTile = grid.WorldToCell(trainMovement.transform.position);
        RailData currentRail = gridManager.GetRailAtPos(currentTile);

        if (currentRail != null && currentRail.railType == RailData.railTypes.end)
        {
            if (playerStatus != null &&
                playerStatus.Hp > 0 && playerStatus.CrystalHp > 0)
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

        Debug.Log("Win triggered! Saving stats and transitioning...");

        // Save run stats
        SaveCurrentRunStats(true, "");

        // Transition to result scene
        StartCoroutine(TransitionToResultScene());
    }

    private void TriggerLose(string reason = "Unknown")
    {
        if (isGameOver) return;

        isGameOver = true;

        if (trainMovement != null)
            trainMovement.enabled = false;

        Debug.Log($"Player Lost! Reason: {reason}");
        GameStateManager.SetPhase(Phase.Lose);

        // Save run stats
        SaveCurrentRunStats(false, reason);

        Time.timeScale = 0f;

        // Transition to result scene
        StartCoroutine(TransitionToResultScene());
    }

    private void SaveCurrentRunStats(bool won, string loseReason)
    {
        if (runStatsData == null)
        {
            Debug.LogWarning("RunStatsData ScriptableObject is not assigned! Cannot save stats.");
            return;
        }

        if (playerStatus == null)
        {
            Debug.LogWarning("PlayerStatusManager is null! Cannot save stats.");
            return;
        }

        int days = dayCycle != null ? dayCycle.GetDay() : 0;
        int totalCombats = combatManager != null ? 
            (combatManager.totalCombatsFaced + combatManager.totalEncountersFaced) : 0;

        runStatsData.SaveRunStats(
            playerStatus.Hp,
            playerStatus.MaxHp,
            playerStatus.CrystalHp,
            playerStatus.MaxCrystalHp,
            playerStatus.Scraps,
            days,
            totalCombats,
            0, // encounters (if tracked separately)
            won,
            loseReason
        );
    }

    private IEnumerator TransitionToResultScene()
    {
        Debug.Log($"Transitioning to '{resultSceneName}' in {transitionDelay} seconds...");

        float elapsed = 0f;
        while (elapsed < transitionDelay)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 1f;
        
        if (!string.IsNullOrEmpty(resultSceneName))
        {
            try
            {
                SceneManager.LoadScene(resultSceneName);
                Debug.Log($"Loading result scene: {resultSceneName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load scene '{resultSceneName}': {e.Message}");
                Debug.LogError("Make sure the scene is added to Build Settings (File → Build Settings → Add Open Scenes)");
            }
        }
        else
        {
            Debug.LogError("Result scene name not set in WinLoseManager!");
        }
    }
}