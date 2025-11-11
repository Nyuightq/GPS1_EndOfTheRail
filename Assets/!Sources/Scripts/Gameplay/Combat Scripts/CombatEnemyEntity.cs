// --------------------------------------------------------------
// Creation Date: 2025-10-02 17:33
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using UnityEngine;

public class CombatEnemyEntity : CombatEntity
{
    [Header("DataFile")]
    [SerializeField] private EnemyProgressionData _enemyProgressionData;
    private int _rewardScrapsCount = 3;
    public int RewardScrapsCount => _rewardScrapsCount;

    public void InitializeCombatData(int dayAmount = 1)
    {
        if (_enemyProgressionData == null)
        {
            Debug.LogWarning("This enemy has no _enemyProgressionData");
            return;
        }

        entityName = _enemyProgressionData.GetName();
        CombatData data = _enemyProgressionData.GetCombatStats(dayAmount);

        _rewardScrapsCount = _enemyProgressionData.GetRandomScraps(dayAmount);

        _maxHp = data.maxHp;
        _hp = data.maxHp;
        _attackDamage = data.attackDamage;
        _attackSpeed = data.attackSpeed;
        _attackDamageVariance = data.attackVariance;
        _defense = data.defense;
        _evasion = 0;
    }

    public override void Attack(CombatEntity target)
    {
        base.Attack(target);
    }
}
