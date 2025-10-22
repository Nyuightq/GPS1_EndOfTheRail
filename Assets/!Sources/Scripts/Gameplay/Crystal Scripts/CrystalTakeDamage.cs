using UnityEngine;

[CreateAssetMenu(menuName = "Custom Tiles/Crystal Damage Tile")]
public class CrystalTakeDamage : EventTile
{
    [Header("Damage Settings")]
    public int damageAmount = 10;

    public override void OnPlayerEnter(GameObject player)
    {
        // Find the crystal in the scene
        CrystalHP crystal = FindFirstObjectByType<CrystalHP>();
        if (crystal != null)
        {
            crystal.TakeDamage(damageAmount);
        }
    }
}
