using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

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
    [SerializeField] private Button declineButton;

    [Header("TMP References")]
    [SerializeField] private TMP_Text scrapText;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Item Costs")]
    [SerializeField] private int item1Cost = 3;
    [SerializeField] private int item2Cost = 5;
    [SerializeField] private int item3Cost = 7;

    private PlayerInventoryTemp currentPlayer;

    // --- NEW: State controls ---
    public bool IsTransactionUIActive { get; private set; } = false;
    public bool IsCooldownActive { get; private set; } = false;

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
        if (IsTransactionUIActive || IsCooldownActive)
        {
            Debug.Log("[TransactionManager] Ignored OpenTransactionUI request (UI busy or cooldown active)");
            return;
        }

        currentPlayer = player.GetComponent<PlayerInventoryTemp>();
        if (currentPlayer == null) return;

        if (uiPanel != null)
        {
            uiPanel.SetActive(true);
            IsTransactionUIActive = true;

            // Clear old listeners
            item1Button.onClick.RemoveAllListeners();
            item2Button.onClick.RemoveAllListeners();
            item3Button.onClick.RemoveAllListeners();
            declineButton.onClick.RemoveAllListeners();

            // Add purchase logic
            item1Button.onClick.AddListener(() => TryPurchase(item1Cost, "Item 1"));
            item2Button.onClick.AddListener(() => TryPurchase(item2Cost, "Item 2"));
            item3Button.onClick.AddListener(() => TryPurchase(item3Cost, "Item 3"));

            // Decline logic
            declineButton.onClick.AddListener(() =>
            {
                feedbackText.text = "Transaction declined.";
                Debug.Log("Player declined the transaction.");
                CloseTransactionUI();
            });

            UpdateUI();
        }

        Debug.Log("[TransactionManager] Transaction UI opened");
    }

    public void CloseTransactionUI()
    {
        if (!IsTransactionUIActive)
            return;

        if (uiPanel != null)
            uiPanel.SetActive(false);

        currentPlayer = null;
        IsTransactionUIActive = false;

        Debug.Log("Transaction UI closed. Event fired.");
        OnTransactionClosed?.Invoke();

        // Begin cooldown before another tile can trigger
        StartCoroutine(CloseCooldown());
    }

    private IEnumerator CloseCooldown()
    {
        IsCooldownActive = true;
        yield return new WaitForSeconds(0.5f);
        IsCooldownActive = false;
        Debug.Log("[TransactionManager] Cooldown finished, ready for next trigger.");
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

            CloseTransactionUI(); // successful purchase closes UI
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
