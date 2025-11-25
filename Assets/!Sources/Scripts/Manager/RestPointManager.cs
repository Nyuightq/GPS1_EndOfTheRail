// --------------------------------------------------------------
// Creation Date: 2025-10-22 23:55
// Author: ZQlie
// Description: Manages rest point interactions including healing and optional night skip
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
    
    [Header("Skip Night Settings (Editor Only)")]
    [SerializeField] private bool skipNightOnRestPoint = false;
    [Tooltip("If enabled, night will be automatically skipped when entering a rest point during day time")]
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

        // Find PlayerStatusManager if not assigned
        if (playerStatusManager == null)
        playerStatusManager = FindFirstObjectByType<PlayerStatusManager>();
        
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
        
        if (healPromptButton != null)
            healPromptButton.gameObject.SetActive(true);
        
        // Auto-skip night if enabled and currently day time
        if (skipNightOnRestPoint && dayCycleScript != null)
        {
            SkipToNextDay();
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

    private void SkipToNextDay()
    {
        if (dayCycleScript == null) return;

        // Determine how many tiles to move to end the current phase
        int requiredTiles = 0;
        if (dayCycleScript.CurrentTime == DayCycleScript.TimeState.Day)
        {
            requiredTiles = dayCycleScript.DayLength + dayCycleScript.DayLengthMod;
        }
        else // Night
        {
            requiredTiles = dayCycleScript.NightLength + dayCycleScript.DayLengthMod;
        }

        // Move tiles to force the phase to the next day
        dayCycleScript.setTilesMoved(requiredTiles);

        // Immediately reset lights and orb
        dayNightController?.ForceResetToDay();
        trainLightController?.ForceResetToDay();

        Debug.Log("[Editor] Skipped to next day and reset lighting");
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