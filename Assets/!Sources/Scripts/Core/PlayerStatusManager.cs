// --------------------------------------------------------------
// Creation Date: 2025-10-11 16:11
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using UnityEngine;

public class PlayerStatusManager : MonoBehaviour
{
    private int _hp;
    private int _maxHp;
    private int _scraps;
    // public Inventory inventory;

    public int Hp { get; set; }
    public int MaxHp => _maxHp;
    public int Scraps => _scraps;


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
