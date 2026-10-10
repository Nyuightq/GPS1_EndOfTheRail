using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class InstructionManager : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator pixelBallInAnimator;
    [SerializeField] private Animator pixelBallOutAnimator;

    [Header("Narrative Panels")]
    [SerializeField] private GameObject[] narrativePanels;

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

        // Hide all narrative panels except the first one
        for (int i = 0; i < narrativePanels.Length; i++)
        {
            narrativePanels[i].SetActive(i == 0);
        }

        currentNarrativeIndex = 0;
    }

public void OnInstructionClick()
{
     if (hasClicked) return; //Prevent double click

     // Play click audio
     if (clickAudio != null)
     clickAudio.Play();

     // CASE 1: Last panel clicked → Do NOT hide it.
    if (currentNarrativeIndex == narrativePanels.Length - 1)
    {
        if (hasClicked) return; //Prevent double click
        hasClicked = true;

        // Play click audio
        if (clickAudio != null)
        clickAudio.Play();

        // Keep last panel visible exactly like your previous script!

        pixelBallOut.SetActive(true);
        pixelBallOutAnimator.Play("PixelBallOutAnim", 0, 0f);

        SoundManager.Instance.PlaySFX("SFX_StartNewGame");

        StartCoroutine(TransitionToNextScene());
        return;
    }

    // CASE 2: First panel special rule
    if (currentNarrativeIndex == 0)
        pixelBallIn.SetActive(false);

    // Hide current panel
    narrativePanels[currentNarrativeIndex].SetActive(false);

    // Move to next
    currentNarrativeIndex++;

    // Show next panel
    narrativePanels[currentNarrativeIndex].SetActive(true);
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

        // Safety check to prevent errors if the array is empty in the Inspector
        if (possibleMapScenes == null || possibleMapScenes.Length == 0)
        {
            Debug.LogError("No maps assigned in the possibleMapScenes array! Please add scene names in the Inspector.");
            yield break;
        }

        // Generate a random number between 0 and the array's length
        int randomIndex = Random.Range(0, possibleMapScenes.Length);

        // Retrieve the randomly selected scene name
        string selectedSceneName = possibleMapScenes[randomIndex];

        // Load the chosen scene
        SceneManager.LoadScene(selectedSceneName);
    }
}