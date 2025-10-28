using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OnClickImage : MonoBehaviour
{
    [SerializeField] private GameObject narrative1;
    [SerializeField] private GameObject narrative2;
    [SerializeField] private GameObject narrative3;
    [SerializeField] private GameObject narrative4;

    [SerializeField] private GameObject narrative1Text;
    [SerializeField] private GameObject narrative2Text;
    [SerializeField] private GameObject narrative3Text;
    [SerializeField] private GameObject narrative4Text;

    [SerializeField] private AudioSource clickAudio;

    private bool clickedAudio = false;

    public void Start()
    {
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
        narrative4.SetActive(false);
        narrative4Text.SetActive(false);

        clickedAudio = true;
        clickAudio.Play();

        StartCoroutine(WaitThenNext());
    }

    private IEnumerator WaitThenNext()
    {
        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene("SampleScene 1");
    }
}
