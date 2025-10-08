// --------------------------------------------------------------
// Creation Date: 2025-10-06 21:09
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_CombatTooltipDetail : MonoBehaviour
{
    public static UI_CombatTooltipDetail Instance { get; private set; }
    [SerializeField] private RectTransform tooltipTransform;
    [SerializeField] private TextMeshProUGUI text_entityName;
    [SerializeField] private TextMeshProUGUI text_entityHp;
    [SerializeField] private TextMeshProUGUI text_entityAttackPower;
    [SerializeField] private TextMeshProUGUI text_entityAttackSpeed;
    [SerializeField] private TextMeshProUGUI text_entityDefense;
    [SerializeField] private TextMeshProUGUI text_entityEvasion;
    [SerializeField] private Vector2 offset = new Vector2(20, -20);
    private CombatEntity _entity;

    private Canvas canvas;

    private void Awake()
    {
        Instance = this;
        canvas = GetComponentInParent<Canvas>();
        tooltipTransform = GetComponent<RectTransform>();
        Hide();
    }

    private void Update()
    {
        if (tooltipTransform.gameObject.activeSelf)
        {
            FollowMouse();
            text_entityHp.text = "HP: " + _entity.CurrentHp + " / " + _entity.MaxHp;
        }
    }

    public void Show(CombatEntity entity)
    {
        _entity = entity;
        text_entityName.text = entity.entityName;
        text_entityHp.text = "HP: " + entity.CurrentHp + " / " + entity.MaxHp;
        text_entityAttackPower.text = "PWR: " + entity.AttackDamage;
        text_entityAttackSpeed.text = "SPD: " + entity.AttackSpeed;
        text_entityDefense.text = "DEF: " + entity.Defense;
        // text_entityEvasion.text = "EVA: ";

        gameObject.SetActive(true);
        FollowMouse();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void FollowMouse()
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.worldCamera,
            out pos
        );

        tooltipTransform.localPosition = pos + offset;
    }
}
