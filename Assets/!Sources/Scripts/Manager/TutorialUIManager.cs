// --------------------------------------------------------------
// Creation Date: 2026-10-10 22:08
// Author: ZQlie
// Description: -
// --------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System; // Required for Action

[System.Serializable]
public struct TutorialUIMapping
{
    public string panelID;
    public GameObject panelObject;
}

public class TutorialUIManager : MonoBehaviour
{
    public static TutorialUIManager Instance { get; private set; }

    public static Action OnTutorialClosed;

    [Header("Registered UI Panels")]
    public List<TutorialUIMapping> uiPanels = new List<TutorialUIMapping>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Opens a specific UI panel by its ID and optionally closes all others.
    /// </summary>
    public void OpenPanel(string targetPanelID)
    {
        bool panelFound = false;

        foreach (var mapping in uiPanels)
        {
            if (mapping.panelID == targetPanelID)
            {
                mapping.panelObject.SetActive(true);
                panelFound = true;
                Debug.Log($"[TutorialUIManager] Opened UI: {targetPanelID}");
            }
            else
            {
                // Optional: Turn off other panels if you only want one open at a time
                mapping.panelObject.SetActive(false);
            }
        }

        if (!panelFound)
        {
            Debug.LogWarning($"[TutorialUIManager] No panel found with ID: {targetPanelID}. Check your inspector mappings.");
        }
    }


    /// <summary>
    /// Closes all managed tutorial panels.
    /// </summary>
    public void CloseAllPanels()
    {
        foreach (var mapping in uiPanels)
        {
            if (mapping.panelObject != null)
            {
                mapping.panelObject.SetActive(false);
            }
        }

        // NEW: Fire the event when panels are closed
        OnTutorialClosed?.Invoke();
    }
}