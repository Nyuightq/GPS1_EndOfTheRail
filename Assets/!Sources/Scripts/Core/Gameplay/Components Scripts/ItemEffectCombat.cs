// --------------------------------------------------------------
// Creation Date: 2025-10-30 20:52
// Author: nyuig
// Description: Used to instantiate battle component when triggered battle.
// --------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

// --------------------------------------------------------------
// Data container for CombatComponentEntity initialization
// --------------------------------------------------------------
public class CombatComponentData
{
    public string name;
    public int level; // Component can upgrade
    public int attackDamage;
    public int attackSpeed;
    public int attackVariance;
    public CombatAnimationClip animationClip;
}

// --------------------------------------------------------------
// ItemEffectCombat - holds data used to create combat components
// --------------------------------------------------------------
public class ItemEffectCombat : ItemEffect
{
    [Header("Combat Component Stats")]
    [SerializeField] private string componentName;
    [SerializeField] private int level; // Component can upgrade
    [SerializeField] private int attackDamage;
    [SerializeField] private int attackSpeed;
    [SerializeField] private int attackVariance;
    [SerializeField] private CombatAnimationClip animationClip;

    public CombatComponentData OnPrepareBattleComponent()
    {
        return new CombatComponentData {
            name = componentName,
            attackDamage = attackDamage,
            attackSpeed = attackSpeed,
            attackVariance = attackVariance,
            animationClip = animationClip
        };
    }
}
