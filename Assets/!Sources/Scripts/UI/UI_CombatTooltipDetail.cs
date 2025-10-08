// --------------------------------------------------------------
// Creation Date: 2025-10-06 21:09
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance;

    [SerializeField] private RectTransform tooltipTransform;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private Vector2 offset = new Vector2(20, -20);

    private Canvas canvas;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        canvas = GetComponentInParent<Canvas>();
        Hide();
    }

    private void Update()
    {
        if (tooltipTransform.gameObject.activeSelf)
        {
            FollowMouse();
        }
    }

    public void Show(string text)
    {
        tooltipText.text = text;
        tooltipTransform.gameObject.SetActive(true);
        FollowMouse();
    }

    public void Hide()
    {
        tooltipTransform.gameObject.SetActive(false);
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
