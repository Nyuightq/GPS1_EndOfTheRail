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

    // Event for train freeze controller
    public static event System.Action OnStoryClosed;

    [Header("UI References")]
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private TMP_Text storyTextUI;
    [SerializeField] private Image storyImageUI;
    [SerializeField] private Button continueButton;

    private bool isStoryActive = false; // Prevent re-entry until UI closes fully
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

    public bool IsStoryActive => isStoryActive;

    public void OpenStoryUI(StoryTile tile, GameObject player)
    {
        if (isStoryActive)
        {
            Debug.Log("[StoryManager] Story already active, ignoring new request.");
            return;
        }

        currentPlayer = player;
        currentStoryTile = tile;
        isStoryActive = true;

        if (storyPanel == null || storyTextUI == null)
        {
            Debug.LogWarning("[StoryManager] Missing UI references!");
            return;
        }

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
        storyPanel.SetActive(true);

        // Disable button input for one frame to prevent double click issues
        continueButton.interactable = false;
        EventSystem.current.SetSelectedGameObject(null);
        yield return null;
        continueButton.interactable = true;

        Debug.Log("[StoryManager] Story UI opened with one-frame delay");
    }

    public void CloseStoryUI()
    {
        if (!isStoryActive)
            return;

        storyPanel.SetActive(false);
        currentPlayer = null;
        currentStoryTile = null;

        Debug.Log("Story UI closed. Event fired to resume train.");
        OnStoryClosed?.Invoke();

        // Small delay before reallowing triggers
        StartCoroutine(ResetStoryActiveAfterDelay(0.25f));
    }

    private IEnumerator ResetStoryActiveAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isStoryActive = false;
        Debug.Log("[StoryManager] Ready for next story trigger.");
    }

    public void DestroyInstance()
{
    if (Instance != null)
    {
        Destroy(Instance.gameObject);
        Instance = null; // OK because this is inside StoryManager
        Debug.Log("[StoryManager] Instance destroyed for replay.");
    }
}


    
}
