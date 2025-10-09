// --------------------------------------------------------------
// Creation Date: 2025-10-02 17:32
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using UnityEngine;

public class CombatPlayerEntity : CombatEntity
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void Attack(CombatEntity target)
    {
        base.Attack(target);
        // Maybe implement generate bullet logic.
    }
}
