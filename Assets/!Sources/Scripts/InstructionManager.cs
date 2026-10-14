using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InstructionManager : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator pixelBallInAnimator;
    [SerializeField] private Animator pixelBallOutAnimator;

    [Header("Narrative Panels")]
    [SerializeField] private GameObject[] narrativePanels;

    [Header("Navigation Buttons")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;

    [Header("Pixel Ball")]
    [SerializeField] private GameObject pixelBallIn;
    [SerializeField] private GameObject pixelBallOut;

    [Header("Audio")]
    [SerializeField] private AudioSource clickAudio;

    [Header("Scene Settings")]
    [SerializeField] private string[] possibleMapScenes;
    [SerializeField] private float transitionDelay = 0.5f;

    private int currentNarrativeIndex = 0;
    private bool hasClicked = false;

    public void Start()
    {
        // Setup pixel ball in
        pixelBallIn.SetActive(true);
        pixelBallInAnimator.Play("PixelBallInAnim", 0, 0f);
        pixelBallOut.SetActive(false);

        // ALWAYS remove existing listeners before adding to prevent multi-firing
        if (previousButton != null)
        {
            previousButton.onClick.RemoveAllListeners();
            previousButton.onClick.AddListener(OnPreviousButtonClicked);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextButtonClicked);
        }

        currentNarrativeIndex = 0;
        ShowCurrentPanel();
        UpdateButtonStates();
    }

    public void OnPreviousButtonClicked()
    {
        if (hasClicked) return;

        if (clickAudio != null)
            clickAudio.Play();

        if (currentNarrativeIndex > 0)
        {
            currentNarrativeIndex--;
            ShowCurrentPanel();
            UpdateButtonStates();
        }
    }

    public void OnNextButtonClicked()
    {
        Debug.Log("Next Clicked! Current Index BEFORE adding is: " + currentNarrativeIndex);
        if (hasClicked) return; // Prevent clicking while transitioning to next scene

        if (clickAudio != null)
            clickAudio.Play();

        // CASE 1: Last panel clicked → Start game sequence
        if (currentNarrativeIndex >= narrativePanels.Length - 1)
        {
            hasClicked = true;

            pixelBallOut.SetActive(true);
            pixelBallOutAnimator.Play("PixelBallOutAnim", 0, 0f);

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX("SFX_StartNewGame");

            StartCoroutine(TransitionToNextScene());
            return;
        }

        // Move to next
        currentNarrativeIndex++;
        ShowCurrentPanel();
        UpdateButtonStates();
    }

    private void ShowCurrentPanel()
    {
        // Toggle Narrative Panels
        for (int i = 0; i < narrativePanels.Length; i++)
        {
            if (narrativePanels[i] != null)
                narrativePanels[i].SetActive(i == currentNarrativeIndex);
        }

        // CASE 2: First panel special rule
        if (currentNarrativeIndex == 0)
            pixelBallIn.SetActive(true);
        else
            pixelBallIn.SetActive(false);
    }

    private void UpdateButtonStates()
    {
        if (previousButton != null)
            previousButton.interactable = currentNarrativeIndex > 0;

        if (nextButton != null)
        {
            nextButton.interactable = true;

            var buttonText = nextButton.GetComponentInChildren<Text>();
            if (buttonText != null)
                buttonText.text = currentNarrativeIndex >= narrativePanels.Length - 1 ? "Start" : "Next";
        }
    }

    private IEnumerator TransitionToNextScene()
    {
        yield return new WaitForSeconds(transitionDelay);

        if (SoundManager.Instance != null)
        {
            Destroy(SoundManager.Instance.gameObject);
            Debug.Log("[OnClickImage] Destroyed SoundManager");
        }

        // --- RANDOMIZED MAP LOADING LOGIC ---
        if (possibleMapScenes == null || possibleMapScenes.Length == 0)
        {
            Debug.LogError("No maps assigned in the possibleMapScenes array! Please add scene names in the Inspector.");
            yield break;
        }

        int randomIndex = Random.Range(0, possibleMapScenes.Length);
        string selectedSceneName = possibleMapScenes[randomIndex];

        SceneManager.LoadScene(selectedSceneName);
    }

    private void OnDestroy()
    {
        if (previousButton != null)
            previousButton.onClick.RemoveListener(OnPreviousButtonClicked);

        if (nextButton != null)
            nextButton.onClick.RemoveListener(OnNextButtonClicked);
    }
}