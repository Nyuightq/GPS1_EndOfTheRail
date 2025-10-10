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
    [SerializeField] protected int _maxHp;
    [SerializeField] protected int _hp;
    [SerializeField] protected int _evasion;
    [SerializeField] protected int _defense;
    [SerializeField] protected int _attackSpeed; // 1 speed = 0.6sec
    [SerializeField] protected int _attackDamage;
    [SerializeField] protected int _attackDamageVariance;
    // Extra Properties
    public bool IsDead => _hp <= 0;
    private float _attackTimer = 0.0f;
    private float _attackTakenTime => COMBAT_BASIC_INTERVAL - (COMBAT_BASIC_INTERVAL / COMBAT_MAX_SPEED) * _attackSpeed;
    // Custom event
    public delegate void GameEvent(CombatEntity value);
    public delegate void TakeDamageEvent(CombatEntity value, int damage);
    public event GameEvent OnAttackReady;
    public event GameEvent OnDeath;
    public event TakeDamageEvent OnTakeDamage;

    private bool hasDied = false; // ensures event fires once
    private UI_CombatEntity combatEntityUI;
    // Getters
    public int CurrentHp => _hp;
    public int MaxHp => _maxHp;
    public int AttackDamage => _attackDamage;
    public int AttackSpeed => _attackSpeed;
    public int Evasion => _evasion;
    public int Defense => _defense;
    public float RemainingAttackTimer => _attackTimer;
    public float AttackInterval => _attackTakenTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_hp == 0) _hp = _maxHp;
    }
    void Awake()
    {
        if (_hp == 0) _hp = _maxHp;
        combatEntityUI = GetComponentInChildren<UI_CombatEntity>();
    }

    // This function is called from CombatManager to update attackTimer
    public void UpdateCombat(float deltaTime)
    {
        if (IsDead) return;

        _attackTimer += deltaTime;
        if (_attackTimer >= _attackTakenTime)
        {
            _attackTimer = 0.0f;
            OnAttackReady?.Invoke(this);
        }
        combatEntityUI?.UpdateAttackIntervalBar(_attackTimer *1.0f, _attackTakenTime *1.0f);
    }

    public void TakeDamage(int dmg)
    {
        _hp -= dmg;
        Debug.Log(entityName + " takes " + dmg + " damage. HP: " + _hp);
        OnTakeDamage?.Invoke(this, dmg);
        combatEntityUI?.UpdateHealthBar(_hp*1.0f, _maxHp*1.0f);
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

        int variance = UnityEngine.Random.Range(-_attackDamageVariance, _attackDamageVariance + 1);
        target.TakeDamage(Math.Max(1, _attackDamage + variance));
    }
}
