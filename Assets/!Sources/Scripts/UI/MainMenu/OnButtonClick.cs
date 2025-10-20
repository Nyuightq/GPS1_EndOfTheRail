using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class OnButtonClick : MonoBehaviour
{
    [SerializeField] private Animator sideSliderAnimator;

    [SerializeField] private GameObject optionMenuText;
    [SerializeField] private GameObject shaderToggle;
    [SerializeField] private GameObject volumeToggle;
    [SerializeField] private GameObject volumeIcon;

    [SerializeField] private GameObject creditsText;
    [SerializeField] private GameObject creditsNames;

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

        creditsText.SetActive(false);
        creditsNames.SetActive(false);
    }

    public void OnStartButton()
    {
        SceneManager.LoadScene("CutScene01");
    }
    
    public void OnOptionsButton()
    {
        if (isSideSliderOpen && optionsClicked == true)
        {
            sideSliderAnimator.Play("CloseSideSliderAnim", 0, 0f);

            optionsClicked = false;
            isSideSliderOpen = false;

            StartCoroutine(optionContentsClose());
            StartCoroutine(creditsContentsClose());

            return;
        }

        if (!isSideSliderOpen)
        {
            sideSliderAnimator.Play("SideSliderAnim", 0, 0f);

            optionsClicked = true;
            creditsClicked = false;
            isSideSliderOpen = true;

            StartCoroutine(optionContentsOpen());

            return;

        }

        if (isSideSliderOpen && creditsClicked == true)
        {
            sideSliderAnimator.Play("SideSliderIdle", 0, 0f);

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

            creditsClicked = false;
            isSideSliderOpen = false;

            StartCoroutine(creditsContentsClose());
            StartCoroutine(optionContentsClose());

            return;
        }

        if (!isSideSliderOpen)
        {
            sideSliderAnimator.Play("SideSliderAnim", 0, 0f);

            optionsClicked = false;
            creditsClicked = true;
            isSideSliderOpen = true;

            StartCoroutine(creditsContentsOpen());

            return;

        }

        if (isSideSliderOpen && optionsClicked == true)
        {
            sideSliderAnimator.Play("SideSliderIdle", 0, 0f);

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

    IEnumerator optionContentsOpen()
    {
        yield return new WaitForSeconds(0.35f);

        optionMenuText.SetActive(true);
        shaderToggle.SetActive(true);
        volumeToggle.SetActive(true);
        volumeIcon.SetActive(true);

        Debug.Log("Options content displayed!");
    }

    IEnumerator optionContentsClose()
    {
        yield return new WaitForSeconds(0.0f);

        optionMenuText.SetActive(false);
        shaderToggle.SetActive(false);
        volumeToggle.SetActive(false);
        volumeIcon.SetActive(false);

        Debug.Log("Options content closed!");
    }

    IEnumerator creditsContentsOpen()
    {
        yield return new WaitForSeconds(0.35f);

        creditsText.SetActive(true);
        creditsNames.SetActive(true);

        Debug.Log("Credits content displayed!");
    }

    IEnumerator creditsContentsClose()
    {
        yield return new WaitForSeconds(0.0f);

        creditsText.SetActive(false);
        creditsNames.SetActive(false);

        Debug.Log("Credits content closed!");
    }

    public void OnExitButton()
    {
        Application.Quit();
        Debug.Log("Exit Game Success!");
    }
}
