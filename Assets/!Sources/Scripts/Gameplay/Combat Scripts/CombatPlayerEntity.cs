// --------------------------------------------------------------
// Creation Date: 2025-10-02 17:32
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using UnityEngine;

public class CombatPlayerEntity : CombatEntity
{
    private PlayerStatusManager playerStatus;
    protected override void Start()
    {
        playerStatus = GameStateManager.Instance.playerStatus;
    }

    public void InitialHealth(int currentHp, int maxHp)
    {
        _hp = currentHp;
        _maxHp = maxHp;
        // Update UI
        combatEntityUI?.UpdateHealthBar(_hp * 1.0f, _maxHp * 1.0f);
    }
    
    public void HealCurrentHp(int amount)
    {
        _hp += amount;
        _hp = Mathf.Clamp(_hp, 0, _maxHp);
        combatEntityUI?.UpdateHealthBar(_hp * 1.0f, _maxHp * 1.0f);
    }
    
    
    public override void TakeDamage(int dmg)
    {
        base.TakeDamage(dmg);
        playerStatus.UpdateCurrentHp(_hp);

        if (IsDead)
        {
            Debug.Log("🚂 Train destroyed — Player loses battle!");
            //CombatManager.Instance?.OnPlayerDefeated();
        }
    }

    public override void Attack(CombatEntity target)
    {
        base.Attack(target);
        // Maybe implement generate bullet logic.
    }
}
