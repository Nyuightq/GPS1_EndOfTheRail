using UnityEngine;
using TMPro;

public class OnButtonContent : MonoBehaviour
{
    [SerializeField] private GameObject CRTShaderVolume;

    private bool toggleShaderOn = false;

    public void Start()
    {
        //toggleShaderOn = true;
        CRTShaderVolume.SetActive(ApplySavedCRT.Instance.toggleShaderOn);
    }

    public void onShaderToggle()
    {
        //toggleShaderOn = !toggleShaderOn;

        //CRTShaderVolume.SetActive(toggleShaderOn);

        ApplySavedCRT.Instance.toggleShaderOn = !ApplySavedCRT.Instance.toggleShaderOn;
        CRTShaderVolume.SetActive(ApplySavedCRT.Instance.toggleShaderOn);
    }
}
