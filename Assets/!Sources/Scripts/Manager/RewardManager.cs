using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private Button componentButton;
    [SerializeField] private Button scrapButton;
    [SerializeField] private Button nothingButton;

    [Header("TMP References")]
    [SerializeField] private TMP_Text componentText;
    [SerializeField] private TMP_Text scrapText;

        // NEW EVENT
    public static event System.Action OnRewardClosed;

    private PlayerInventoryTemp currentPlayer;

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

    public void OpenRewardUI(GameObject player)
    {
        currentPlayer = player.GetComponent<PlayerInventoryTemp>();
        if (currentPlayer == null) return;

        if (uiPanel != null)
        {
            uiPanel.SetActive(true);

            // clear previous listeners
            componentButton.onClick.RemoveAllListeners();
            scrapButton.onClick.RemoveAllListeners();
            nothingButton.onClick.RemoveAllListeners();

            // set button actions
            componentButton.onClick.AddListener(() => { currentPlayer.AddComponent(); UpdateUI(); CloseRewardUI(); });
            scrapButton.onClick.AddListener(() => { currentPlayer.AddScrap(); UpdateUI(); CloseRewardUI(); });
            nothingButton.onClick.AddListener(() => { CloseRewardUI(); });

            UpdateUI();
        }
    }

public void CloseRewardUI()
{
    if (uiPanel != null)
        uiPanel.SetActive(false);

    currentPlayer = null;

    // Trigger event
    OnRewardClosed?.Invoke();
}


    private void UpdateUI()
    {
        if (currentPlayer == null) return;

        if (componentText != null)
            componentText.text = "Components: " + currentPlayer.componentCount;

        if (scrapText != null)
            scrapText.text = "Scraps: " + currentPlayer.scrapCount;
    }

    public void DestroyInstance()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
            Instance = null; // OK because this is inside Reward Manager
            Debug.Log("[RewardManager] Instance destroyed for replay.");
        }
    }
}
