using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Header("Combat UI")]
    [SerializeField] private GameObject combatUIPanel; // Assign your UI Panel here

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (combatUIPanel != null)
            combatUIPanel.SetActive(false);
    }

    public void StartCombat(GameObject player)
    {
        Debug.Log("Combat started against enemies!");

        if (combatUIPanel != null)
            combatUIPanel.SetActive(true);

        // Here you can add logic to initialize combat stats, enemies, etc.
    }

    public void EndCombat()
    {
        if (combatUIPanel != null)
            combatUIPanel.SetActive(false);

        Debug.Log("Combat ended.");
    }
}
