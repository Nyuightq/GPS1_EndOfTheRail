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
    [SerializeField] private Button declineButton;

    [Header("TMP References")]
    [SerializeField] private TMP_Text messageText;

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
        declineButton.onClick.RemoveAllListeners();

        // Display message
        if (messageText != null)
            messageText.text = "Do you wish to heal your Crystal by " + healAmount + " HP?";

        // Yes = heal and close
        yesButton.onClick.AddListener(() =>
        {
            CrystalHP crystal = FindObjectOfType<CrystalHP>();
            if (crystal != null)
                crystal.Heal(healAmount);

            Debug.Log("Crystal healed by " + healAmount);
            CloseChurchUI();
        });

        // Decline = just close
        declineButton.onClick.AddListener(() =>
        {
            Debug.Log("Player declined Church healing.");
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
