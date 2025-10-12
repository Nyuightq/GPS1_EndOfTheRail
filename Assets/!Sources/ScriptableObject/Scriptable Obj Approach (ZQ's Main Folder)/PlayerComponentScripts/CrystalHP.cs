using UnityEngine;

public class CrystalHP : MonoBehaviour
{
    [Header("Crystal Stats")]
    public int maxHP = 100;
    public int currentHP;

    private void Awake()
    {
        currentHP = maxHP; // start at full health
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        Debug.Log($"Crystal HP reduced by {damage}. Current HP: {currentHP}");
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        Debug.Log($"Crystal healed by {amount}. Current HP: {currentHP}");
    }

    public bool IsDestroyed()
    {
        return currentHP <= 0;
    }
}
