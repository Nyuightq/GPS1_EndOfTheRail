// --------------------------------------------------------------
// Creation Date: 2025-10-26 22:49
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using UnityEngine;

public class CombatComponentEntity : CombatEntity
{
    private WeaponStats _weaponStats;
    protected override void Start()
    {
        base.Start();
        _isComponent = true;
    }
    public void Initialize(CombatComponentData data)
    {
        _weaponStats = data.weaponStats;
        entityName = data.name;
        // _maxHp = data.maxHp;
        // _hp = data.maxHp;
        //_attackDamage = data.weaponStats.AttackDamage;
        //_attackSpeed = data.weaponStats.AttackSpeed;
        //_attackDamageVariance = data.weaponStats.AttackVariance;
        RefreshStats();

        _attackSfxName = data.attackSfxName;
        // _defense = data.defense;
        // _evasion = data.evasion;
    }

    public void RefreshStats()
    {
        _attackDamage = _weaponStats.AttackDamage;
        _attackSpeed = _weaponStats.AttackSpeed;
        _attackDamageVariance = _weaponStats.AttackVariance;

        Debug.Log($"<color=red>attackDamage : {_weaponStats.AttackDamage}</color>");
        Debug.Log($"<color=red>attackSpeed : {_weaponStats.AttackSpeed}</color>");
        Debug.Log($"<color=red>attackDamageVariance : {_weaponStats.AttackVariance}</color>");
    }

    public override void Attack(CombatEntity target)
    {
        base.Attack(target);
    }
}
