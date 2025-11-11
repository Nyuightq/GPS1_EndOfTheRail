// --------------------------------------------------------------
// Creation Date: 2025-11-11 10:24
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ScrapsRewardData
{
    public int baseCount;
    public int variance;

    public int GetRandomScrapsReward()
    {
        int min = Mathf.Max(0, baseCount - variance);
        int max = baseCount + variance + 1; // +1 because Random.Range upper bound is exclusive for ints

        int result = Random.Range(min, max);
        
        return result;
    }
}

[System.Serializable]
public class EnemyEncounterDayData
{
    public int dayNumber;
    public CombatData combatStats;
    public ScrapsRewardData scrapsReward;

    public CombatData CombatStats => combatStats;
    public int GetRandomScrapsReward => scrapsReward.GetRandomScrapsReward();
}

[CreateAssetMenu(fileName = "EnemyProgressionData", menuName = "Scriptable Objects/EnemyProgressionData")]
public class EnemyProgressionData : ScriptableObject
{
    public string entityName;
    public List<EnemyEncounterDayData> data = new();

    public string GetName()
    {
        return entityName;
    }
    
    public CombatData GetCombatStats(int dayNumber)
    {
        if (data.Count == 0) return null;

        // Sort days ascending
        data.Sort((a, b) => a.dayNumber.CompareTo(b.dayNumber));

        // If dayNumber is smaller than smallest day, pick first
        if (dayNumber <= data[0].dayNumber) return data[0].CombatStats;

        // If dayNumber is larger than largest day, pick last
        if (dayNumber >= data[data.Count - 1].dayNumber) return data[data.Count - 1].CombatStats;

        // Otherwise, find the largest day <= dayNumber
        for (int i = data.Count - 1; i >= 0; i--)
        {
            if (data[i].dayNumber <= dayNumber)
                return data[i].CombatStats;
        }

        return null; // fallback (shouldn't reach here)
    }
}
