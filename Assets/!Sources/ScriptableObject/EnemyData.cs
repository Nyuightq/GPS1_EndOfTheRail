// --------------------------------------------------------------
// Creation Date: 2025-10-09 17:15
// Author: nyuig
// Description: A template data to create new enemy
// --------------------------------------------------------------
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    // public GameObject prefab;
    public int maxHp;
    public int attackDamage;
    public int attackSpeed;
    public int attackVariance;
    public int defense;
    public int evasion;
}