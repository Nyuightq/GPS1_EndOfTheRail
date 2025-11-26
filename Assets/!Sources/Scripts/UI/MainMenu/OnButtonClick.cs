using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class OnButtonClick : MonoBehaviour
{
    [Header("Animators")]
    [SerializeField] private Animator sideSliderAnimator;
    [SerializeField] private Animator transitionAnimator;
    
    [Header("UI References")]
    [SerializeField] private GameObject optionMenuText;
    [SerializeField] private GameObject shaderToggle;
    [SerializeField] private GameObject volumeToggle;
    [SerializeField] private GameObject volumeIcon;
    [SerializeField] private GameObject creditsText;
    [SerializeField] private GameObject creditsNames;
    [SerializeField] private GameObject transitionGO;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip startClip;
    [SerializeField] private AudioClip optionsInClip;
    [SerializeField] private AudioClip optionsOutClip;
    [SerializeField] private AudioClip creditsInClip;
    [SerializeField] private AudioClip creditsOutClip;
    [SerializeField] private AudioClip exitClip;

    [Header("Booleans")]
    private bool isSideSliderOpen = false;
    private bool optionsClicked = false;
    private bool creditsClicked = false;

    public void Start()
    {
        isSideSliderOpen = false;
        optionsClicked = false;
        creditsClicked = false;

        optionMenuText.SetActive(false);
        shaderToggle.SetActive(false);
        volumeToggle.SetActive(false);
        volumeIcon.SetActive(false);
        transitionGO.SetActive(false);

        creditsText.SetActive(false);
        creditsNames.SetActive(false);
    }

    public void OnStartButton()
    {
        SoundManager.Instance.PlaySFX("SFX_StartNewGame");
        Debug.Log($"SFX_StartNewGame");

        if (isSideSliderOpen)
        {
            SoundManager.Instance.PlaySFX("SFX_PanelSlideOut_2");
            Debug.Log($"SFX_PanelSlideOut_2");

            sideSliderAnimator.Play("CloseSideSliderAnim", 0, 0f);

            transitionAnimator.Play("TransitionOutAnim", 0, 0f);
            transitionGO.SetActive(true);

            optionsClicked = false;
            isSideSliderOpen = false;

            StartCoroutine(optionContentsClose());
            StartCoroutine(creditsContentsClose());
            StartCoroutine(WaitThenStart());
        }
        else
        {
            transitionAnimator.Play("TransitionOutAnim", 0, 0f);
            transitionGO.SetActive(true);

            StartCoroutine(WaitThenStart());
        }
    }

    private IEnumerator WaitThenStart()
    {
        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene("CutScene01");
        Debug.Log("Dramatic entrance!");
    }
    
    public void OnOptionsButton()
    {
        if (isSideSliderOpen && optionsClicked == true)
        {
            SoundManager.Instance.PlaySFX("SFX_PanelSlideOut_2");
            Debug.Log($"SFX_PanelSlideOut_2");
            
            sideSliderAnimator.Play("CloseSideSliderAnim", 0, 0f);

            optionsClicked = false;
            isSideSliderOpen = false;

            StartCoroutine(optionContentsClose());
            StartCoroutine(creditsContentsClose());

            return;
        }

        if (!isSideSliderOpen)
        {
            SoundManager.Instance.PlaySFX("SFX_PanelSlideIn");
            Debug.Log($"SFX_PanelSlideIn");
            
            sideSliderAnimator.Play("SideSliderAnim", 0, 0f);

            optionsClicked = true;
            creditsClicked = false;
            isSideSliderOpen = true;

            StartCoroutine(optionContentsOpen());

            return;

        }

        if (isSideSliderOpen && creditsClicked == true)
        {
            sideSliderAnimator.Play("SideSliderIdleAnim", 0, 0f);

            optionsClicked = true;
            creditsClicked = false;

            optionMenuText.SetActive(true);
            shaderToggle.SetActive(true);
            volumeToggle.SetActive(true);
            volumeIcon.SetActive(true);

            creditsText.SetActive(false);
            creditsNames.SetActive(false);

            return;
        }
    }

    public void OnCreditsButton()
    {
        if (isSideSliderOpen && creditsClicked == true)
        {
            sideSliderAnimator.Play("CloseSideSliderAnim", 0, 0f);

            SoundManager.Instance.PlaySFX("SFX_PanelSlideOut_2");
            Debug.Log($"SFX_PanelSlideOut_2");

            creditsClicked = false;
            isSideSliderOpen = false;

            StartCoroutine(creditsContentsClose());
            StartCoroutine(optionContentsClose());

            return;
        }

        if (!isSideSliderOpen)
        {
            SoundManager.Instance.PlaySFX("SFX_PanelSlideIn");
            Debug.Log($"SFX_PanelSlideIn");
            
            sideSliderAnimator.Play("SideSliderAnim", 0, 0f);

            optionsClicked = false;
            creditsClicked = true;
            isSideSliderOpen = true;

            StartCoroutine(creditsContentsOpen());

            return;

        }

        if (isSideSliderOpen && optionsClicked == true)
        {
            sideSliderAnimator.Play("SideSliderIdleAnim", 0, 0f);

            optionsClicked = false;
            creditsClicked = true;

            optionMenuText.SetActive(false);
            shaderToggle.SetActive(false);
            volumeToggle.SetActive(false);
            volumeIcon.SetActive(false);

            creditsText.SetActive(true);
            creditsNames.SetActive(true);

            return;
        }
    }

    public void OnExitButton()
    {        
        SoundManager.Instance.PlaySFX("SFX_Exit_2");
        Debug.Log($"SFX_Exit_2");

        if (isSideSliderOpen)
        {
            SoundManager.Instance.PlaySFX("SFX_PanelSlideOut_2");
            Debug.Log($"SFX_PanelSlideOut_2");

            sideSliderAnimator.Play("CloseSideSliderAnim", 0, 0f);

            optionsClicked = false;
            isSideSliderOpen = false;

            StartCoroutine(optionContentsClose());
            StartCoroutine(creditsContentsClose());
            StartCoroutine(WaitThenExit());
        }
        else
        {
            StartCoroutine(WaitThenExit());
        }
    }

    private IEnumerator WaitThenExit()
    {
        yield return new WaitForSeconds(1.25f);

        Application.Quit();
        Debug.Log("Dramatic exit!");
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxAudioSource != null)
            sfxAudioSource.PlayOneShot(clip);
    }

    private IEnumerator optionContentsOpen()
    {
        yield return new WaitForSeconds(0.35f);

        optionMenuText.SetActive(true);
        shaderToggle.SetActive(true);
        volumeToggle.SetActive(true);
        volumeIcon.SetActive(true);

        Debug.Log("Options content displayed!");
    }

    private IEnumerator optionContentsClose()
    {
        yield return new WaitForSeconds(0.0f);

        optionMenuText.SetActive(false);
        shaderToggle.SetActive(false);
        volumeToggle.SetActive(false);
        volumeIcon.SetActive(false);

        Debug.Log("Options content closed!");
    }

    private IEnumerator creditsContentsOpen()
    {
        yield return new WaitForSeconds(0.35f);

        creditsText.SetActive(true);
        creditsNames.SetActive(true);

        Debug.Log("Credits content displayed!");
    }

    private IEnumerator creditsContentsClose()
    {
        yield return new WaitForSeconds(0.0f);

        creditsText.SetActive(false);
        creditsNames.SetActive(false);

        Debug.Log("Credits content closed!");
    }
}
