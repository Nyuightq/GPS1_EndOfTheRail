using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages instruction UI panels that pause the game when displayed.
/// Shows instructions via help button, with navigation between panels.
/// </summary>
public class HelpManager : MonoBehaviour
{
    [Header("Instruction Panels")]
    [SerializeField] private GameObject instructionContainer;
    [SerializeField] private GameObject[] instructionPanels;

    [Header("Navigation Buttons")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;

    private int currentPanelIndex;
    private float previousTimeScale;

    public int CurrentPanelIndex => currentPanelIndex;
    public int TotalPanels => instructionPanels?.Length ?? 0;
    public bool IsShowing => instructionContainer != null && instructionContainer.activeSelf;

    private void Awake()
    {
        ValidateSetup();
        SetupButtons();
    }

    private void Start()
    {
        if (instructionContainer != null)
            instructionContainer.SetActive(false);
    }

    private void ValidateSetup()
    {
        if (instructionPanels == null || instructionPanels.Length == 0)
        {
            Debug.Log("[InstructionUI] Instruction panels not assigned!");
            enabled = false;
            return;
        }

        if (instructionContainer == null)
        {
            Debug.Log("[InstructionUI] Instruction container not assigned. Using first panel's parent.");
            if (instructionPanels[0] != null)
                instructionContainer = instructionPanels[0].transform.parent.gameObject;
        }
    }

    private void SetupButtons()
    {
        if (previousButton != null)
            previousButton.onClick.AddListener(OnPreviousButtonClicked);
        else
            Debug.Log("[InstructionUI] Previous button not assigned!");

        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextButtonClicked);
        else
            Debug.Log("[InstructionUI] Next button not assigned!");
    }

    /// <summary>
    /// Call this method from your help button to show instructions
    /// </summary>
    public void ShowInstructions()
    {
        if (instructionContainer == null) return;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        currentPanelIndex = 0;
        instructionContainer.SetActive(true);
        ShowCurrentPanel();
        UpdateButtonStates();

        Debug.Log("[InstructionUI] Instructions shown, game paused");
    }

    /// <summary>
    /// Hides the instruction panels and resumes the game
    /// </summary>
    public void HideInstructions()
    {
        if (instructionContainer == null) return;

        instructionContainer.SetActive(false);
        Time.timeScale = previousTimeScale;

        Debug.Log("[InstructionUI] Instructions hidden, game resumed");
    }

    public void OnPreviousButtonClicked()
    {
        if (currentPanelIndex > 0)
        {
            currentPanelIndex--;
            ShowCurrentPanel();
            UpdateButtonStates();
            Debug.Log($"[InstructionUI] Returned to panel {currentPanelIndex}");
        }
    }

    public void OnNextButtonClicked()
    {
        if (currentPanelIndex >= instructionPanels.Length - 1)
        {
            Debug.Log("[InstructionUI] Exiting instructions");
            HideInstructions();
        }
        else
        {
            currentPanelIndex++;
            ShowCurrentPanel();
            UpdateButtonStates();
            Debug.Log($"[InstructionUI] Moved to panel {currentPanelIndex}");
        }
    }

    private void ShowCurrentPanel()
    {
        for (int i = 0; i < instructionPanels.Length; i++)
        {
            if (instructionPanels[i] != null)
                instructionPanels[i].SetActive(i == currentPanelIndex);
        }
    }

    private void UpdateButtonStates()
    {
        if (previousButton != null)
            previousButton.interactable = currentPanelIndex > 0;

        if (nextButton != null)
        {
            nextButton.interactable = true;

            var buttonText = nextButton.GetComponentInChildren<Text>();
            if (buttonText != null)
                buttonText.text = currentPanelIndex >= instructionPanels.Length - 1 ? "Close" : "Next";
        }
    }

    private void OnDestroy()
    {
        // Ensure time scale is restored if object is destroyed while instructions are showing
        if (IsShowing)
            Time.timeScale = previousTimeScale;

        if (previousButton != null)
            previousButton.onClick.RemoveListener(OnPreviousButtonClicked);

        if (nextButton != null)
            nextButton.onClick.RemoveListener(OnNextButtonClicked);
    }
}