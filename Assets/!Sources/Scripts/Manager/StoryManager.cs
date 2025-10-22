// --------------------------------------------------------------
// Creation Date: 2025-10-13
// Author: Liew ZQ
// Description: Manages story UI and integrates with StoryTiles.
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

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
        // Optional: Persist across scenes if needed
        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (storyPanel != null)
            storyPanel.SetActive(false);

        if (continueButton != null)
            continueButton.onClick.AddListener(CloseStoryUI);
    }

    //Opens the story UI and displays the StoryTile content.
    public void OpenStoryUI(StoryTile tile, GameObject player)
    {
        currentPlayer = player;
        currentStoryTile = tile;

        if (storyPanel == null || storyTextUI == null)
        {
            Debug.LogWarning("[StoryManager] Story UI references missing!");
            return;
        }

        // Set story text and image
        storyTextUI.text = tile.storyText;
        if (storyImageUI != null)
        {
            storyImageUI.sprite = tile.storyImage;
            storyImageUI.gameObject.SetActive(tile.storyImage != null);
        }

        StartCoroutine(ShowUIWithInputDelay());
    }

    private IEnumerator ShowUIWithInputDelay()
    {
        // Show panel
        storyPanel.SetActive(true);

        // Temporarily disable input for one frame to avoid click overlap
        continueButton.interactable = false;
        EventSystem.current.SetSelectedGameObject(null);
        yield return null;
        continueButton.interactable = true;

        Debug.Log("[StoryManager] Story UI shown with one-frame input delay");
    }

    //Closes the story UI and resumes the train.
    public void CloseStoryUI()
    {
        if (storyPanel != null)
            storyPanel.SetActive(false);

        currentPlayer = null;
        currentStoryTile = null;

        // Fire event for TrainFreezeController to resume
        OnStoryClosed?.Invoke();

        Debug.Log("Story UI closed. Event fired to resume train.");
    }
}
