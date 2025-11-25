// --------------------------------------------------------------
// Creation Date: 2025-11-11 08:36
// Author: User
// Description: -
// --------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
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

#region conditions
[System.Serializable] public abstract class Conditions
{
    public object owner { get; set; }

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
[System.Serializable] public class LessThanEqualCondition : Conditions
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
            return value <= Mathf.FloorToInt(maxValue * threshold);
        }
        return value <= threshold;
    }
}

[System.Serializable] public class MoreThanEqualCondition : Conditions
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
            return value >= Mathf.FloorToInt(maxValue * threshold);
        }
        return value >= threshold;
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

[System.Serializable] public class AdjacentCondition : Conditions
{
    [SerializeField] bool specificAdjacentItem;
    [SerializeField] ItemSO requiredAdjacentItem;

    public override bool check()
    {
        if (owner is Item item)
        {
            List<GameObject> adjacentList = GameManager.instance.inventoryScript.GetAdjacentComponents(Vector2Int.FloorToInt(item.GetComponent<ItemDragManager>().topLeftCellPos), item.itemShape, item.gameObject);
            if (specificAdjacentItem)
            {
                foreach (GameObject adjacent in adjacentList)
                {
                    if (adjacent.GetComponent<Item>().itemData == requiredAdjacentItem) return true;
                }
                return false;
            }
            else
            {
                if (adjacentList.Count == 0) return false; else return true;
            }
        }
        return false;
    }
}
#endregion

