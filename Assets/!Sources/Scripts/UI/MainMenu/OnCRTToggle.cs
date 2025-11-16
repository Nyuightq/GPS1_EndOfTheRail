using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OnCRTToggle : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image crtToggle;
    [SerializeField] private AudioSource checkOnAudio;
    [SerializeField] private AudioSource checkOffAudio;

    [SerializeField] private Color normalColor = new Color(255f / 255f, 255f / 255f, 255f / 255f);
    [SerializeField] private Color clickColor = new Color(251f / 255f, 197f / 255f, 97f / 255f);

    private bool isToggled = false;

    private void Start()
    {
        crtToggle.color = normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (crtToggle != null)
        {
            isToggled = !isToggled;

            crtToggle.color = isToggled ? clickColor : normalColor;
        }

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

