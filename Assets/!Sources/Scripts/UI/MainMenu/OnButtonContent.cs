using UnityEngine;
using TMPro;

public class OnButtonContent : MonoBehaviour
{
    [SerializeField] private GameObject CRTShaderVolume;

    private bool toggleShaderOn = false;

    public void Start()
    {
        toggleShaderOn = true;
    }

    public void onShaderToggle()
    {
        toggleShaderOn = !toggleShaderOn;

        CRTShaderVolume.SetActive(toggleShaderOn);
    }
}
