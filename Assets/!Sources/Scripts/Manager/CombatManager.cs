using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }
    private static float slideDuration = 0.5f;

    [Header("Combat UI")]
    [SerializeField] private GameObject combatUIPanel;
    private RectTransform _panelRect;
    private Vector2 _panelOriginalPos;
    private CanvasGroup _panelCanvasGroup;

    [Header("Player & Enemy Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<GameObject> enemyPrefabs;
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
        {
            combatUIPanel.SetActive(false);
            _panelRect = combatUIPanel.GetComponent<RectTransform>();
            _panelOriginalPos = _panelRect.anchoredPosition;
            _panelCanvasGroup = combatUIPanel.GetComponent<CanvasGroup>();   
        }
    }

    public void StartCombat(CombatType combatType = CombatType.Standard)
    {
        // Increment counters based on type
        if (combatType == CombatType.Standard)
            totalCombatsFaced++;
        else
            totalEncountersFaced++;

        Debug.Log("Combat started against enemies!");

        // if (combatUIPanel != null)
        //     combatUIPanel.SetActive(true);
        ShowCombatUI();
        // ===========================
        // Instantiate combat entities
        // ===========================
        // Generate player
        GameObject playerObj = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity, playerSpawnPoint);
        CombatPlayerEntity playerEntity = playerObj.GetComponent<CombatPlayerEntity>();
        playerEntity.InitialHealth(GameStateManager.Instance.playerStatus.Hp, GameStateManager.Instance.playerStatus.MaxHp);

        // Generate Components
        List<CombatComponentEntity> components = GenerateComponents();

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

    private List<CombatComponentEntity> GenerateComponents()
    {
        List<CombatComponentData> componentDatas = InventoryItemManager.Instance.PrepareBattleComponents();
        Debug.Log("Test" + componentDatas);
        List<CombatComponentEntity> components = new List<CombatComponentEntity>();

        if (componentDatas != null)
        {
            Vector3[] componentSpawnPositions = GetComponentSpawnPositionsCircle(componentDatas.Count, 32f);
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
        int enemyCount = Random.Range(2, 5);
        Vector3[] enemySpawnPositions = GetEnemySpawnPositionsCircle(enemyCount, 36f);

        for (int i = 0; i < enemyCount; i++)
        {
            GameObject enemyObj = Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Count)], enemySpawnParent);
            enemyObj.transform.localPosition = enemySpawnPositions[i];
            CombatEnemyEntity enemyEntity = enemyObj.GetComponent<CombatEnemyEntity>();
            enemies.Add(enemyEntity);
        }

        return enemies;
    }

    private Vector3[] GetComponentSpawnPositionsCircle(int count, float radius)
    {
        Vector3[] positions = new Vector3[count];

        float startAngle = 90f;

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle /*+ (360f / count)* i */;
            float rad = angle * Mathf.Deg2Rad;

            float x = Mathf.Cos(rad) * radius;
            float y = Mathf.Sin(rad) * radius;

            positions[i] = new Vector3(x, y, 0f);
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
        GameStateManager.Instance.playerStatus.UpdateCurrentHp(remainHp);
        // if (combatUIPanel != null)
        //     combatUIPanel.SetActive(false);
        HideCombatUI();

        combatSystem.onBattleEnd -= EndCombat;
        Debug.Log("Combat ended.");

        // Notify TrainFreezeController to resume movement
        OnCombatClosed?.Invoke();
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

    // =========================
    // Combat Panel UI DoTween
    // =========================
    public void ShowCombatUI()
    {
        if (combatUIPanel == null || _panelRect == null)
            return;

        combatUIPanel.SetActive(true);
        _panelRect.DOKill();
        _panelCanvasGroup.DOKill();

        // Start from off-screen (below)
        _panelRect.anchoredPosition = new Vector2(_panelOriginalPos.x, _panelOriginalPos.y - 40f);
        _panelCanvasGroup.alpha = 0f;

        // Slide up smoothly to its original position
         Sequence seq = DOTween.Sequence();

        seq.Join(_panelRect
            .DOAnchorPosY(_panelOriginalPos.y, slideDuration)
            .SetEase(Ease.OutBack));

        seq.Join(_panelCanvasGroup
            .DOFade(1f, slideDuration * 0.4f)
            .SetEase(Ease.OutQuad));
    }

    public void HideCombatUI()
    {
        if (combatUIPanel == null || _panelRect == null)
            return;

        _panelRect.DOKill();
        _panelCanvasGroup.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.Join(_panelRect
            .DOAnchorPosY(_panelOriginalPos.y - 300f, slideDuration)
            .SetEase(Ease.InBack));

        seq.Join(_panelCanvasGroup
            .DOFade(0f, slideDuration * 0.4f)
            .SetEase(Ease.InQuad));
        seq.OnComplete(() =>
        {
            combatUIPanel.SetActive(false);
        });
    }
}
