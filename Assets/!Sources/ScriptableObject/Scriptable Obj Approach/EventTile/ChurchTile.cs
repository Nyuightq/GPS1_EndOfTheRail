using UnityEngine;

[CreateAssetMenu(menuName = "Custom Tiles/Church Heal Tile")]
public class ChurchTile : EventTile
{
    [Header("Healing Settings")]
    public int healAmount = 20;

    public override void OnPlayerEnter(GameObject player)
    {
        CrystalHP crystal = FindObjectOfType<CrystalHP>();
        if (crystal != null)
        {
            crystal.Heal(healAmount);
        }
    }
}
