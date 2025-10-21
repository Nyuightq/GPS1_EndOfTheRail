// --------------------------------------------------------------
// Creation Date: 2025-10-16
// Author: -
// Description: Gradually increases enemy stats after every few days.
// --------------------------------------------------------------
using UnityEngine;

public class IncreaseDifficulty : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DayCycleScript dayCycleScript; // Assign your DayCycleScript object here
    [SerializeField] private CombatEntity[] enemyPrefabs;   // Assign enemy prefabs or templates here

    [Header("Difficulty Settings")]
    [Tooltip("How many days before difficulty increases.")]
    [SerializeField] private int daysPerIncrease = 2;

    [Tooltip("Multiplier for enemy HP per difficulty increase (1.1 = +10%)")]
    [SerializeField] private float hpMultiplier = 1.1f;

    [Tooltip("Multiplier for enemy damage per difficulty increase (1.05 = +5%)")]
    [SerializeField] private float damageMultiplier = 1.05f;

    [Tooltip("Optional multiplier for attack speed increase (1.02 = +2%)")]
    [SerializeField] private float speedMultiplier = 1.02f;

    private int lastAppliedDay = 0;

    private void Start()
    {
        if (dayCycleScript == null)
        {
            dayCycleScript = FindObjectOfType<DayCycleScript>();
        }
    }

    private void Update()
    {
        if (dayCycleScript == null) return;

        int currentDay = dayCycleScript.GetDay(); // need a GetDay() method in DayCycleScript

        // Only trigger when enough new days have passed
        if (currentDay >= lastAppliedDay + daysPerIncrease)
        {
            lastAppliedDay = currentDay;
            ApplyDifficultyIncrease();
        }
    }

    private void ApplyDifficultyIncrease()
    {
        Debug.Log($" Difficulty increased on Day {lastAppliedDay}! Enemies grow stronger.");

        foreach (CombatEntity enemy in enemyPrefabs)
        {
            if (enemy == null) continue;

            // Increase stats multiplicatively
            enemy.SetStats(
                Mathf.RoundToInt(enemy.MaxHp * hpMultiplier),
                Mathf.RoundToInt(enemy.AttackDamage * damageMultiplier),
                Mathf.RoundToInt(enemy.AttackSpeed * speedMultiplier)
            );
        }
    }
}
