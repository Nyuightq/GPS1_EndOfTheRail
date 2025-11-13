// --------------------------------------------------------------
// Creation Date: 2025-11-11 08:36
// Author: User
// Description: -
// --------------------------------------------------------------
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public enum StatType
{
    scraps,
    currentHp,
    maxHp
}


[System.Serializable] public abstract class Conditions
{
    public float getValue(StatType statType)
    {
        switch (statType)
        {
            case StatType.scraps:
                return GameManager.instance.playerStatus.Scraps;
            case StatType.currentHp:
                return GameManager.instance.playerStatus.Hp;
            case StatType.maxHp:
                return GameManager.instance.playerStatus.MaxHp;
        }
        return 0;
    }
    public abstract bool check();
}

#region comparison conditions
[System.Serializable] public class LessThanCondition : Conditions
{
    [SerializeField] StatType valueType;
    [SerializeField] float threshold;
    [Header("Only if threshold is decimal")]
    [SerializeField] StatType maxValueType;

    public override bool check()
    {
        float value = getValue(valueType);
        float maxValue = getValue(maxValueType);

        if(threshold <= 1)
        {
            return value < Mathf.FloorToInt(maxValue * threshold);
        }
        return value < threshold;
    }
}

[System.Serializable] public class MoreThanCondition : Conditions
{
    [SerializeField] StatType valueType;
    [SerializeField] float threshold;
    [Header("Only if threshold is decimal")]
    [SerializeField] StatType maxValueType;

    public override bool check()
    {
        float value = getValue(valueType);
        float maxValue = getValue(maxValueType);

        if (threshold <= 1)
        {
            return value > Mathf.FloorToInt(maxValue * threshold);
        }
        return value > threshold;
    }
}

[System.Serializable] public class EqualCondition : Conditions
{
    [SerializeField] StatType valueType;
    [SerializeField] float comparedValue;
    [Header("Only if threshold is decimal")]
    [SerializeField] StatType maxValueType;

    public override bool check()
    {
        float value = getValue(valueType);
        float maxValue = getValue(maxValueType);

        if (comparedValue <= 1)
        {
            return value == Mathf.FloorToInt(maxValue * comparedValue);
        }
        return value == comparedValue;
    }
}
#endregion

[System.Serializable] public abstract class Effect
{
    [SerializeReference] public Conditions[] conditions;
    public abstract void apply();
}

[System.Serializable] public class HealEffect : Effect
{
    [SerializeField] float healAmount;
    public override void apply()
    {
        foreach(var condition in conditions)
        {
            if (!condition.check())
            {
                Debug.Log("Condition not met");
                return;
            }
        }
        CombatSystem combatSystem = Object.FindFirstObjectByType<CombatSystem>();
        if (combatSystem != null)
        {
            if (healAmount <= 1)
            {
                combatSystem.player.HealCurrentHp(Mathf.FloorToInt(combatSystem.player.MaxHp * healAmount));
                Debug.Log($"healing {Mathf.FloorToInt(combatSystem.player.MaxHp * healAmount)} hp out of {combatSystem.player.MaxHp}");
            }
            else
            {
                combatSystem.player.HealCurrentHp(Mathf.FloorToInt(healAmount));
                Debug.Log($"healing {healAmount}");
            }
        }
    }
}


