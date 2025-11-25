// --------------------------------------------------------------
// Creation Date: 2025-10-30 21:52
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

public class ItemEffect_RepairKit : ItemEffect
{
    [Header("Component Attributes")]
    [SerializeField] private int healAmount = 10;
    
    private PlayerStatusManager _playerStatus;
    private bool _isUsed = false;

    private void Awake()
    {
        _playerStatus = GameStateManager.Instance.playerStatus;
    }
    public override void OnEquip() { }
    public override void OnUnequip() { }
    public override void OnUpdate()
    {
        float hpPercentage = 1.0f * _playerStatus.Hp / _playerStatus.MaxHp;
        if ( hpPercentage <= 0.5f && !_isUsed )
        {
            Debug.Log("ItemEffect_RepairKit: OnUpdate" + hpPercentage + !_isUsed);
            _isUsed = true;
            _playerStatus.HealCurrentHp(healAmount);

            CombatSystem combatSystem = FindFirstObjectByType<CombatSystem>();
            if (combatSystem != null)
                combatSystem.player.HealCurrentHp(healAmount);
        }
    }
    public override void OnAffectComponent() { }
}
