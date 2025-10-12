using UnityEngine;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Header("Combat UI")]
    [SerializeField] private GameObject combatUIPanel;

    [Header("Player & Enemy Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform enemySpawnParent;

    [Header("System")]
    [SerializeField] private CombatSystem combatSystem;

    // Event for TrainFreezeController
    public static event System.Action OnCombatClosed;

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

    public void StartCombat()
    {
        Debug.Log("Combat started against enemies!");

        if (combatUIPanel != null)
            combatUIPanel.SetActive(true);

        // Generate player
        GameObject playerObj = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity, playerSpawnPoint);
        CombatPlayerEntity playerEntity = playerObj.GetComponent<CombatPlayerEntity>();
        playerEntity.InitialHealth(GameStateManager.Instance.playerStatus.Hp, GameStateManager.Instance.playerStatus.MaxHp);

        // Generate enemies
        List<CombatEnemyEntity> enemies = new List<CombatEnemyEntity>();
        for (int i = 0; i < 1; i++)
        {
            GameObject enemyObj = Instantiate(enemyPrefab, enemySpawnParent);
            CombatEnemyEntity enemyEntity = enemyObj.GetComponent<CombatEnemyEntity>();
            enemies.Add(enemyEntity);
        }

        // Pass enemies data to CombatSystem
        combatSystem.InitializeBattle(playerEntity, enemies);
        combatSystem.onBattleEnd += EndCombat;

        Debug.Log("[CombatManager] Combat setup completed!");
    }

    public void EndCombat(bool playerWon, int remainHp)
    {
        GameStateManager.Instance.playerStatus.UpdateCurrentHp(remainHp);
        if (combatUIPanel != null)
            combatUIPanel.SetActive(false);

        combatSystem.onBattleEnd -= EndCombat;
        Debug.Log("Combat ended.");

        // Notify TrainFreezeController to resume movement
        OnCombatClosed?.Invoke();
    }

    public void EndCombat()
    {
        combatSystem.Test_BattleForceCancel();

        if (combatUIPanel != null)
            combatUIPanel.SetActive(false);

        Debug.Log("Combat forced to end.");

        // Notify TrainFreezeController to resume movement
        OnCombatClosed?.Invoke();
    }
}
