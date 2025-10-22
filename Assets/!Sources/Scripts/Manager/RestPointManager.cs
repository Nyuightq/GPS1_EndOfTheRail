// --------------------------------------------------------------
// Creation Date: 2025-10-22 23:55
// Author: ZQlie
// Description: -
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;

public class RestPointManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject restPanel;          // First panel (shows "Rest" button)
    [SerializeField] private GameObject healConfirmPanel;   // Second panel (confirm heal)
    [SerializeField] private Button restButton;             // Button to open heal confirmation
    [SerializeField] private Button healButton;             // Button to perform heal
    [SerializeField] private Button cancelButton;           // Button to cancel heal
    [SerializeField] private PlayerStatusManager playerStatus;
    [SerializeField] private RailGridScript railGrid;       // Reference to the grid to detect rest point tiles

    [Header("Healing Settings")]
    [SerializeField] private int healCost = 5;              // Scraps required for full heal
    [SerializeField] private int healAmount = 999;          // Amount of HP restored (set to large for full heal)

    private bool isPlayerOnRestTile = false;
    private GameObject player;

    private void Start()
    {
        // Ensure panels are hidden at start
        restPanel.SetActive(false);
        healConfirmPanel.SetActive(false);

        if (restButton != null)
            restButton.onClick.AddListener(OpenHealConfirmPanel);

        if (healButton != null)
            healButton.onClick.AddListener(AttemptHeal);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(CloseHealConfirmPanel);

        // Find player automatically if not set
        if (playerStatus == null)
            playerStatus = FindFirstObjectByType<PlayerStatusManager>();

        if (railGrid == null)
            railGrid = FindFirstObjectByType<RailGridScript>();

        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        if (player == null || railGrid == null)
            return;

        // Check if the player is currently standing on a rest tile
        bool onRestTile = railGrid.IsPlayerOnRestTile(player.transform.position);

        if (onRestTile && !isPlayerOnRestTile)
        {
            // Player just entered a rest tile
            isPlayerOnRestTile = true;
            ShowRestPanel(true);
        }
        else if (!onRestTile && isPlayerOnRestTile)
        {
            // Player just left the rest tile
            isPlayerOnRestTile = false;
            ShowRestPanel(false);
            healConfirmPanel.SetActive(false);
        }
    }

    private void ShowRestPanel(bool show)
    {
        restPanel.SetActive(show);
    }

    private void OpenHealConfirmPanel()
    {
        healConfirmPanel.SetActive(true);
    }

    private void CloseHealConfirmPanel()
    {
        healConfirmPanel.SetActive(false);
    }

    private void AttemptHeal()
    {
        if (playerStatus == null)
            return;

        if (playerStatus.ConsumeScraps(healCost))
        {
            playerStatus.HealCurrentHp(healAmount);
            Debug.Log($"[RestPointManager] Player healed by {healAmount} HP for {healCost} scraps.");
        }
        else
        {
            Debug.LogWarning("[RestPointManager] Not enough scraps to heal!");
        }

        healConfirmPanel.SetActive(false);
    }
}
