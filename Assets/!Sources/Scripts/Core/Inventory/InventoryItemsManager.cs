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
        // do OnUpdate() to each item.
        foreach (GameObject item in _inventory.equippedItems)
        {
            Item thisItem = item.GetComponent<Item>();
            if(thisItem.itemEffect != null)
            thisItem.itemEffect.OnUpdate();
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
