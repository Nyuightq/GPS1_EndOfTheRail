using UnityEngine;
using System.Collections.Generic;
public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Header("Combat UI")]
    [SerializeField] private GameObject combatUIPanel; // Assign your UI Panel here

    [Header("Player & Enemy Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform enemySpawnParent;

    // [Header("Enemy Data")]
    // [SerializeField] private List<EnemyData> enemyWaves;

    [Header("System")]
    [SerializeField] private CombatSystem combatSystem;

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

    public void StartCombat()
    {
        if (combatUIPanel != null)
            combatUIPanel.SetActive(true);

        // Generate player
        GameObject playerObj = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity, playerSpawnPoint);
        CombatPlayerEntity playerEntity = playerObj.GetComponent<CombatPlayerEntity>();

        // Generate enemies
        List<CombatEnemyEntity> enemies = new List<CombatEnemyEntity>();
        //foreach (var enemyData in enemyWaves)
        for (int a=0; a<1; a++)
        {
            GameObject enemyObj = Instantiate(enemyPrefab, enemySpawnParent);
            CombatEnemyEntity enemyEntity = enemyObj.GetComponent<CombatEnemyEntity>();
            // enemyEntity.Initialize(enemyData);
            enemies.Add(enemyEntity);
        }

        // Pass enemies data to CombatSystem
        combatSystem.InitializeBattle(playerEntity, enemies);
        combatSystem.onBattleEnd += EndCombat;

        Debug.Log("[CombatManager] Combat setup completed!");
    }

    public void EndCombat(bool playerWon)
    {
        if (combatUIPanel != null)
            combatUIPanel.SetActive(false);

        combatSystem.onBattleEnd -= EndCombat;
        Debug.Log("Combat ended.");
    }

    // Delete it when TrainMovement interruption is done
    public void EndCombat()
    {
        combatSystem.Test_BattleForceCancel();
        if (combatUIPanel != null)
            combatUIPanel.SetActive(false);

        
        Debug.Log("Combat forced to end.");
    }
}
