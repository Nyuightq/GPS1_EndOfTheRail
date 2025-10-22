// --------------------------------------------------------------
// Creation Date: 2025-10-09 18:55
// Author: ZQlie
// Description: -
// --------------------------------------------------------------
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChurchManager : MonoBehaviour
{
    public static ChurchManager Instance { get; private set; }

    // Event so TrainFreezeController can listen and resume movement
    public static event System.Action OnChurchClosed;

    [Header("UI References")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private Button yesButton;

    [Header("TMP References")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text yesButtonText; // Text on the yes button

    [Header("Healing Settings")]
    [SerializeField] private int healAmount = 20;

    private GameObject currentPlayer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (uiPanel != null)
            uiPanel.SetActive(false);
    }

    public void OpenChurchUI(GameObject player)
    {
        currentPlayer = player;
        if (uiPanel == null) return;

        uiPanel.SetActive(true);

        // Clear old listeners
        yesButton.onClick.RemoveAllListeners();

        // Narrative story text
        if (messageText != null)
        {
            messageText.text = "As the train halts beside the ancient chapel, the faint hum of energy fills the air. " +
                               "You sense the Crystal’s pulse weaken... yet the altar glows warmly, offering restoration.";
        }

        // Find the crystal once and keep reference for callback
        CrystalHP crystal = FindObjectOfType<CrystalHP>();

        // Update the button label with current and post-heal HP
        if (crystal != null && yesButtonText != null)
        {
            int currentHp = crystal.currentHP;
            int maxHp = crystal.maxHP;
            int healedHp = Mathf.Min(currentHp + healAmount, maxHp);
            yesButtonText.text = $"Heal Crystal ({currentHp} → {healedHp} HP)";

            // Adjust auto-sizing so long labels fit — requires TextMeshPro component to support auto-size
            yesButtonText.enableAutoSizing = true;
            yesButtonText.fontSizeMin = 14;
            yesButtonText.fontSizeMax = 36;
        }
        else if (yesButtonText != null)
        {
            // Fallback label if crystal not found
            yesButtonText.text = $"Heal Crystal (+{healAmount} HP)";
            yesButtonText.enableAutoSizing = true;
            yesButtonText.fontSizeMin = 14;
            yesButtonText.fontSizeMax = 36;
        }

        // Yes = heal and close. Use the captured 'crystal' reference.
        yesButton.onClick.AddListener(() =>
        {
            if (crystal != null)
            {
                crystal.Heal(healAmount);
                Debug.Log($"Crystal healed by {healAmount}. Current HP: {crystal.currentHP}");
            }
            else
            {
                Debug.LogWarning("No CrystalHP instance found to heal.");
            }

            CloseChurchUI();
        });
    }

    public void CloseChurchUI()
    {
        if (uiPanel != null)
            uiPanel.SetActive(false);

        currentPlayer = null;

        // Notify listeners to resume train movement
        OnChurchClosed?.Invoke();

        Debug.Log("Church UI closed. Event fired.");
    }
}

