using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OnCRT : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image crtToggle;
    [SerializeField] private GameObject crtEffect;
    [SerializeField] private AudioSource checkOnAudio;
    [SerializeField] private AudioSource checkOffAudio;

    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f);
    [SerializeField] private Color clickColor = new Color(201f / 255f, 192f / 255f, 61f / 255f);

    private bool isToggled = false;

    private void Start()
    {
        // Sync toggle state from saved setting
        if (ApplySavedCRT.Instance != null)
        {
            isToggled = ApplySavedCRT.Instance.toggleShaderOn;
        }

        // Update visuals and effect based on saved state
        crtToggle.color = isToggled ? clickColor : normalColor;

        if (crtEffect != null)
        {
            crtEffect.SetActive(isToggled);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (crtToggle == null) return;

        // Toggle state
        isToggled = !isToggled;

        // Update color
        crtToggle.color = isToggled ? clickColor : normalColor;

        // Update CRT effect visibility
        if (crtEffect != null)
        {
            crtEffect.SetActive(isToggled);
        }

        // Save state globally
        if (ApplySavedCRT.Instance != null)
        {
            ApplySavedCRT.Instance.toggleShaderOn = isToggled;
        }

        // Play toggle SFX
        if (isToggled && checkOnAudio != null)
        {
            checkOnAudio.Play();
        }
        else if (!isToggled && checkOffAudio != null)
        {
            checkOffAudio.Play();
        }
    }
}

