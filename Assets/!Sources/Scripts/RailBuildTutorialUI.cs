// --------------------------------------------------------------
// Creation Date: 2026-10-14 15:48
// Author: ZQlie
// Description: -
// --------------------------------------------------------------
using UnityEngine;

public class RailBuildTutorialUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Assign the panel you want to pop up here.")]
    [SerializeField] private GameObject popUpPanel;

    private void OnEnable()
    {
        // Subscribe to the static action that fires when the tutorial crystal finishes
        TutorialCrystalManager.OnRewardClosed += ShowPanel;
    }

    private void OnDisable()
    {
        // Always unsubscribe to prevent memory leaks
        TutorialCrystalManager.OnRewardClosed -= ShowPanel;
    }

    private void Start()
    {
        // Ensure the panel starts closed
        if (popUpPanel != null)
        {
            popUpPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Triggered automatically when TutorialCrystalManager finishes and closes its reward UI.
    /// </summary>
    private void ShowPanel()
    {
        if (popUpPanel != null)
        {
            popUpPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Call this method from your UI Button's OnClick() event in the Unity Inspector.
    /// </summary>
    public void OnButtonClicked()
    {
        if (popUpPanel != null)
        {
            popUpPanel.SetActive(false);
        }
    }
}
