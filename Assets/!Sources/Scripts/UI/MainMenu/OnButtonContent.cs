using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;

public class OnButtonContent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject CRTShaderVolume;
    [SerializeField] private ScriptableRendererFeature fullScreenPassRendererFeature; //assign the renderer2D fullscreen feature in here

    public void onShaderToggle()
    {
        ApplySavedCRT.Instance.toggleShaderOn = !ApplySavedCRT.Instance.toggleShaderOn;
        bool isOn = ApplySavedCRT.Instance.toggleShaderOn;

        if (CRTShaderVolume != null)
            CRTShaderVolume.SetActive(isOn);

        if (fullScreenPassRendererFeature != null)
            fullScreenPassRendererFeature.SetActive(isOn);
    }
}
