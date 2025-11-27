using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OnCRTToggle : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image crtToggle;
    [SerializeField] private AudioSource checkOnAudio;
    [SerializeField] private AudioSource checkOffAudio;

    [Header("Toggle Animation States")]
    [SerializeField] private Animator crtToggleAnimator;

    private bool isToggled = false;

    private void Start()
    {
        crtToggleAnimator.Play(isToggled ? "RedButtonIdleAnim" : "GreenButtonIdleAnim", 0, 1f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isToggled = !isToggled;

        if (isToggled && checkOnAudio != null)
        {
            checkOffAudio.Play();
        }
        else if (!isToggled && checkOffAudio != null)
        {
            checkOnAudio.Play();
        }

        //Play crt toggle animation based on the state
        crtToggleAnimator.Play(isToggled ? "GreenToRedButtonAnim" : "RedToGreenButtonAnim", 0, 0f);
    }

    private void OnEnable()
    {
        crtToggleAnimator.Play(isToggled ? "RedButtonIdleAnim" : "GreenButtonIdleAnim", 0, 1f);
    }
}

