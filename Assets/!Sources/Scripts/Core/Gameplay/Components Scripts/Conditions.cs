// --------------------------------------------------------------
// Creation Date: 2025-11-11 08:36
// Author: User
// Description: -
// --------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public enum ConditionStatType
{
    scraps,
    currentHp,
    maxHp
}

public enum InputType
{
    percentage,
    flat
}

public enum BuffableWeaponStats
{
    AttackDamage,
    AttackSpeed,
    AttackVariance
}


[System.Serializable] public abstract class Conditions
{
    public float getValue(ConditionStatType statType)
    {
        switch (statType)
        {
            case ConditionStatType.scraps:
                return GameManager.instance.playerStatus.Scraps;
            case ConditionStatType.currentHp:
                return GameManager.instance.playerStatus.Hp;
            case ConditionStatType.maxHp:
                return GameManager.instance.playerStatus.MaxHp;
        }
        return 0;
    }
    public abstract bool check();
}

#region comparison conditions
[System.Serializable] public class LessThanCondition : Conditions
{
    [SerializeField] ConditionStatType valueType;
    [SerializeField] float threshold;
    [Header("Only if threshold is decimal")]
    [SerializeField] ConditionStatType maxValueType;

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
    [SerializeField] ConditionStatType valueType;
    [SerializeField] float threshold;
    [Header("Only if threshold is decimal")]
    [SerializeField] ConditionStatType maxValueType;

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
    [SerializeField] ConditionStatType valueType;
    [SerializeField] float comparedValue;
    [Header("Only if threshold is decimal")]
    [SerializeField] ConditionStatType maxValueType;

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

public enum triggers
{ 
    OnEquip,
    OnUpdate,
    OnBattleStart,
    OnBattleUpdate,
    OnConditionTriggerOnce
}


[System.Serializable] public abstract class Effect
{
    [SerializeField] public triggers trigger;
    [SerializeReference] public Conditions[] conditions;
    [NonSerialized] private object _owner; // hidden from inspector
    public object owner { get => _owner; set => _owner = value; }
    public abstract void apply();

    public abstract void remove();
}

[System.Serializable] public class HealEffect : Effect
{
    [SerializeField] private InputType inputType;
    [SerializeField] private float healAmount;
    [SerializeField] private int uses;
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
        CombatSystem combatSystem = UnityEngine.Object.FindFirstObjectByType<CombatSystem>();
        if (combatSystem != null)
        {
            switch (inputType)
            {
                case InputType.percentage:
                    combatSystem.player.HealCurrentHp(Mathf.FloorToInt(combatSystem.player.MaxHp * healAmount/100));
                    Debug.Log($"healing {Mathf.FloorToInt(combatSystem.player.MaxHp * healAmount/100)} hp out of {combatSystem.player.MaxHp}");
                    break;
                case InputType.flat:
                    combatSystem.player.HealCurrentHp(Mathf.FloorToInt(healAmount));
                    Debug.Log($"healing {healAmount}");
                    break;
            }
        }
    }

    public override void remove() {}
}

[System.Serializable] public class BuffStatEffect : Effect
{
    [SerializeField] private InputType inputType;
    [SerializeField] private float buffAmount;
    [SerializeField] private BuffableStats statType;
    AdditionModifier<BuffableStats> mod;

    public override void apply()
    {
        StatsMediator<BuffableStats> mediator = PlayerStatusManager.mediator;
        switch(inputType)
        {
            case InputType.percentage:
                mod = new AdditionModifier<BuffableStats>(statType, buffAmount, AdditionModifier<BuffableStats>.AdditionType.percentage);
                mediator.AddModifier(mod);                
                break;
            case InputType.flat:
                mod = new AdditionModifier<BuffableStats>(statType, buffAmount, AdditionModifier<BuffableStats>.AdditionType.flat);
                mediator.AddModifier(mod);
                break;
        }
    }

    public override void remove()
    {
        mod.Dispose();
    }
}

public class WeaponStats
{
    private int baseAttackDamage, baseAttackSpeed, baseAttackVariance;
    public StatsMediator<BuffableWeaponStats> weaponMediator = new StatsMediator<BuffableWeaponStats>();
    public WeaponStats(int baseAttackDamage, int baseAttackSpeed, int baseAttackVariance)
    {
        this.baseAttackDamage = baseAttackDamage;
        this.baseAttackSpeed = baseAttackSpeed;
        this.baseAttackVariance = baseAttackVariance;
    }

    public int AttackDamage
    {
        get
        {
            var q = new Query<BuffableWeaponStats>(BuffableWeaponStats.AttackDamage, baseAttackDamage);
            weaponMediator.PerformQuery(this, q);
            return Mathf.FloorToInt(q.value);
        }
    }
    public int AttackSpeed
    {
        get
        {
            var q = new Query<BuffableWeaponStats>(BuffableWeaponStats.AttackSpeed, baseAttackSpeed);
            weaponMediator.PerformQuery(this, q);
            return Mathf.FloorToInt(q.value);
        }
    }
    public int AttackVariance
    {
        get
        {
            var q = new Query<BuffableWeaponStats>(BuffableWeaponStats.AttackVariance, baseAttackVariance);
            weaponMediator.PerformQuery(this, q);
            return Mathf.FloorToInt(q.value);
        }
    }
}


[System.Serializable] public class WeaponSpawnEffect : Effect
{
    [SerializeField] private string weaponName;
    [SerializeField] private int baseAttackDamage;
    [SerializeField] private int baseAttackSpeed;
    [SerializeField] private int baseAttackVariance;
    [SerializeField] private CombatAnimationClip animationClip;
    [NonSerialized] public WeaponStats weaponStats;

    public CombatComponentData OnPrepareBattleComponent()
    {
        return new CombatComponentData
        {
            name = weaponName,
            attackDamage = baseAttackDamage,
            attackSpeed = baseAttackSpeed,
            attackVariance = baseAttackVariance,
            animationClip = animationClip
        };
    }

    public override void apply() {}
    public override void remove() {}
}

[System.Serializable] public class weaponStatBuff
{
    public BuffableWeaponStats statType;
    public InputType inputType;
    public int amount;
}

[System.Serializable] public class AdjacentWeaponBoosterEffect : Effect
{
    [SerializeField] weaponStatBuff[] weaponBuffs;
    public override void apply()
    {
        if (owner is Item item)
        {
            List<GameObject> adjacentList = GameManager.instance.inventoryScript.GetAdjacentComponents(Vector2Int.FloorToInt(item.GetComponent<ItemDragManager>().TopLeftCellPos), item.itemShape, item.gameObject); // updated by zq to fetch topLeftCellPos
            foreach (GameObject adjacent in adjacentList)
            {
                
                if(adjacent.GetComponent<Item>() != null && adjacent.GetComponent<Item>().effects.Any(effect => effect is WeaponSpawnEffect))
                {
                    Debug.Log($"<color=yellow>{item} is adjacent to {adjacent}</color>");
                    //foreach (weaponStatBuff buff in weaponBuffs)
                    //{
                        
                    //}
                }
            }

        }
    }
    public override void remove() {}
}






