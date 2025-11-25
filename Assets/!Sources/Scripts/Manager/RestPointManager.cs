// --------------------------------------------------------------
// Creation Date: 2025-10-22 23:55
// Modified: 2025-11-25
// Author: ZQlie
// Description: Manages rest point interactions including healing and optional night/day skip
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RestPointManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStatusManager playerStatusManager;
    [SerializeField] private DayCycleScript dayCycleScript;
    
    [Header("UI References")]
    [SerializeField] private Button healPromptButton;
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button cancelButton;
    
    [Header("Heal Settings")]
    [SerializeField] private int healAmount = 20;
    [SerializeField] private int scrapCost = 10;
    
    [Header("Skip Time Settings (Editor Only)")]
    [SerializeField] private bool skipNightOnRestPoint = false;
    [Tooltip("If enabled, night will be automatically skipped when entering a rest point during night time")]
    [SerializeField] private bool skipDayOnRestPoint = false;
    [Tooltip("If enabled, day will be automatically skipped when entering a rest point during day time")]
    [SerializeField] private OnDayToNight dayNightController;
    [SerializeField] private OnTrainLight trainLightController;
    
    private bool isOnRestPoint = false;
    private TrainMovement trainMovement;

    private void Start()
    {
        // Subscribe to heal button clicks
        if (healPromptButton != null)
            healPromptButton.onClick.AddListener(OnPromptButtonClicked);
        
        if (acceptButton != null)
            acceptButton.onClick.AddListener(OnAcceptButtonClicked);
        
        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
        
        // Find DayCycleScript if not assigned
        if (dayCycleScript == null)
            dayCycleScript = FindFirstObjectByType<DayCycleScript>();
        
        // Find OnDayToNight if not assigned
        if (dayNightController == null)
            dayNightController = FindFirstObjectByType<OnDayToNight>();

        // Find OnTrainLight if not assigned
        if (trainLightController == null)
            trainLightController = FindFirstObjectByType<OnTrainLight>();
        
        HideAll();
    }

    private void OnDestroy()
    {
        // Unsubscribe from heal button clicks
        if (healPromptButton != null)
            healPromptButton.onClick.RemoveListener(OnPromptButtonClicked);
        
        if (acceptButton != null)
            acceptButton.onClick.RemoveListener(OnAcceptButtonClicked);
        
        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
    }

    public void OnRestPointEntered(TrainMovement train)
    {
        // Put SFX - UI_PlanPhase_SlideIn
        SoundManager.Instance.PlaySFX("SFX_UI_PlanPhase_SlideIn");
        Debug.Log("Play - UI_PlanPhase_SlideIn");
        
        isOnRestPoint = true;
        trainMovement = train;

        //----------Debug for tileMoevd = 0 when enter rest point-------------
        if (dayCycleScript != null)
        {
            int tiles = dayCycleScript.getTilesMoved();
            if (tiles == 0)
            {
                Debug.Log("[RestPoint] tilesMoved == 0 on entering rest point (start of time phase).");
            }
            else
            {
                Debug.Log($"[RestPoint] tilesMoved on enter = {tiles}");
            }
        }
        //----------Debug for tileMoevd = 0 when enter rest point-------------

        if (healPromptButton != null)
            healPromptButton.gameObject.SetActive(true);
        
        // Auto-skip time based on current state
        if (dayCycleScript != null)
        {
            if (dayCycleScript.CurrentTime == DayCycleScript.TimeState.Night && skipNightOnRestPoint)
            {
                Debug.Log("[Editor Feature] Auto-skipping night on rest point");
                SkipToNextDay();
            }
            else if (dayCycleScript.CurrentTime == DayCycleScript.TimeState.Day && skipDayOnRestPoint)
            {
                Debug.Log("[Editor Feature] Auto-skipping day on rest point");
                SkipToNextDay();
            }
        }
        
        Debug.Log("Entered Rest Point - Heal Button Available");
    }

    public void OnRestPointExited()
    {
        isOnRestPoint = false;
        trainMovement = null;
        HideAll();
        
        Debug.Log("Exited Rest Point");
    }

    private void OnPromptButtonClicked()
    {
        if (!isOnRestPoint || playerStatusManager == null)
            return;

        // Check if already at max HP
        if (playerStatusManager.Hp >= playerStatusManager.MaxHp)
        {
            ShowConfirmPanel("Already at full health!", false);
            return;
        }

        // Check if player has enough scraps
        bool hasEnoughScraps = playerStatusManager.Scraps >= scrapCost;
        
        string description = hasEnoughScraps 
            ? $"Heal {healAmount} HP for {scrapCost} scraps?\nCurrent HP: {playerStatusManager.Hp}/{playerStatusManager.MaxHp}\nCurrent Scraps: {playerStatusManager.Scraps}"
            : $"Not enough scraps!\nNeed: {scrapCost}\nHave: {playerStatusManager.Scraps}";

        ShowConfirmPanel(description, hasEnoughScraps);
    }

    private void OnAcceptButtonClicked()
    {
        if (playerStatusManager == null)
            return;

        // Try to consume scraps
        if (!playerStatusManager.ConsumeScraps(scrapCost))
        {
            Debug.LogWarning("Not enough scraps to heal!");
            HideConfirmPanel();
            return;
        }

        // Heal the player
        playerStatusManager.HealCurrentHp(healAmount);
        
        Debug.Log($"Healed {healAmount} HP for {scrapCost} scraps");
        
        HideConfirmPanel();
    }

    private void OnCancelButtonClicked()
    {
        HideConfirmPanel();
    }

    /// <summary>
    /// Skips the current time phase (day or night) and fast forwards to the next day.
    /// Works for both day->night->day and night->day transitions.
    /// </summary>
    private void SkipToNextDay()
    {
        if (dayCycleScript == null)
        {
            Debug.LogWarning("DayCycleScript not found!");
            return;
        }

        DayCycleScript.TimeState currentState = dayCycleScript.CurrentTime;
        int currentDay = dayCycleScript.GetDay();

        Debug.Log($"[Editor] Starting skip from {currentState} (Day {currentDay})");

        if (currentState == DayCycleScript.TimeState.Day)
        {
            // Skip day -> transition to night -> immediately skip night -> arrive at next day
            StartCoroutine(SkipDayToNextDay(currentDay));
        }
        else // Night
        {
            // Skip night -> transition to next day
            StartCoroutine(SkipNightToNextDay(currentDay));
        }
    }

    private System.Collections.IEnumerator SkipDayToNextDay(int startDay)
    {
        // Step 1: Fast forward day to trigger night transition
        int requiredTiles = dayCycleScript.DayLength + dayCycleScript.DayLengthMod;
        dayCycleScript.setTilesMoved(requiredTiles);
        
        Debug.Log($"[Editor] Fast forwarding day (set tiles to {requiredTiles})");

        // Wait for the day->night transition to complete
        yield return null;
        yield return null; // Extra frame for safety
        
        // Step 2: Verify we're in night, then immediately skip night
        if (dayCycleScript.CurrentTime == DayCycleScript.TimeState.Night)
        {
            Debug.Log("[Editor] Day->Night transition complete, now skipping night");
            
            // Immediately end the night
            dayCycleScript.setTilesMoved(dayCycleScript.NightLength);
            
            // Wait for night->day transition
            yield return null;
            yield return null;
            
            // Reset lights to day state
            ResetLightsToDay();
            
            int newDay = dayCycleScript.GetDay();
            Debug.Log($"[Editor] Day skip complete - Advanced from Day {startDay} to Day {newDay}");
        }
        else
        {
            Debug.LogWarning("[Editor] Failed to transition to night!");
        }
    }

    private System.Collections.IEnumerator SkipNightToNextDay(int startDay)
    {
        // Fast forward night to trigger day transition
        dayCycleScript.setTilesMoved(dayCycleScript.NightLength);
        
        Debug.Log($"[Editor] Fast forwarding night (set tiles to {dayCycleScript.NightLength})");
        
        // Wait for the night->day transition to complete
        yield return null;
        yield return null; // Extra frame for safety
        
        // Verify we're in day state
        if (dayCycleScript.CurrentTime == DayCycleScript.TimeState.Day)
        {
            // Reset lights to day state
            ResetLightsToDay();
            
            int newDay = dayCycleScript.GetDay();
            Debug.Log($"[Editor] Night skip complete - Advanced from Day {startDay} to Day {newDay}");
        }
        else
        {
            Debug.LogWarning("[Editor] Failed to transition to day!");
        }
    }

    private void ResetLightsToDay()
    {
        // Force light intensity back to day
        if (dayNightController != null)
        {
            dayNightController.ForceResetToDay();
        }

        // Force train light intensity and alpha back to day
        if (trainLightController != null)
        {
            trainLightController.ForceResetToDay();
        }
        
        Debug.Log("[Editor] Lights reset to day state");
    }

    private void ShowConfirmPanel(string description, bool canAfford)
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(true);
        
        if (descriptionText != null)
            descriptionText.text = description;
        
        if (acceptButton != null)
            acceptButton.interactable = canAfford;
        
        // Hide buttons when panel is open
        if (healPromptButton != null)
            healPromptButton.gameObject.SetActive(false);
    }

    private void HideConfirmPanel()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
        
        // Show heal button again if on rest point
        if (isOnRestPoint && healPromptButton != null)
            healPromptButton.gameObject.SetActive(true);
    }

    private void HideAll()
    {
        if (healPromptButton != null)
            healPromptButton.gameObject.SetActive(false);
        
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }

    // Optional: Public method to set heal parameters dynamically
    public void SetHealParameters(int amount, int cost)
    {
        healAmount = amount;
        scrapCost = cost;
    }
}