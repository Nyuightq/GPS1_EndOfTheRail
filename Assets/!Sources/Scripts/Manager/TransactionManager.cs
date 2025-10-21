using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TransactionManager : MonoBehaviour
{
    public static TransactionManager Instance { get; private set; }

    // Event so TrainFreezeController can listen and resume the train
    public static event System.Action OnTransactionClosed;

    [Header("UI References")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private Button item1Button;
    [SerializeField] private Button item2Button;
    [SerializeField] private Button item3Button;
    [SerializeField] private Button declineButton; // NEW: Decline / Cancel button

    [Header("TMP References")]
    [SerializeField] private TMP_Text scrapText;
    [SerializeField] private TMP_Text feedbackText; // shows purchase result

    [Header("Item Costs")]
    [SerializeField] private int item1Cost = 3;
    [SerializeField] private int item2Cost = 5;
    [SerializeField] private int item3Cost = 7;

    private PlayerInventoryTemp currentPlayer;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (uiPanel != null)
            uiPanel.SetActive(false);
    }

    public void OpenTransactionUI(GameObject player)
    {
        currentPlayer = player.GetComponent<PlayerInventoryTemp>();
        if (currentPlayer == null) return;

        if (uiPanel != null)
        {
            uiPanel.SetActive(true);

            // Clear old listeners
            item1Button.onClick.RemoveAllListeners();
            item2Button.onClick.RemoveAllListeners();
            item3Button.onClick.RemoveAllListeners();
            declineButton.onClick.RemoveAllListeners();

            // Add purchase logic
            item1Button.onClick.AddListener(() => TryPurchase(item1Cost, "Item 1"));
            item2Button.onClick.AddListener(() => TryPurchase(item2Cost, "Item 2"));
            item3Button.onClick.AddListener(() => TryPurchase(item3Cost, "Item 3"));

            // Add decline logic
            declineButton.onClick.AddListener(() =>
            {
                feedbackText.text = "Transaction declined.";
                Debug.Log("Player declined the transaction.");
                CloseTransactionUI(); // Closes UI and resumes train
            });

            UpdateUI();
        }
    }

public void CloseTransactionUI()
{
    if (uiPanel != null)
        uiPanel.SetActive(false);

    currentPlayer = null;

    // Notify listeners (e.g. TrainFreezeController)
    OnTransactionClosed?.Invoke();

    Debug.Log("Transaction UI closed. Event fired.");
}


    private void TryPurchase(int cost, string itemName)
    {
        if (currentPlayer == null) return;

        if (currentPlayer.scrapCount >= cost)
        {
            currentPlayer.SpendScrap(cost);
            feedbackText.text = $"Purchased {itemName}!";
            Debug.Log($"{itemName} purchased for {cost} scraps.");

            UpdateUI();

            // Close UI and resume train after successful purchase
            CloseTransactionUI();
        }
        else
        {
            feedbackText.text = $"Not enough scraps for {itemName}!";
            Debug.Log("Not enough scraps!");
        }
    }

    private void UpdateUI()
    {
        if (currentPlayer == null) return;

        if (scrapText != null)
            scrapText.text = "Scraps: " + currentPlayer.scrapCount;
    }
}
