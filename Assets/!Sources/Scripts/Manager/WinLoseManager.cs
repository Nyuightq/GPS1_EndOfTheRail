// --------------------------------------------------------------
// Creation Date: 2025-10-13
// Author: ZQlie
// Description: Handles win/lose conditions based on player and crystal HP,
// and when player reaches the end point. Pauses game on win/lose.
// --------------------------------------------------------------
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinLoseManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStatusManager playerStatus;
    [SerializeField] private CrystalHP crystalHP;
    [SerializeField] private TrainMovement trainMovement;
    [SerializeField] private RailGridScript gridManager;

    [Header("UI Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private Button replayButtonWin;
    [SerializeField] private Button replayButtonLose;

    [Header("Game State")]
    public bool isGameOver = false;
    public bool isPlayerWin = false;

    private bool _initialized = false;

    private IEnumerator Start()
    {
        yield return null; // wait one frame so all dynamic objects can spawn
        _initialized = true;

        if (replayButtonWin != null)
            replayButtonWin.onClick.AddListener(ReloadScene);
        if (replayButtonLose != null)
            replayButtonLose.onClick.AddListener(ReloadScene);

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        // Auto-assign runtime references
        if (trainMovement == null)
            trainMovement = FindFirstObjectByType<TrainMovement>();
        if (playerStatus == null)
            playerStatus = FindFirstObjectByType<PlayerStatusManager>();
        if (crystalHP == null)
            crystalHP = FindFirstObjectByType<CrystalHP>();
        if (gridManager == null)
            gridManager = FindFirstObjectByType<RailGridScript>();

        if (trainMovement == null)
            Debug.LogWarning("WinLoseManager could not find TrainMovement at start.");
        else
            Debug.Log($"TrainMovement assigned: {trainMovement.name}");
    }

    private void Update()
    {
        if (!_initialized || isGameOver) return;

        // Re-assign if still missing
        if (trainMovement == null)
            trainMovement = FindFirstObjectByType<TrainMovement>();
        if (playerStatus == null)
            playerStatus = FindFirstObjectByType<PlayerStatusManager>();
        if (crystalHP == null)
            crystalHP = FindFirstObjectByType<CrystalHP>();
        if (gridManager == null)
            gridManager = FindFirstObjectByType<RailGridScript>();

        if (trainMovement == null || playerStatus == null || crystalHP == null || gridManager == null)
            return;

        CheckHealthCondition();
        CheckWinCondition();
    }

    private void CheckHealthCondition()
    {
        if (playerStatus == null || crystalHP == null) return;

        if (playerStatus.Hp <= 0)
        {
            Debug.Log("Player Lost! Reason: Player HP reached 0.");
            TriggerLose("Player HP reached 0");
        }
        else if (crystalHP.currentHP <= 0)
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
            if (playerStatus != null && crystalHP != null &&
                playerStatus.Hp > 0 && crystalHP.currentHP > 0)
            {
                TriggerWin();
            }
        }
    }

    private void TriggerWin()
    {
        isGameOver = true;
        isPlayerWin = true;

        if (trainMovement != null)
            trainMovement.enabled = false;

        Debug.Log("Player Won! Reached End Point Safely!");
        GameStateManager.SetPhase(Phase.Win);

        Time.timeScale = 0f;
        if (winPanel != null)
            winPanel.SetActive(true);
    }

    private void TriggerLose(string reason = "Unknown")
    {
        isGameOver = true;
        isPlayerWin = false;

        if (trainMovement != null)
            trainMovement.enabled = false;

        Debug.Log($"Player Lost! Reason: {reason}");
        GameStateManager.SetPhase(Phase.Lose);

        Time.timeScale = 0f;
        if (losePanel != null)
            losePanel.SetActive(true);
    }

    private void ReloadScene()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
        Debug.Log("Reloading scene...");
    }
}
