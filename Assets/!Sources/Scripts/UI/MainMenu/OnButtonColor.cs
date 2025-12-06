using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class OnButtonColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI buttonText;

    [SerializeField] private Color normalColor = new Color(251f / 255f, 197f / 255f, 97f / 255f); //Yellowish
    [SerializeField] private Color hoverColor = new Color(255f / 255f, 255f / 255f, 255f / 255f); //White

    private bool audioPlayed = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(buttonText != null)
        {
            buttonText.color = hoverColor;
        }

        if(buttonText != null && !audioPlayed)
        {
            SoundManager.Instance.PlaySFX("SFX_Button_OnHover");
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
