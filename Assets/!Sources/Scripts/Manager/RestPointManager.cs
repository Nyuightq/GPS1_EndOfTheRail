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
            if (dayCycleScript.CurrentTime == DayCycleScript.TimeState.Day)
            {
                Debug.Log("[Editor Feature] Auto-skipping night on rest point");
                SkipNightImmediate();
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

    private void SkipNightImmediate()
    {
        if (dayCycleScript == null)
            return;

        // Check if it's day time
        if (dayCycleScript.CurrentTime != DayCycleScript.TimeState.Day)
        {
            Debug.LogWarning("Can only skip night during day time!");
            return;
        }

        Debug.Log($"[Editor] Skipping night - Fast forwarding to Day {dayCycleScript.GetDay() + 1}");

        // Set tiles moved to the day length to trigger night transition
        int requiredTiles = dayCycleScript.DayLength + dayCycleScript.DayLengthMod;
        dayCycleScript.setTilesMoved(requiredTiles);

        // Wait one frame for night to start, then immediately end it
        StartCoroutine(SkipNightCoroutine());
    }

    private System.Collections.IEnumerator SkipNightCoroutine()
    {
        // Wait for one frame to let the day->night transition happen
        yield return null;
        
        // Check if we're now in night phase
        if (dayCycleScript.CurrentTime == DayCycleScript.TimeState.Night)
        {
            // Immediately end the night by setting tiles moved to night length
            dayCycleScript.setTilesMoved(dayCycleScript.NightLength);
            
            // Force light intensity back to day
            if (dayNightController != null)
            {
                dayNightController.ForceResetToDay();
            }
            
            Debug.Log("[Editor] Night skipped - Starting new day with lighting reset");
        }
        else
        {
            Debug.LogWarning("Failed to skip night - not in night phase");
        }
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