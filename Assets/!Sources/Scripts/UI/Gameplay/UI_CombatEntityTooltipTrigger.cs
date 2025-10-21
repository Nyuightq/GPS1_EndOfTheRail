// --------------------------------------------------------------
// Creation Date: 2025-10-08 20:05
// Author: nyuig
// Description: high dependencies with CombatEntity
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.EventSystems;
public class UI_CombatEntityTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private CombatEntity entity;
    private UI_CombatTooltipDetail tooltip;

    private void Awake()
    {
        entity = GetComponent<CombatEntity>();
        tooltip = UI_CombatTooltipDetail.Instance;
    }

    private void Start()
    {
        tooltip = UI_CombatTooltipDetail.Instance;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip.Show(entity);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.Hide();
    }
}
