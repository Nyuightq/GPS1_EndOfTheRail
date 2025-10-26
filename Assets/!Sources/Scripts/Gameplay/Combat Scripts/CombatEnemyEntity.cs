// --------------------------------------------------------------
// Creation Date: 2025-10-02 17:33
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using UnityEngine;

public class CombatEnemyEntity : CombatEntity
{
    public void Initialize(EnemyData data)
    {
        entityName = data.enemyName;
        _maxHp = data.maxHp;
        _hp = data.maxHp;
        _attackDamage = data.attackDamage;
        _attackSpeed = data.attackSpeed;
        _attackDamageVariance = data.attackVariance;
        _defense = data.defense;
        _evasion = data.evasion;
    }

    public override void Attack(CombatEntity target)
    {
        base.Attack(target);
    }
}
