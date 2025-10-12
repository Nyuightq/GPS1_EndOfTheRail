// --------------------------------------------------------------
// Creation Date: 2025-10-02 17:32
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using UnityEngine;

public class CombatPlayerEntity : CombatEntity
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitialHealth(int currentHp, int maxHp)
    {
        _hp = currentHp;
        _maxHp = maxHp;
        // Update UI
        combatEntityUI?.UpdateHealthBar(_hp*1.0f, _maxHp*1.0f);
    }

    public override void Attack(CombatEntity target)
    {
        base.Attack(target);
        // Maybe implement generate bullet logic.
    }
}
