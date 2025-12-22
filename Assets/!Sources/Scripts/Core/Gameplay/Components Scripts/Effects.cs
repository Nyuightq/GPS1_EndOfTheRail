// --------------------------------------------------------------
// Creation Date: 2025-11-25 08:01
// Author: User
// Description: -
// --------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#region Weapon data/stats 
public class WeaponStats
{
    private int baseAttackDamage, baseAttackSpeed, baseAttackVariance;
    private Item item;
    public StatsMediator<BuffableWeaponStats> weaponMediator = new StatsMediator<BuffableWeaponStats>();
    public WeaponStats(Item item, int baseAttackDamage, int baseAttackSpeed, int baseAttackVariance)
    {
        this.item = item;
        this.baseAttackDamage = baseAttackDamage;
        this.baseAttackSpeed = baseAttackSpeed;
        this.baseAttackVariance = baseAttackVariance;
    }

    public int AttackDamage
    {
        get
        {
            var q = new Query<BuffableWeaponStats>(BuffableWeaponStats.AttackDamage, baseAttackDamage * item.level);
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

public class CombatComponentData
{
    public string name;
    public int level; // Component can upgrade
    public CombatAnimationClip animationClip;
    public string attackSfxName;
    public StatsMediator<BuffableWeaponStats> weaponMediator;
    public WeaponStats weaponStats;
    public Sprite weaponSprite;
}
#endregion

#region triggers and buffable weapon stats
public enum BuffableWeaponStats
{
    AttackDamage,
    AttackSpeed,
    AttackVariance
}

public enum triggers
{
    OnEquip,
    OnEquipAndAdjacentEquip,
    OnUpdate,
    OnBattleStart,
    OnBattleUpdate,
    OnBattleEnd,
    OnDayStart
}
#endregion

#region effects

// ----- BASE EFFECT -----
[System.Serializable]
public abstract class Effect
{
    [SerializeField] public triggers trigger;
    [SerializeField] public bool triggerOnConditionOnce;
    [SerializeReference] public Conditions[] conditions;
    [SerializeField] public Sprite conditionMetSprite;
    [NonSerialized] private object _owner;

    public Effect(triggers trigger, bool triggerOnConditionOnce, Conditions[] conditions, Sprite conditionMetSprite)
    {
        this.trigger = trigger;
        this.triggerOnConditionOnce = triggerOnConditionOnce;
        this.conditions = CloneConditions(conditions);
        this.conditionMetSprite = conditionMetSprite;
    }

    public object owner
    {
        get => _owner;
        set
        {
            _owner = value;

            // propagate owner down into all conditions
            if (conditions != null)
            {
                foreach (var c in conditions)
                    c.owner = value;
            }
        }
    }

    bool hasTriggered = false;

    public bool checkCondition()
    {
        Debug.Log($"<color=yellow> hasTrigged : {hasTriggered} </color>");
        if (triggerOnConditionOnce && hasTriggered) return false;

        foreach (var condition in conditions)
        {
            if (!condition.check())
            {
                //change the item sprite back to its original sprite
                if (conditionMetSprite != null)
                {
                    Item item = owner as Item;
                    item.changeSprite(item.itemData.itemSprite);
                }
                //Debug.Log("Condition not met");
                return false;
            }
        }
        Debug.Log("<color=green>Condition met</color>");
        hasTriggered = true;

        //change the item into its activated sprite
        if (conditionMetSprite != null)
        {
            Item item = owner as Item;
            item.changeSprite(conditionMetSprite);
        }

        return true;
    }

    protected Conditions[] CloneConditions(Conditions[] oldCondition)
    {
        Conditions[] conditionsCloned = new Conditions[oldCondition.Length];
        for (int i = 0; i < oldCondition.Length; i++)
        {
            if (oldCondition[i] != null)
            {
                conditionsCloned[i] = oldCondition[i].clone();
            }
        }
        return conditionsCloned;
    }

    public void ResetTriggered() { hasTriggered = false; }

    public abstract Effect clone();
    public abstract void apply();

    public abstract void remove();

}
// ----- HEAL EFFECT ----- 
[System.Serializable]
public class HealEffect : Effect
{
    [SerializeField] private InputType inputType;
    [SerializeField] private float healAmount;
    [SerializeField] public bool limitedUses { get; private set; }
    [SerializeField] public int uses { get; private set; }
    [SerializeField] private Sprite usedSprite;

    public HealEffect(triggers trigger, bool triggerOnConditionOnce, Conditions[] conditions, Sprite conditionMetSprite, InputType inputType, float healAmount, bool limitedUses, int uses, Sprite usedSprite) : base(trigger, triggerOnConditionOnce, conditions, conditionMetSprite)
    {
        this.inputType = inputType;
        this.healAmount = healAmount;
        this.limitedUses = limitedUses;
        this.uses = uses;
        this.usedSprite = usedSprite;
    }

    public override Effect clone()
    {
        return new HealEffect(trigger, triggerOnConditionOnce, conditions, conditionMetSprite, inputType, healAmount, limitedUses, uses, usedSprite);
    }

    public override void apply()
    {
        if ((limitedUses && uses <= 0) || !checkCondition()) return;
        CombatSystem combatSystem = UnityEngine.Object.FindFirstObjectByType<CombatSystem>();
        Item item = owner as Item;
        if (item == null) return;

        float finalHealAmount = healAmount * item.level;
        int maxHp;
        int healVal;
        Action<int> Heal;

        //Debug.Log($"[HEAL EFFECT] Owner item = {item.name}, item.level = {item.level}");

        //get the correct hp & healFunction then get the correct heal amount
        if (combatSystem != null && combatSystem.player != null)
        {
            maxHp = combatSystem.player.MaxHp;
            Heal = (amount) => combatSystem.player.HealCurrentHp(amount);
        }
        else
        {
            maxHp = GameManager.instance.playerStatus.MaxHp;
            Heal = (amount) => GameManager.instance.playerStatus.HealCurrentHp(amount);
        }

        switch (inputType)
        {
            case InputType.percentage:
                healVal = Mathf.FloorToInt(maxHp * finalHealAmount / 100);
                break;
            case InputType.flat:
                healVal = Mathf.FloorToInt(finalHealAmount);
                break;
            default:
                healVal = 0;
                break;
        }

        //heal and decrement uses
        Heal(healVal);
        Debug.Log($"<color=green>healing {healVal} hp out of {maxHp}</color>");

        if (limitedUses) uses--;

        if (uses <= 0 && usedSprite != null)
        {
            item.changeSprite(usedSprite);
            Debug.Log("<color=purple>sprite changed</color>");
        }
    }

    public override void remove() { }
} 
// ----- BUFF STAT EFFECT ----- 
[System.Serializable]
public class BuffStatEffect : Effect
{
    [SerializeField] private InputType inputType;
    [SerializeField] private float buffAmount;
    [SerializeField] private BuffableStats statType;
    AdditionModifier<BuffableStats> mod;

    public BuffStatEffect(triggers trigger, bool triggerOnConditionOnce, Conditions[] conditions, Sprite conditionMetSprite, InputType inputType, float buffAmount, BuffableStats statType, AdditionModifier<BuffableStats> mod) : base(trigger, triggerOnConditionOnce, conditions, conditionMetSprite)
    {
        this.inputType = inputType;
        this.buffAmount = buffAmount;
        this.statType = statType;
        this.mod = mod;
    }

    public override Effect clone()
    {
        return new BuffStatEffect(trigger, triggerOnConditionOnce, conditions, conditionMetSprite, inputType, buffAmount, statType, mod);
    }

    public override void apply()
    {
        if (!checkCondition()) return;
        StatsMediator<BuffableStats> mediator = PlayerStatusManager.mediator;
        Item item = owner as Item;
        if (item == null) return;

        float finalBuffAmount = buffAmount * item.level;

        switch (inputType)
        {
            case InputType.percentage:
                mod = new AdditionModifier<BuffableStats>(statType, finalBuffAmount, AdditionModifier<BuffableStats>.AdditionType.percentage);
                mediator.AddModifier(mod);
                break;
            case InputType.flat:
                mod = new AdditionModifier<BuffableStats>(statType, finalBuffAmount, AdditionModifier<BuffableStats>.AdditionType.flat);
                mediator.AddModifier(mod);
                break;
        }
    }

    public override void remove()
    {
        if (mod != null)
        {
            mod.Dispose();
        }
    }
}
// ----- SPAWN WEAPON EFFECT ----- 
[System.Serializable]
public class WeaponSpawnEffect : Effect
{
    [SerializeField] private string weaponName;
    [SerializeField] private int baseAttackDamage;
    [SerializeField] private int baseAttackSpeed;
    [SerializeField] private int baseAttackVariance;
    [SerializeField] private Sprite weaponSprite;
    [SerializeField] private CombatAnimationClip animationClip;
    [SerializeField] private string attackSfxName;

    private WeaponStats _weaponStats;
    public WeaponStats weaponStats
    {
        get
        {
            if (_weaponStats == null && owner is Item item)
            {
                _weaponStats = new WeaponStats(item, baseAttackDamage, baseAttackSpeed, baseAttackVariance);
            }
            return _weaponStats;
        }
        set => _weaponStats = value;
    }

    public WeaponSpawnEffect(triggers trigger, bool triggerOnConditionOnce, Conditions[] conditions, Sprite conditionMetSprite, string weaponName, int baseAttackDamage, int baseAttackSpeed, int baseAttackVariance, Sprite weaponSprite, CombatAnimationClip animationClip, string attackSfxName) : base(trigger, triggerOnConditionOnce, conditions, conditionMetSprite)
    {
        this.weaponName = weaponName;
        this.baseAttackDamage = baseAttackDamage;
        this.baseAttackSpeed = baseAttackSpeed;
        this.baseAttackVariance = baseAttackVariance;
        this.weaponSprite = weaponSprite;
        this.animationClip = animationClip;
        this.attackSfxName = attackSfxName;
    }

    public override Effect clone()
    {
        return new WeaponSpawnEffect(trigger, triggerOnConditionOnce, conditions, conditionMetSprite, weaponName, baseAttackDamage, baseAttackSpeed, baseAttackVariance, weaponSprite, animationClip, attackSfxName);
    }

    public CombatComponentData OnPrepareBattleComponent()
    {
        return new CombatComponentData
        {
            name = weaponName,
            animationClip = animationClip,
            weaponStats = weaponStats,
            attackSfxName = attackSfxName,
            weaponSprite = weaponSprite
            //weaponMediator = mediator
        };
    }

    public override void apply() { }
    public override void remove() { }
}
// ----- BOOST ADJACENT WEAPON EFFECT ----- 
[System.Serializable]
public class AdjacentWeaponBoosterEffect : Effect
{
    [SerializeField] private BuffableWeaponStats statType;
    [SerializeField] private InputType inputType;
    [SerializeField] private int buffAmount;

    // Track which weapons have which modifier applied by this booster
    private Dictionary<Item, AdditionModifier<BuffableWeaponStats>> activeModifiers = new Dictionary<Item, AdditionModifier<BuffableWeaponStats>>();

    public AdjacentWeaponBoosterEffect(triggers trigger, bool triggerOnConditionOnce, Conditions[] conditions, Sprite conditionMetSprite, BuffableWeaponStats statType, InputType inputType, int buffAmount) : base(trigger, triggerOnConditionOnce, conditions, conditionMetSprite)
    {
        this.statType = statType;
        this.inputType = inputType;
        this.buffAmount = buffAmount;
    }

    public override Effect clone()
    {
        return new AdjacentWeaponBoosterEffect(trigger, triggerOnConditionOnce, conditions, conditionMetSprite, statType, inputType, buffAmount);
    }

    public override void apply()
    {
        if (!checkCondition()) return;
        if (owner is not Item booster) return;

        float finalBuffAmount = buffAmount * booster.level;

        // Get all currently adjacent weapon items
        List<GameObject> adjacentList = GameManager.instance.inventoryScript.GetAdjacentComponents(Vector2Int.FloorToInt(booster.GetComponent<ItemDragManager>().topLeftCellPos), booster.itemShape, booster.gameObject);

        List<Item> adjacentItems = new List<Item>();
        foreach (GameObject adjacentObject in adjacentList)
        {
            Item item = adjacentObject.GetComponent<Item>();
            if (item != null && item.effects.OfType<WeaponSpawnEffect>().Any())
            {
                adjacentItems.Add(item);
            }
        }

        // Remove buffs from items that are no longer adjacent
        List<Item> itemsToRemove = activeModifiers.Keys.Where(item => !adjacentItems.Contains(item)).ToList();

        foreach (Item item in itemsToRemove)
        {
            activeModifiers[item].Dispose();
            activeModifiers.Remove(item);
        }

        // Apply buffs to newly adjacent weapons
        foreach (Item weaponItem in adjacentItems)
        {
            //already buffed
            if (activeModifiers.ContainsKey(weaponItem)) continue;

            foreach (WeaponSpawnEffect weapon in weaponItem.effects.OfType<WeaponSpawnEffect>())
            {
                if (weapon.weaponStats == null) continue;

                StatsMediator<BuffableWeaponStats> mediator = weapon.weaponStats.weaponMediator;
                AdditionModifier<BuffableWeaponStats> mod = null;

                switch (inputType)
                {
                    case InputType.percentage:
                        mod = new AdditionModifier<BuffableWeaponStats>(statType, finalBuffAmount, AdditionModifier<BuffableWeaponStats>.AdditionType.percentage);
                        break;
                    case InputType.flat:
                        mod = new AdditionModifier<BuffableWeaponStats>(statType, finalBuffAmount, AdditionModifier<BuffableWeaponStats>.AdditionType.flat);
                        break;
                    default:
                        mod = null;
                        break;
                }

                if (mod != null)
                {
                    mediator.AddModifier(mod);
                    activeModifiers[weaponItem] = mod;
                }
            }

            // Subscribe to the weapon's Unequip to remove the buff if it moves away
            weaponItem.OnUnequip += () =>
            {
                if (activeModifiers.ContainsKey(weaponItem))
                {
                    activeModifiers[weaponItem].Dispose();
                    activeModifiers.Remove(weaponItem);
                }
            };
        }
    }

    public override void remove()
    {
        foreach (var mod in activeModifiers.Values)
        {
            mod.Dispose();
        }
        activeModifiers.Clear();
    }
}
#endregion
