using UnityEngine;
using System.Collections.Generic;
using System;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Header("Combat UI")]
    [SerializeField] private GameObject combatUIPanel;

    [Header("Player & Enemy Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject defaultComponentPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform enemySpawnParent;

    [Header("System")]
    [SerializeField] private CombatSystem combatSystem;

    [Header("Combat Stats")]
    public int totalCombatsFaced = 0;
    public int totalEncountersFaced = 0;
    // Event for TrainFreezeController
    public static event System.Action OnCombatClosed;

        public enum CombatType
        {
            Standard,
            Encounter
        }


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

    public void StartCombat(CombatType combatType = CombatType.Standard)
    {
        // Increment counters based on type
        if (combatType == CombatType.Standard)
            totalCombatsFaced++;
        else
            totalEncountersFaced++;

        Debug.Log("Combat started against enemies!");

        if (combatUIPanel != null)
            combatUIPanel.SetActive(true);

        // Generate player
        GameObject playerObj = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity, playerSpawnPoint);
        CombatPlayerEntity playerEntity = playerObj.GetComponent<CombatPlayerEntity>();
        playerEntity.InitialHealth(GameStateManager.Instance.playerStatus.Hp, GameStateManager.Instance.playerStatus.MaxHp);

        // Generate Components
        List<CombatComponentEntity> components = new List<CombatComponentEntity>();
        int componentsCount = 1;// UnityEngine.Random.Range(1, 2);
        for (int i = 0; i< componentsCount; i++)
        {
            GameObject componentObj = Instantiate(defaultComponentPrefab, playerSpawnPoint);
            // componentObj.transform.localPosition = spawnPositions[i];
            CombatComponentEntity componentEntity = componentObj.GetComponent<CombatComponentEntity>();
            components.Add(componentEntity);
        }

        // Generate enemies
        List<CombatEnemyEntity> enemies = new List<CombatEnemyEntity>();
        int enemyCount = UnityEngine.Random.Range(2, 5); 
        Vector3[] enemySpawnPositions = GetEnemySpawnPositionsCircle(enemyCount, 24f);

        for (int i = 0; i < enemyCount; i++)
        {
            GameObject enemyObj = Instantiate(enemyPrefab, enemySpawnParent);
            enemyObj.transform.localPosition = enemySpawnPositions[i];
            CombatEnemyEntity enemyEntity = enemyObj.GetComponent<CombatEnemyEntity>();
            enemies.Add(enemyEntity);
        }

        // Pass enemies data to CombatSystem
        combatSystem.InitializeBattle(playerEntity, enemies, components);
        combatSystem.onBattleEnd += EndCombat;

        Debug.Log("[CombatManager] Combat setup completed!");
        Debug.Log($"[CombatManager] Combat setup completed! Type: {combatType}, " +
                $"Total Combats: {totalCombatsFaced}, Total Encounters: {totalEncountersFaced}");
    }

    // Used in StartCombat, auto initialize position for enemies.
    private Vector3[] GetEnemySpawnPositionsCircle(int count, float radius)
    {
        Vector3[] positions = new Vector3[count];

        float startAngle = 70f;

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + (350f / count) * i;
            float rad = angle * Mathf.Deg2Rad;

            float x = Mathf.Cos(rad) * radius;
            float y = Mathf.Sin(rad) * radius;

            positions[i] = new Vector3(x, y, 0f);
        }

        return positions;
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
        combatSystem.onBattleEnd -= EndCombat;
        combatSystem.Test_BattleForceCancel();

        if (combatUIPanel != null)
            combatUIPanel.SetActive(false);

        Debug.Log("Combat forced to end.");

        // Notify TrainFreezeController to resume movement
        OnCombatClosed?.Invoke();
    }
}
