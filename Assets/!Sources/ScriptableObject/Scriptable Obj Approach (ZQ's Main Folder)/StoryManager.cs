// --------------------------------------------------------------
// Creation Date: 2025-10-13
// Author: Liew ZQ
// Description: Manages story UI and integrates with StoryTiles.
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance { get; private set; }

    // Event that TrainFreezeController listens to
    public static event System.Action OnStoryClosed;

    [Header("UI References")]
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private TMP_Text storyTextUI;
    [SerializeField] private Image storyImageUI;
    [SerializeField] private Button continueButton;

    private GameObject currentPlayer;
    private StoryTile currentStoryTile;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (storyPanel != null)
            storyPanel.SetActive(false);

        if (continueButton != null)
            continueButton.onClick.AddListener(CloseStoryUI);
    }

    public void OpenStoryUI(StoryTile tile, GameObject player)
    {
        currentPlayer = player;
        currentStoryTile = tile;

        if (storyPanel == null || storyTextUI == null)
        {
            Debug.LogWarning("Story UI references missing!");
            return;
        }

        // Display text
        storyTextUI.text = tile.storyText;

        // Display image (if any)
        if (storyImageUI != null)
        {
            storyImageUI.sprite = tile.storyImage;
            storyImageUI.gameObject.SetActive(tile.storyImage != null);
        }

        storyPanel.SetActive(true);
    }

    public void CloseStoryUI()
    {
        if (storyPanel != null)
            storyPanel.SetActive(false);

        currentPlayer = null;
        currentStoryTile = null;

        // Fire event to resume train
        OnStoryClosed?.Invoke();

        Debug.Log("Story UI closed. Event fired to resume train.");
    }
}
