// --------------------------------------------------------------
// Creation Date: 2025-10-30 21:52
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using System;
using UnityEngine;

[System.Serializable] public class baseValue { }
[System.Serializable] public class intValue : baseValue { public int value;}
[System.Serializable] public class floatValue : baseValue { public float value; }
[System.Serializable] public class boolValue : baseValue { public bool value; }


public class ItemEffect_Condition_Comparison : ItemEffect
{
    public enum Conditions
    {
        MoreThan,
        LessThan,
        LessThanOrEqual,
        MoreThanOrEqual,
        Equal
    }

    [Header("Component Attributes")]
    [SerializeField] private Conditions condition = Conditions.Equal;
    [SerializeReference] private baseValue conditionValueA;
    [SerializeReference] private baseValue conditionValueB;
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
        if(CompareCondition(conditionValueA, conditionValueB))
        {
            Debug.Log("Condition met!");
        }

        //float hpPercentage = 1.0f * _playerStatus.Hp / _playerStatus.MaxHp;
        //if ( hpPercentage <= 0.5f && !_isUsed )
        //{
        //    Debug.Log("ItemEffect_RepairKit: OnUpdate" + hpPercentage + !_isUsed);
        //    _isUsed = true;
        //    _playerStatus.HealCurrentHp(healAmount);

        //    CombatSystem combatSystem = FindFirstObjectByType<CombatSystem>();
        //    if (combatSystem != null)
        //        combatSystem.player.HealCurrentHp(healAmount);
        //}
    }
    public override void OnAffectComponent() { }

    private bool CompareCondition(baseValue a, baseValue b)
    {
        if(a == null || b == null) return false;
        if(a is intValue intA && b is intValue intB)
        {
            return CompareConditionType(intA.value, intB.value);
        }
        if(a is floatValue floatA && b is floatValue floatB)
        {
            return CompareConditionType(floatA.value, floatB.value);
        }
        if(a is boolValue boolA && b is boolValue boolB)
        {
            return CompareConditionType(boolA.value, boolB.value);
        }

        return false;
    }

    private bool CompareConditionType<T>(T a, T b) where T : IComparable<T>
    {
        switch (condition)
        {
            case Conditions.MoreThan:
                return a.CompareTo(b) > 0;
                break;
            case Conditions.LessThan:
                return a.CompareTo(b) < 0;
                break;
            case Conditions.Equal:
                return a.CompareTo(b) == 0;
                break;
            case Conditions.MoreThanOrEqual:
                return a.CompareTo(b) > 0 || a.CompareTo(b) == 0;
                break;
            case Conditions.LessThanOrEqual:
                return a.CompareTo(b) < 0 || a.CompareTo(b) == 0;
                break;
        }
        return false;
    }

    private bool CompareCondition(bool a, bool b)
    {
        if(condition == Conditions.Equal)
        {
            if (a == b) return true; else return false;
        }
        return false;
    }
}
