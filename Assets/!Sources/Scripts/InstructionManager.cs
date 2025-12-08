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
    [SerializeField] private string nextSceneName = "GameplayScene";
    [SerializeField] private float transitionDelay = 0.5f;

    private int currentNarrativeIndex = 0;

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
    // Play click audio
    if (clickAudio != null)
        clickAudio.Play();

    // CASE 1: Last panel clicked → Do NOT hide it.
    if (currentNarrativeIndex == narrativePanels.Length - 1)
    {
        // Keep last panel visible exactly like your previous script!

        pixelBallOut.SetActive(true);
        pixelBallOutAnimator.Play("PixelBallOutAnim", 0, 0f);

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

        SceneManager.LoadScene(nextSceneName);
    }
}