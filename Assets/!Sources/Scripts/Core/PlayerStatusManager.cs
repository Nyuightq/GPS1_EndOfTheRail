// --------------------------------------------------------------
// Creation Date: 2025-10-11 16:11
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using System;
using UnityEngine;

public class PlayerStatusManager : MonoBehaviour
{
    [SerializeField] private int _hp = 0;
    [SerializeField] private int _maxHp = 0;
    [SerializeField] private int _scraps = 0;
    // public Inventory inventory;

    // Delegate definitions
    public delegate void HpChangedHandler(int currentHp, int maxHp);
    public delegate void ScrapsChangedHandler(int currentScraps);

    // Events based on those delegates
    public event HpChangedHandler OnHpChanged;
    public event ScrapsChangedHandler OnScrapsChanged;

    public int Hp => _hp;
    public int MaxHp => _maxHp;
    public int Scraps => _scraps;

    public void Start()
    {
        _hp = _maxHp;

        // Initial
        OnHpChanged?.Invoke(_hp, _maxHp);
        OnScrapsChanged?.Invoke(_scraps);
    }

    public void UpdateCurrentHp(int hp)
    {
        _hp = hp;
        OnHpChanged?.Invoke(_hp, _maxHp);
    }

    public void HealCurrentHp(int amount)
    {
        _hp = Math.Min(_hp + amount, _maxHp);
        OnHpChanged?.Invoke(_hp, _maxHp);
    }

    public bool ConsumeScraps(int value)
    {
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
        _scraps += value;
        OnScrapsChanged?.Invoke(_scraps);
    }
}
