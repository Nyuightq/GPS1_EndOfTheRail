// --------------------------------------------------------------
// Creation Date: 2025-11-13 16:32
// Author: User
// Description: -
// --------------------------------------------------------------
using System.Collections.Generic;
using System;
using UnityEngine;




public class StatsMediator<StatType>
{
    readonly List<StatModifier<StatType>> modifiers = new();

    public event EventHandler<Query<StatType>> Queries;
    public void PerformQuery(object sender, Query<StatType> query) { Queries?.Invoke(sender, query); }

    public void AddModifier(StatModifier<StatType> modifier)
    {
        modifiers.Add(modifier);
        Queries += modifier.Handle;

        modifier.OnDispose += _ =>
        {
            modifiers.Remove(modifier);
            Queries -= modifier.Handle;
        };
    }

    public void Update()
    {
        foreach(StatModifier<StatType> modifier in modifiers)
        {
            if(modifier.markedForDisposal)
            {
                modifier.Dispose();
            }
        }
        
    }
}

public class Query<StatType>
{
    public readonly StatType statType;
    public float value;

    public Query(StatType statType, float value)
    {
        this.statType = statType;
        this.value = value;
    }

}

public abstract class StatModifier<StatType> : IDisposable
{
    public bool markedForDisposal { get; private set; }
    public event Action<StatModifier<StatType>> OnDispose = delegate { };
    public void Dispose()
    {
        markedForDisposal = true;
        OnDispose.Invoke(this);
    }

    public abstract void Handle(object sender, Query<StatType> query);
}

public class AdditionModifier<StatType> : StatModifier<StatType>
{
    public enum AdditionType{flat, percentage}

    readonly StatType statType;
    readonly float amount;
    readonly AdditionType mode;

    
    public AdditionModifier(StatType statType, float amount, AdditionType mode)
    {
        this.statType = statType;
        this.amount = amount;
        this.mode = mode;
    }
    public override void Handle(object sender, Query<StatType> query)
    {
        if (Equals(query.statType, statType))
        {
            switch(mode)
            {
                case AdditionType.flat:
                    query.value += amount;
                    break;
                case AdditionType.percentage:
                    query.value += query.value * amount/100;
                    break;
            }
        }
    }
}