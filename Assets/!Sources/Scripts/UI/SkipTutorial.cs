// --------------------------------------------------------------
// Creation Date: 2026-10-12 00:42
// Author: ZQlie
// Description: -
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class SkipTutorial : MonoBehaviour
{
    [Header("HUD Skip Button")]
    [SerializeField] private Button skipHUDButton;

    [Header("Confirmation Panel")]
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button declineButton;
    [SerializeField] private GameObject confirmationPanel;

    [Header("Win/Lose Manager Reference")]
    [SerializeField] private WinLoseManager winLoseManager;


    private void Start()
    {
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);

        if (skipHUDButton != null)
            skipHUDButton.onClick.AddListener(OnSkipTutorialHUDButtonClicked);

        if (acceptButton != null)
            acceptButton.onClick.AddListener(OnAcceptButtonClicked);

        if (declineButton != null)
            declineButton.onClick.AddListener(OnDeclineButtonClicked);

    }

    public void OnAcceptButtonClicked()
    {
        Debug.Log("Accept Btn Click");
        confirmationPanel.SetActive(false);
        if (winLoseManager != null)
        {
            winLoseManager.LoadRandomMap();
        }
    }

    public void OnDeclineButtonClicked()
    {
        confirmationPanel.SetActive(false);
        Debug.Log("Decline Btn Click");
    }

    public void OnSkipTutorialHUDButtonClicked()
    {
        Debug.Log("HUD Btn Click");
        confirmationPanel.SetActive(true);

    }

    private void OnDestroy()
    {

        if (acceptButton != null)
            acceptButton.onClick.RemoveListener(OnAcceptButtonClicked);

        if (declineButton != null)
            declineButton.onClick.RemoveListener(OnDeclineButtonClicked);
    }
}

