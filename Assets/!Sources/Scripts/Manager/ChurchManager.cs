// --------------------------------------------------------------
// Creation Date: 2025-10-09 18:55
// Author: ZQlie
// Description: -
// --------------------------------------------------------------
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ChurchManager : MonoBehaviour
{
    public static ChurchManager Instance { get; private set; }

    public static event System.Action OnChurchClosed;

    [Header("UI References")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private Button yesButton;

    [Header("TMP References")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text yesButtonText;

    [Header("Healing Settings")]
    [SerializeField] private int healAmount = 20;

    private GameObject currentPlayer;

    public bool IsChurchUIActive { get; private set; } = false;
    public bool IsCooldownActive { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (uiPanel != null)
            uiPanel.SetActive(false);
    }

    public void OpenChurchUI(GameObject player)
    {
        if (IsChurchUIActive || IsCooldownActive)
        {
            Debug.Log("[ChurchManager] Ignored OpenChurchUI request (UI busy or cooldown active)");
            return;
        }

        currentPlayer = player;
        if (uiPanel == null) return;

        uiPanel.SetActive(true);
        IsChurchUIActive = true;

        yesButton.onClick.RemoveAllListeners();

        if (messageText != null)
        {
            messageText.text = "As the train halts beside the ancient chapel, the faint hum of energy fills the air. " +
                               "You sense the Crystal’s pulse weaken... yet the altar glows warmly, offering restoration.";
        }

        PlayerStatusManager playerStatus = GameStateManager.Instance.playerStatus;

        if (playerStatus != null && yesButtonText != null)
        {
            int currentHp = playerStatus.CrystalHp;
            int maxHp = playerStatus.MaxCrystalHp;
            int healedHp = Mathf.Min(currentHp + healAmount, maxHp);
            yesButtonText.text = $"{currentHp}→{healedHp} HP";
        }
        else if (yesButtonText != null)
        {
            yesButtonText.text = $"Heal Crystal (+{healAmount} HP)";
        }

        yesButton.onClick.AddListener(() =>
        {
            PlayerStatusManager playerStatus = GameStateManager.Instance.playerStatus;
            if (playerStatus != null)
                playerStatus.HealCrystal(healAmount);

            Debug.Log("Crystal healed by " + healAmount);
            CloseChurchUI();
        });

        Debug.Log("[ChurchManager] Church UI opened");
    }

    public void CloseChurchUI()
    {
        if (!IsChurchUIActive)
            return;

        if (uiPanel != null)
            uiPanel.SetActive(false);

        currentPlayer = null;
        IsChurchUIActive = false;

        Debug.Log("Church UI closed. Event fired.");
        OnChurchClosed?.Invoke();

        // Begin cooldown before another tile can trigger
        StartCoroutine(CloseCooldown());
    }

    private IEnumerator CloseCooldown()
    {
        IsCooldownActive = true;
        yield return new WaitForSeconds(0.5f);
        IsCooldownActive = false;
        Debug.Log("[ChurchManager] Cooldown finished, ready for next trigger.");
    }

    public void DestroyInstance()
{
    if (Instance != null)
    {
        Destroy(Instance.gameObject);
        Instance = null; // OK because this is inside ChurchManager
        Debug.Log("[ChurchManager] Instance destroyed for replay.");
    }
}

}
