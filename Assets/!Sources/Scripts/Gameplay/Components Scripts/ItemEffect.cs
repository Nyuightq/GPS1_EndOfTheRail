// --------------------------------------------------------------
// Creation Date: 2025-10-30 20:52
// Author: nyuig
// Description: ItemEffect is being attached to empty prefab object
//              this prefab object will then included inside ItemSO.
// --------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

public class ItemEffect : MonoBehaviour
{
    protected List<ItemEffect> _relatedComponents;

    public virtual void OnEquip() { }
    public virtual void OnUnequip() { }
    public virtual void OnUpdate() { }
    public virtual void OnAffectComponent() { }
}
