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
    public static float COMBAT_BASIC_INTERVAL = 2.6f; // 2.6 sec/per attack in speed 1
    public static int COMBAT_MAX_SPEED = 10;
    // Basic Properties define
    public string entityName;
    [SerializeField] protected int _maxHp;
    [SerializeField] protected int _hp;
    [SerializeField] protected int _evasion;
    [SerializeField] protected int _defense;
    [SerializeField] protected int _attackSpeed; // 1 speed = 0.26sec
    [SerializeField] protected int _attackDamage;
    [SerializeField] protected int _attackDamageVariance;
    protected bool _isComponent = false;
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
    protected UI_CombatEntity combatEntityUI;
    // Getters
    public int CurrentHp => _hp;
    public int MaxHp => _maxHp;
    public int AttackDamage => _attackDamage;
    public int AttackSpeed => _attackSpeed;
    public int Evasion => _evasion;
    public int Defense => _defense;
    public float RemainingAttackTimer => _attackTimer;
    public float AttackTakenTime => _attackTakenTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        if (_hp == 0) _hp = _maxHp;
    }
    void Awake()
    {
        if (_hp == 0) _hp = _maxHp;
        combatEntityUI = GetComponentInChildren<UI_CombatEntity>();
    }

    // This function is called from CombatManager to update attackTimer
    public virtual void UpdateCombat(float deltaTime)
    {
        if (IsDead && _isComponent == false) return;

        _attackTimer += deltaTime;
        if (_attackTimer >= _attackTakenTime)
        {
            _attackTimer = 0.0f;
            OnAttackReady?.Invoke(this);
        }
        combatEntityUI?.UpdateAttackIntervalBar(_attackTimer *1.0f, _attackTakenTime *1.0f);
    }

    public virtual void TakeDamage(int dmg)
    {
        int damageTaken = Math.Max(1, dmg - _defense);
        _hp -= damageTaken;
        Debug.Log(entityName + " takes " + damageTaken + " damage. HP: " + _hp);
        OnTakeDamage?.Invoke(this, damageTaken);
        combatEntityUI?.UpdateHealthBar(_hp * 1.0f, _maxHp * 1.0f);
        combatEntityUI?.ShowDamageText(damageTaken);
        if (IsDead == true && hasDied == false)
        {
            hasDied = true;
            OnDeath?.Invoke(this); // Trigger event once to CombatManager
        }
    }
    
    public void updateUI()
    {
        combatEntityUI?.UpdateHealthBar(_hp*1.0f, _maxHp*1.0f);
    }

    // Return true when target is getting killed in this hit.
    public virtual void Attack(CombatEntity target)
    {
        if (target.IsDead) return;

        int variance = UnityEngine.Random.Range(-_attackDamageVariance, _attackDamageVariance + 1);
        target.TakeDamage(Math.Max(1, _attackDamage + variance));
    }

    public void SetStats(int newHp, int newDamage, int newSpeed)
    {
        _maxHp = newHp;
        _hp = Mathf.Min(_hp, _maxHp);
        _attackDamage = newDamage;
        _attackSpeed = newSpeed;

        Debug.Log($"{entityName} stats updated: HP={_maxHp}, DMG={_attackDamage}, SPD={_attackSpeed}");
    }

}
