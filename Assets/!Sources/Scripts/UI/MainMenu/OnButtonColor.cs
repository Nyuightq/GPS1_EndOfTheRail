using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class OnButtonColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private AudioSource hoverAudio;

    [SerializeField] private Color normalColor = new Color(50f / 255f, 50f / 255f, 50f / 255f);
    [SerializeField] private Color hoverColor = new Color(255f / 255f, 255f / 255f, 255f / 255f);

    private bool audioPlayed = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(buttonText != null)
        {
            buttonText.color = hoverColor;
        }

        if(buttonText != null && !audioPlayed)
        {
            hoverAudio.Play();
            audioPlayed = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonText != null)
        {
            buttonText.color = normalColor;
        }

        audioPlayed = false;
    }
}
