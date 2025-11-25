// --------------------------------------------------------------
// Creation Date: 2025-10-30 17:03
// Author: nyuig
// Description: Attach to InventoryManager which included InventoryGridScript.cs
// --------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InventoryGridScript))]
public class InventoryItemManager : MonoBehaviour
{
    public static InventoryItemManager Instance;
    private InventoryGridScript _inventory;

    void Start()
    {
        Instance = this;
        _inventory = GetComponent<InventoryGridScript>();
    }

    void Update()
    {
        // Remove destroyed or null items first
        for (int i = _inventory.equippedItems.Count - 1; i >= 0; i--)
        {
            GameObject itemObj = _inventory.equippedItems[i];
            if (itemObj == null)   // destroyed Unity object
            {
                _inventory.equippedItems.RemoveAt(i);
                continue;
            }

            Item item = itemObj.GetComponent<Item>();
            if (item == null)      // component missing or destroyed
            {
                _inventory.equippedItems.RemoveAt(i);
                continue;
            }

            if (item.itemEffect != null)
                item.itemEffect.OnUpdate();
        }
    }

    // This function is called by CombatManager.cs when intialize combat components.
    // List of CombatComponentEntity should be change return a set of data. (Not scriptable object)
    public List<CombatComponentData> PrepareBattleComponents()
    {
        Debug.Log("InventoryItemManager.PrepareBattleComponents()" + _inventory.equippedItems.Count);
        List<CombatComponentData> componentList = new List<CombatComponentData>();

        foreach (GameObject item in _inventory.equippedItems)
        {
            if (item == null) continue;

            Item thisItem = item.GetComponent<Item>();
            foreach(Effect effect in thisItem.effects)
            {
                Debug.Log("Effect type: " + (effect != null ? effect.GetType().Name : "null"));
                if (effect is WeaponSpawnEffect combatComp)
                {
                    Debug.Log("found a weapon!");
                    componentList.Add(combatComp.OnPrepareBattleComponent());
                }
            }
        }

        if (componentList.Count > 0) return componentList;
        else return null;
    }
}
