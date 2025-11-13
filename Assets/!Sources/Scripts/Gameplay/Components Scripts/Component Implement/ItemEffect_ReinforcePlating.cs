using UnityEngine;

public class ItemEffect_ReinforcePlatings : ItemEffect
{
    [Header("Defense Settings")]
    [SerializeField] private int baseDefenseBonus = 15;
    [SerializeField] private int itemLevel = 1;
    [SerializeField] private int maxLevel = 3;

    private CombatPlayerEntity _player;
    private bool _defenseApplied = false;

    public override void OnEquip()
    {
        Debug.Log($"Reinforce Platings (Lv.{itemLevel}) equipped!");
    }

    public override void OnUnequip()
    {
        Debug.Log($"Reinforce Platings (Lv.{itemLevel}) unequipped!");
        if (_defenseApplied && _player != null)
        {
            _player.RemoveDefense(GetDefenseBonus());
            _defenseApplied = false;
            _player = null;
        }
    }

    public override void OnUpdate() { }

    public override void OnAffectComponent() { }

    public int GetDefenseBonus()
    {
        return baseDefenseBonus * itemLevel;
    }

    // Upgrade (?)
    public bool TryMerge(ItemEffect_ReinforcePlatings other)
    {
        if (other == null || other.GetType() != this.GetType())
            return false;

        if (itemLevel >= maxLevel)
        {
            Debug.Log("Item already at max level!");
            return false;
        }

        // Merge successful
        itemLevel++;
        Debug.Log($"Merged Reinforce Platings! Now Level {itemLevel}, Bonus {GetDefenseBonus()}");
        return true;
    }
}
