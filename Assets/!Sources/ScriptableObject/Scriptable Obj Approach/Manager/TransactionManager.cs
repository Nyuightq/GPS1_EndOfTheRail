using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TransactionManager : MonoBehaviour
{
    public static TransactionManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private Button item1Button;
    [SerializeField] private Button item2Button;
    [SerializeField] private Button item3Button;

    [Header("TMP References")]
    [SerializeField] private TMP_Text scrapText;
    [SerializeField] private TMP_Text feedbackText; // shows purchase result

    [Header("Item Costs")]
    [SerializeField] private int item1Cost = 3;
    [SerializeField] private int item2Cost = 5;
    [SerializeField] private int item3Cost = 7;

    private PlayerInventory currentPlayer;

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

    public void OpenTransactionUI(GameObject player)
    {
        currentPlayer = player.GetComponent<PlayerInventory>();
        if (currentPlayer == null) return;

        if (uiPanel != null)
        {
            uiPanel.SetActive(true);

            // Clear old listeners
            item1Button.onClick.RemoveAllListeners();
            item2Button.onClick.RemoveAllListeners();
            item3Button.onClick.RemoveAllListeners();

            // Add purchase logic
            item1Button.onClick.AddListener(() => TryPurchase(item1Cost, "Item 1"));
            item2Button.onClick.AddListener(() => TryPurchase(item2Cost, "Item 2"));
            item3Button.onClick.AddListener(() => TryPurchase(item3Cost, "Item 3"));

            UpdateUI();
        }
    }

    public void CloseTransactionUI()
    {
        if (uiPanel != null)
            uiPanel.SetActive(false);

        currentPlayer = null;
    }

    private void TryPurchase(int cost, string itemName)
    {
        if (currentPlayer == null) return;

        if (currentPlayer.scrapCount >= cost)
        {
            currentPlayer.SpendScrap(cost);
            feedbackText.text = $"Purchased {itemName}!";
            Debug.Log($"{itemName} purchased for {cost} scraps.");
        }
        else
        {
            feedbackText.text = $"Not enough scraps for {itemName}!";
            Debug.Log("Not enough scraps!");
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (currentPlayer == null) return;

        if (scrapText != null)
            scrapText.text = "Scraps: " + currentPlayer.scrapCount;
    }
}
