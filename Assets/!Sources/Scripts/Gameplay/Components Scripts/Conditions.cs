// --------------------------------------------------------------
// Creation Date: 2025-11-11 08:36
// Author: User
// Description: -
// --------------------------------------------------------------
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public abstract class Conditions
{
    public abstract bool check();
}

public class LessThanCondition : Conditions
{
    [SerializeField] int value;
    [SerializeField] float threshold;
    [SerializeField] int maxValue;
    public override bool check()
    {
        if(threshold <= 1)
        {
            return value < Mathf.FloorToInt(maxValue * threshold);
        }
        return value < threshold;
    }
}

[System.Serializable] public abstract class Effect
{
    [SerializeReference] public Conditions[] conditions;
    public abstract void apply(GameObject user);
}

[System.Serializable] public class HealEffect : Effect
{
    [SerializeField] float healAmount;
    public override void apply(GameObject user)
    {
        foreach(var condition in conditions)
        {
            if (!condition.check()) return;
        }
        CombatSystem combatSystem = Object.FindFirstObjectByType<CombatSystem>();
        if (combatSystem != null)
        {
            if (healAmount <= 1)
            {
                combatSystem.player.HealCurrentHp(Mathf.FloorToInt(combatSystem.player.MaxHp * healAmount));
            }
            else
            {
                combatSystem.player.HealCurrentHp(Mathf.FloorToInt(healAmount));
            }
        }
    }
}


