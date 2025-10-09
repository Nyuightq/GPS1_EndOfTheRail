using UnityEngine;
using TMPro;

public class OnButtonContent : MonoBehaviour
{
    [SerializeField] private GameObject CRTShaderVolume;

    [SerializeField] private TextMeshProUGUI[] allTexts;
    [SerializeField] private TMP_FontAsset normalFont;
    [SerializeField] private TMP_FontAsset pixelFont;

    [SerializeField] private GameObject gameTitleN;
    [SerializeField] private GameObject gameTitleP;

    private bool toggleShaderOn = false;
    private bool isPixelFont = false;

    public void Start()
    {
        toggleShaderOn = true;

        gameTitleN.SetActive(true);
        gameTitleP.SetActive(false);

        isPixelFont = false;
    }

    public void onShaderToggle()
    {
        toggleShaderOn = !toggleShaderOn;

        CRTShaderVolume.SetActive(toggleShaderOn);
    }

    public void onNormalFontToggle()
    {
        foreach (TextMeshProUGUI text in allTexts)
        {
            text.font = normalFont;
        }

        isPixelFont = false;
        gameTitleN.SetActive(true);
        gameTitleP.SetActive(false);

        Debug.Log("Is Normal Font");
    }

    public void onPixelFontToggle()
    {
        foreach (TextMeshProUGUI text in allTexts)
        {
            text.font = pixelFont;
        }

        isPixelFont = true;
        gameTitleN.SetActive(false);
        gameTitleP.SetActive(true);

        Debug.Log("Is Pixel Font");
    }
}
