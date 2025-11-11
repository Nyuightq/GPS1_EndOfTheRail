// --------------------------------------------------------------
// Creation Date: 2025-11-10 20:04
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemyEncounterWeight
{
    [Range(1, 4)] public int enemyCount;
    public int weight;
}

[System.Serializable]
public class EnemyEncounterWeightList
{
    public int dayNumber;
    public List<EnemyEncounterWeight> encounterWeights = new();

    public int TotalWeight()
    {
        int total = 0;
        foreach(EnemyEncounterWeight e in encounterWeights)
        {
            total += e.weight;
        }

        return total;
    }
}


[CreateAssetMenu(fileName = "EnemyNumberEncounterData", menuName = "Scriptable Objects/EnemyNumberEncounterData")]
public class EnemyNumberEncounterData : ScriptableObject
{
    public List<EnemyEncounterWeightList> data = new();

    public int GetRandomEnemyCount(int dayNumber)
    {
        // Find the day data
        EnemyEncounterWeightList dayData = GetClosestDayData(dayNumber);
        if (dayData == null || dayData.encounterWeights.Count == 0)
        {
            Debug.LogWarning($"No encounter data for day {dayNumber}");
            return 2; // fallback
        }

        // Calculate total weight
        int totalWeight = dayData.TotalWeight();
        int randomValue = Random.Range(0, totalWeight);

        // Determine which enemy count is selected
        int runningSum = 0;
        foreach (EnemyEncounterWeight ew in dayData.encounterWeights)
        {
            runningSum += ew.weight;
            if (randomValue < runningSum)
                return ew.enemyCount;
        }

        // fallback
        return 2;
    }
    
    private EnemyEncounterWeightList GetClosestDayData(int dayNumber)
    {
        if (data.Count == 0) return null;

        // Sort days ascending
        data.Sort((a, b) => a.dayNumber.CompareTo(b.dayNumber));

        // If dayNumber is smaller than smallest day, pick first
        if (dayNumber <= data[0].dayNumber) return data[0];

        // If dayNumber is larger than largest day, pick last
        if (dayNumber >= data[data.Count - 1].dayNumber) return data[data.Count - 1];

        // Otherwise, find the largest day <= dayNumber
        for (int i = data.Count - 1; i >= 0; i--)
        {
            if (data[i].dayNumber <= dayNumber)
                return data[i];
        }

        return null; // fallback (shouldn't reach here)
    }
}
