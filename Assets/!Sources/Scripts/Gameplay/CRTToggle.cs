using UnityEngine;
using System.Collections;

public class CRTToggle : MonoBehaviour
{
    [SerializeField] private GameObject CRTShaderVolume;
    [SerializeField] private GameObject shaderToggle;

    private bool toggleShaderOn = false;

    public void Start()
    {
        toggleShaderOn = true; // Start with shader ON
    }

    public void onShaderToggle()
    {
        toggleShaderOn = !toggleShaderOn; // Flip the state
        CRTShaderVolume.SetActive(toggleShaderOn); // Turn on/off GameObject
    }
}

