// --------------------------------------------------------------
// Creation Date: 2025-10-02 17:25
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CombatEntity : MonoBehaviour
{
    public static float COMBAT_BASIC_INTERVAL = 6.0f; // 6 sec/per attack in speed 1
    public static int COMBAT_MAX_SPEED = 10;
    // Basic Properties define
    public string entityName;
    [SerializeField] private int maxHp;
    [SerializeField] private int hp;
    [SerializeField] private int evasion;
    [SerializeField] private int defense;
    public int attackSpeed; // 1 speed = 0.6sec
    public int attackDamage;
    public int attackDamageVariance;
    // Extra Properties
    public bool IsDead => hp <= 0;
    private float attackTimer = 0.0f;
    private float attackTakenTime => COMBAT_BASIC_INTERVAL - (COMBAT_BASIC_INTERVAL / COMBAT_MAX_SPEED) * attackSpeed;
    // Custom event
    public delegate void GameEvent(CombatEntity value);
    public delegate void TakeDamageEvent(CombatEntity value, int damage);
    public event GameEvent OnAttackReady;
    public event GameEvent OnDeath;
    public event TakeDamageEvent OnTakeDamage;

    private bool hasDied = false; // ensures event fires once
    [SerializeField] UI_CombatEntity combatEntityUI;
    // Getters
    public int CurrentHp => hp;
    public float RemainingAttackTimer => attackTimer;
    public float AttackInterval => attackTakenTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hp = maxHp;
    }
    void Awake()
    {
        combatEntityUI = GetComponentInChildren<UI_CombatEntity>();
    }

    // This function is called from CombatManager to update attackTimer
    public void UpdateCombat(float deltaTime)
    {
        if (IsDead) return;

        attackTimer += deltaTime;
        if (attackTimer >= attackTakenTime)
        {
            attackTimer = 0.0f;
            OnAttackReady?.Invoke(this);
        }
        combatEntityUI?.UpdateAttackIntervalBar(attackTimer *1.0f, attackTakenTime *1.0f);
    }

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        Debug.Log(entityName + " takes " + dmg + " damage. HP: " + hp);
        OnTakeDamage?.Invoke(this, dmg);
        combatEntityUI?.UpdateHealthBar(hp*1.0f, maxHp*1.0f);
        combatEntityUI?.ShowDamageText(dmg);
        if (IsDead == true && hasDied == false)
        {
            hasDied = true;
            OnDeath?.Invoke(this); // Trigger event once to CombatManager
        }
    }

    // Return true when target is getting killed in this hit.
    public virtual void Attack(CombatEntity target)
    {
        if (target.IsDead) return;

        int variance = UnityEngine.Random.Range(-attackDamageVariance, attackDamageVariance + 1);
        target.TakeDamage(Math.Max(1, attackDamage + variance));
    }
}
