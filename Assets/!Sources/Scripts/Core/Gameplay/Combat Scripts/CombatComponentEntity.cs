// --------------------------------------------------------------
// Creation Date: 2025-10-26 22:49
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using UnityEngine;

public class CombatComponentEntity : CombatEntity
{
    // Update is called once per frame
    protected override void Start()
    {
        base.Start();
        _isComponent = true;
    }
    public void Initialize(CombatComponentData data)
    {
        entityName = data.name;
        // _maxHp = data.maxHp;
        // _hp = data.maxHp;
        _attackDamage = data.attackDamage;
        _attackSpeed = data.attackSpeed;
        _attackDamageVariance = data.attackVariance;
        _attackSfxName = data.attackSfxName;
        // _defense = data.defense;
        // _evasion = data.evasion;

        // Assign custom animation clip
        CombatEntityAnimator animationScript = GetComponent<CombatEntityAnimator>();
        if(animationScript != null)
        {
            animationScript.animationClip = data.animationClip;
        }
    }

    public override void Attack(CombatEntity target)
    {
        base.Attack(target);
    }
}
