using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OnClickImage : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator pixelBallInAnimator;
    [SerializeField] private Animator pixelBallOutAnimator;

    [Header("UI References")]
    [SerializeField] private GameObject narrative1;
    [SerializeField] private GameObject narrative2;
    [SerializeField] private GameObject narrative3;
    [SerializeField] private GameObject narrative4;

    [SerializeField] private GameObject narrative1Text;
    [SerializeField] private GameObject narrative2Text;
    [SerializeField] private GameObject narrative3Text;
    [SerializeField] private GameObject narrative4Text;

    [SerializeField] private GameObject pixelBallIn;
    [SerializeField] private GameObject pixelBallOut;

    [SerializeField] private GameObject blackBarNar3;

    public void Start()
    {
        pixelBallIn.SetActive(true);
        pixelBallInAnimator.Play("PixelBallInAnim", 0, 0f);

        pixelBallOut.SetActive(false);

        narrative1.SetActive(true);
        narrative1Text.SetActive(true);

        narrative2.SetActive(false);
        narrative3.SetActive(false);
        blackBarNar3.SetActive(false);
        narrative4.SetActive(false);

        narrative2Text.SetActive(false);
        narrative3Text.SetActive(false);
        narrative4Text.SetActive(false);

    }

    public void OnNarrative1()
    {
        pixelBallIn.SetActive(false);
        
        narrative2.SetActive(true);
        narrative2Text.SetActive(true);

        narrative1.SetActive(false);
        narrative1Text.SetActive(false);

        SoundManager.Instance.PlaySFX("SFX_PageFlip");
    }

    public void OnNarrative2()
    {
        narrative3.SetActive(true);
        narrative3Text.SetActive(true);
        blackBarNar3.SetActive(true);

        narrative2.SetActive(false);
        narrative2Text.SetActive(false);

        SoundManager.Instance.PlaySFX("SFX_PageFlip");
    }

    public void OnNarrative3()
    {
        narrative4.SetActive(true);
        narrative4Text.SetActive(true);

        narrative3.SetActive(false);
        narrative3Text.SetActive(false);
        blackBarNar3.SetActive(false);

        SoundManager.Instance.PlaySFX("SFX_PageFlip");
    }

    public void OnNarrative4()
    {
        pixelBallOut.SetActive(true);
        pixelBallOutAnimator.Play("PixelBallOutAnim", 0, 0f);

        narrative4.SetActive(true);
        narrative4Text.SetActive(true);

        narrative1.SetActive(false);
        narrative1Text.SetActive(false);

        SoundManager.Instance.PlaySFX("SFX_StartNewGame");

        StartCoroutine(TransitionOutToNextScene());
    }

    private IEnumerator TransitionOutToNextScene()
    {
        yield return new WaitForSeconds(0.5f);

        if (SoundManager.Instance != null)
        {
            Destroy(SoundManager.Instance.gameObject);
            Debug.Log("[WinLoseManager] Destroyed SoundManager");
        }

        SceneManager.LoadScene("GameplayLevel");
    }
}
