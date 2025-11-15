using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator transitionAnimator;

    [Header("UI References")]
    [SerializeField] private GameObject transitionGO;
    void Start()
    {
        transitionAnimator.enabled = false;
        StartCoroutine(TransitionIn());
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
}
