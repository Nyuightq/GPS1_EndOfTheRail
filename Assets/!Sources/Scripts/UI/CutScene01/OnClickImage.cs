using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OnClickImage : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator transitionAnimator;
    
    [Header("UI References")]
    [SerializeField] private GameObject narrative1;
    [SerializeField] private GameObject narrative2;
    [SerializeField] private GameObject narrative3;
    [SerializeField] private GameObject narrative4;

    [SerializeField] private GameObject narrative1Text;
    [SerializeField] private GameObject narrative2Text;
    [SerializeField] private GameObject narrative3Text;
    [SerializeField] private GameObject narrative4Text;

    [SerializeField] private GameObject transitionGO;

    [Header("Audio")]
    [SerializeField] private AudioSource clickAudio;

    private bool clickedAudio = false;

    public void Start()
    {
        transitionAnimator.enabled = false;
        StartCoroutine(TransitionIn());

        narrative1.SetActive(true);
        narrative1Text.SetActive(true);

        narrative2.SetActive(false);
        narrative3.SetActive(false);
        narrative4.SetActive(false);

        narrative2Text.SetActive(false);
        narrative3Text.SetActive(false);
        narrative4Text.SetActive(false);

        clickedAudio = false;
    }

    private IEnumerator TransitionIn()
    {
        transitionGO.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        transitionAnimator.enabled = true;
        transitionAnimator.Play("TransitionInAnim", 0, 0f);

        yield return new WaitForSeconds(0.5f);
        transitionGO.SetActive(false);
    }

    public void OnNarrative1()
    {
        narrative2.SetActive(true);
        narrative2Text.SetActive(true);

        narrative1.SetActive(false);
        narrative1Text.SetActive(false);

        clickedAudio = true;
        clickAudio.Play();
    }

    public void OnNarrative2()
    {
        narrative3.SetActive(true);
        narrative3Text.SetActive(true);

        narrative2.SetActive(false);
        narrative2Text.SetActive(false);

        clickedAudio = true;
        clickAudio.Play();
    }

    public void OnNarrative3()
    {
        narrative4.SetActive(true);
        narrative4Text.SetActive(true);

        narrative3.SetActive(false);
        narrative3Text.SetActive(false);

        clickedAudio = true;
        clickAudio.Play();
    }

    public void OnNarrative4()
    {
        clickedAudio = true;
        clickAudio.Play();

        StartCoroutine(TransitionOut());
        StartCoroutine(WaitThenNext());
    }

    private IEnumerator TransitionOut()
    {
        transitionGO.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        transitionAnimator.enabled = true;
        transitionAnimator.Play("TransitionOutAnim", 0, 0f);
    }

    private IEnumerator WaitThenNext()
    {
        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene("GameplayLevel");
    }
}
