using DG.Tweening;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }
    [Header("Player & Enemy Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private GameObject defaultComponentPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform enemySpawnParent;
    [Header("System")]
    [SerializeField] private CombatSystem combatSystem;
    [SerializeField] private EnemyNumberEncounterData encounterQuantityData;
    [SerializeField] private DayCycleScript dayCycleSystem;
    public List<CombatComponentEntity> components { get; set;}
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
    }

    public void StartCombat(CombatType combatType = CombatType.Standard)
    {
        // Increment counters based on type
        if (combatType == CombatType.Standard)
            totalCombatsFaced++;
        else
            totalEncountersFaced++;

        SoundManager.Instance.PlaySFX("SFX_Encounter");
        Debug.Log($"SFX_Encounter");

        Debug.Log("Combat started against enemies!");

        combatSystem.ShowEventPanel();
        // ===========================
        // Instantiate combat entities
        // ===========================
        // Generate player
        GameObject playerObj = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity, playerSpawnPoint);
        CombatPlayerEntity playerEntity = playerObj.GetComponent<CombatPlayerEntity>();
        playerEntity.InitialHealth(GameStateManager.Instance.playerStatus.Hp, GameStateManager.Instance.playerStatus.MaxHp);

        ApplyInventoryDefenseBonus(playerEntity);

        // Generate Components
        components = GenerateComponents();

        // Generate enemies
        List<CombatEnemyEntity> enemies = GenerateEnemies();
        // ===========================
        // Instantiate combat entities
        // ===========================

        // Pass enemies data to CombatSystem
        combatSystem.InitializeBattle(playerEntity, enemies, components);
        combatSystem.onBattleEnd += EndCombat;

        Debug.Log($"[CombatManager] Combat setup completed! Type: {combatType}, " +
                $"Total Combats: {totalCombatsFaced}, Total Encounters: {totalEncountersFaced}");
    }

    /// <summary>
    /// Checks inventory and applies defense bonuses from Reinforce Platings
    /// </summary>
    private void ApplyInventoryDefenseBonus(CombatPlayerEntity player)
    {
        int totalDefense = 0;
        int reinforcePlatingCount = 0;

        // Get inventory instance
        if (InventoryItemManager.Instance == null)
        {
            Debug.LogWarning("InventoryItemManager.Instance is null!");
            return;
        }
        
        InventoryGridScript inventory = InventoryItemManager.Instance.GetComponent<InventoryGridScript>();
        
        if (inventory == null)
        {
            Debug.LogWarning("InventoryGridScript not found!");
            return;
        }

        if (inventory.equippedItems == null || inventory.equippedItems.Count == 0)
        {
            Debug.Log(" No items equipped in inventory");
            return;
        }

        // Loop through all equipped items
        foreach (GameObject itemObj in inventory.equippedItems)
        {
            if (itemObj == null) continue;

            Item item = itemObj.GetComponent<Item>();
            if (item == null || item.itemEffect == null) continue;

            // Check if this item is Reinforce Platings
            ItemEffect_ReinforcePlatings reinforcePlating = item.itemEffect as ItemEffect_ReinforcePlatings;
            
            if (reinforcePlating != null)
            {
                reinforcePlatingCount++;
                int bonus = reinforcePlating.GetDefenseBonus();
                totalDefense += bonus;
                Debug.Log($"🛡️ Found Reinforce Platings #{reinforcePlatingCount} → +{bonus} defense");
            }
        }

        // Apply total defense to player
        if (totalDefense > 0)
        {
            player.AddDefense(totalDefense);
            Debug.Log($"Total Defense Applied: {totalDefense} (from {reinforcePlatingCount} Reinforce Plating(s))");
        }
        else
        {
            Debug.Log("No Reinforce Platings equipped - Defense remains at 0");
        }
    }
    private List<CombatComponentEntity> GenerateComponents()
    {
        List<CombatComponentData> componentDatas = InventoryItemManager.Instance.PrepareBattleComponents();
        if (componentDatas != null) Debug.Log("componentDatas count: " + componentDatas.Count);
        List<CombatComponentEntity> components = new List<CombatComponentEntity>();

        if (componentDatas != null)
        {
            Vector3[] componentSpawnPositions = GetComponentSpawnPositionsGrid(componentDatas.Count);
            Debug.Log("GenerateComponents Action: componentsData != null");
            for (int i = 0; i < componentDatas.Count; i++)
            {
                GameObject componentObj = Instantiate(defaultComponentPrefab, playerSpawnPoint);
                componentObj.transform.localPosition = componentSpawnPositions[i];
                CombatComponentEntity componentEntity = componentObj.GetComponent<CombatComponentEntity>();
                componentEntity.Initialize(componentDatas[i]);
                components.Add(componentEntity);
            }
        }
        else
        {
            Debug.Log("GenerateComponents Action: componentsData == null");
            GameObject componentObj = Instantiate(defaultComponentPrefab, playerSpawnPoint);
            CombatComponentEntity componentEntity = componentObj.GetComponent<CombatComponentEntity>();
            components.Add(componentEntity);
        }

        return components;
    }

    private List<CombatEnemyEntity> GenerateEnemies()
    {
        List<CombatEnemyEntity> enemies = new List<CombatEnemyEntity>();
        int dayAmount = dayCycleSystem.GetDay();
        int enemyCount = encounterQuantityData.GetRandomEnemyCount(dayAmount);
        Vector3[] enemySpawnPositions = GetEnemySpawnPositionsCircle(enemyCount, 36f);

        for (int i = 0; i < enemyCount; i++)
        {
            GameObject enemyObj = Instantiate(enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Count)], enemySpawnParent);
            enemyObj.transform.localPosition = enemySpawnPositions[i];
            CombatEnemyEntity enemyEntity = enemyObj.GetComponent<CombatEnemyEntity>();
            enemyEntity.InitializeCombatData(dayAmount);
            enemies.Add(enemyEntity);
        }

        return enemies;
    }

    private Vector3[] GetComponentSpawnPositionsGrid(int count)
    {
        count = Mathf.Clamp(count, 1, 8);

        // int columns = 2;
        int rows = 3;

        float xOffset = 36f;
        float yOffset = 40f;

        Vector3[] positions = new Vector3[count];

        int index = 0;

        if ( count <= 3 )
        {
            float initialYOffset = (count - 1) * (-yOffset * 0.5f);

            for (int r = 0; r < rows && index < count; r++)
            {
                positions[index++] = new Vector3(0f, initialYOffset + r * yOffset, 0f);
            }
        }
        else if (count > 3 && count <= 6)
        {
            // Initial for calculation
            bool isCountOdd = count % 2 == 1;
            int leftCount = Math.Min(count / 2, 3);
            int rightCount = Math.Min(count / 2 + (isCountOdd ? 1 : 0), 3);

            float initialYOffset;
            // Initial for calculation

            // Right side
            initialYOffset = (rightCount - 1) * (-yOffset * 0.5f);
            for (int r = 0; r < rightCount && index < count; r++)
            {
                positions[index++] = new Vector3(xOffset * 0.5f, initialYOffset + r * yOffset, 0f);
            }

            // Left side
            initialYOffset = (leftCount - 1) * (-yOffset * 0.5f);
            for (int r = 0; r < leftCount && index < count; r++)
            {
                positions[index++] = new Vector3(-xOffset * 0.5f, initialYOffset + r * yOffset, 0f);
            }
        }

        if (count > 6)
        {
            positions[index++] = new Vector3(0f, yOffset * 0.5f, 0f);
            positions[index++] = new Vector3(0f, -yOffset * 0.5f, 0f);
        }


        return positions;
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
        // GameStateManager.Instance.playerStatus.UpdateCurrentHp(remainHp);
        combatSystem.HideEventPanel(() => OnCombatClosed?.Invoke());
        combatSystem.onBattleEnd -= EndCombat;
    }

    public void DestroyInstance()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
            Instance = null; // OK because this is inside CombatManager
            Debug.Log("[CombatManager] Instance destroyed for replay.");
        }
    }
}
