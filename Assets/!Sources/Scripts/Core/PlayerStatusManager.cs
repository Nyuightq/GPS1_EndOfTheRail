// --------------------------------------------------------------
// Creation Date: 2025-10-11 16:11
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using System;
using UnityEngine;

public class PlayerStatusManager : MonoBehaviour
{
    [SerializeField] private int _scraps = 0;
    [SerializeField] private int _hp = 0;
    [SerializeField] private int _maxHp = 0;
    [SerializeField] private int _crystalHp = 0;
    [SerializeField] private int _maxCrystalHp = 75;
    // public Inventory inventory;

    // Delegate definitions
    public delegate void HpChangedHandler(int currentHp, int maxHp);
    public delegate void ScrapsChangedHandler(int currentScraps);

    // Events based on those delegates
    public event HpChangedHandler OnHpChanged;
    public event HpChangedHandler OnCrystalHpChanged;
    public event ScrapsChangedHandler OnScrapsChanged;

    public int Hp => _hp;
    public int MaxHp => _maxHp;
    public int Scraps => _scraps;
    public int CrystalHp => _crystalHp;
    public int MaxCrystalHp => _maxCrystalHp;

    public void Awake()
    {
        _hp = _maxHp;
        _crystalHp = _maxCrystalHp;
    }

    public void Start()
    {
        OnHpChanged?.Invoke(_hp, _maxHp);
        OnCrystalHpChanged?.Invoke(_crystalHp, _maxCrystalHp);
        OnScrapsChanged?.Invoke(_scraps);
    }

    public void UpdateCurrentHp(int hp)
    {
        _hp = Mathf.Clamp(hp, 0, _maxHp);
        OnHpChanged?.Invoke(_hp, _maxHp);
    }

    public void HealCurrentHp(int amount)
    {
        _hp += amount;
        _hp = Mathf.Clamp(_hp, 0, _maxHp);
        OnHpChanged?.Invoke(_hp, _maxHp);
    }

    public void CrystalTakeDamage(int amount)
    {
        _crystalHp -= amount;
        _crystalHp = Mathf.Clamp(_crystalHp, 0, _maxCrystalHp); // Force value to be >= 0
        OnCrystalHpChanged?.Invoke(_crystalHp, _maxCrystalHp);
    }

    public void HealCrystal(int amount)
    {
        _crystalHp += amount;
        _crystalHp = Mathf.Clamp(_crystalHp, 0, _maxCrystalHp);
        OnCrystalHpChanged?.Invoke(_crystalHp, _maxCrystalHp);
    }

    public bool ConsumeScraps(int value)
    {
        if (value < 0)
        {
            Debug.LogWarning("ConsumeScraps(int value): cannot accept negative value");
            return false;
        }
        
        if (_scraps >= value)
        {
            _scraps -= value;
            OnScrapsChanged?.Invoke(_scraps);
            return true;
        }
        return false;
    }
    
    public void RewardScraps(int value)
    {
        if (value < 0)
        {
            Debug.LogWarning("RewardScraps(int value): cannot accept negative value");
            return;
        }

        _scraps += value;
        OnScrapsChanged?.Invoke(_scraps);
    }
}
