using System.Collections.Generic;
using UnityEngine;

public class ItemEffect_ReinforcePlatings : ItemEffect
{
    [Header("Defense Settings")]
    [SerializeField] private int defenseBonus = 15;
    
    private CombatPlayerEntity _player;
    private bool _defenseApplied = false;
    
    public override void OnEquip() 
    {
        Debug.Log("Reinforce Platings equipped in inventory!");
    }
    
    public override void OnUnequip() 
    {
        Debug.Log("Reinforce Platings unequipped from inventory!");
        
        // Remove defense if combat is active
        if (_defenseApplied && _player != null)
        {
            _player.RemoveDefense(defenseBonus);
            _defenseApplied = false;
            _player = null;
        }
    }
    
    public override void OnUpdate()
    {

    }

    public override void OnAffectComponent()
    {
        // Not used for this passive defense item
    }

    public int GetDefenseBonus()
    {
        return defenseBonus;
    }
}