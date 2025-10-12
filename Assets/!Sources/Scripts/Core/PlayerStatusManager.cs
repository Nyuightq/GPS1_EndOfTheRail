// --------------------------------------------------------------
// Creation Date: 2025-10-11 16:11
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using UnityEngine;

public class PlayerStatusManager : MonoBehaviour
{
    [SerializeField] private int _hp = 0;
    [SerializeField] private int _maxHp = 0;
    [SerializeField] private int _scraps = 0;
    // public Inventory inventory;

    public int Hp => _hp;
    public int MaxHp => _maxHp;
    public int Scraps => _scraps;

    public void UpdateCurrentHp(int hp)
    {
        _hp = hp;
    }

    public void Start()
    {
        _hp = _maxHp;
    }
    public bool ConsumeScraps(int value)
    {
        if (_scraps >= value)
        {
            _scraps -= value;
            return true;
        }
        return false;
    }
    
    public void RewardScraps(int value)
    {
        _scraps += value;
    }
}
