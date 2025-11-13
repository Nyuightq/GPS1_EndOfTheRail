// --------------------------------------------------------------
// Creation Date: 2025-11-10 11:40
// Author: nyuig
// Description: The class simply append to any event tile panel,
//              Need of using DoTween show and disappear transition.
// --------------------------------------------------------------
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class UI_BaseEventPanel : MonoBehaviour
{
    //[SerializeField] private float slideDuration = 0.4f;
    private static float slideDuration = 0.4f;
    protected RectTransform _panelRect;
    protected Vector2 _panelOriginalPos;
    private CanvasGroup _panelCanvasGroup;

    private void Awake()
    {
        _panelRect = GetComponent<RectTransform>();
        _panelOriginalPos = _panelRect.anchoredPosition;
        _panelCanvasGroup = GetComponent<CanvasGroup>();
    }

    public void ShowEventPanel()
    {
        gameObject.SetActive(true);
        _panelRect.DOKill();
        _panelCanvasGroup.DOKill();

        // Start from off-screen (below)
        _panelRect.anchoredPosition = new Vector2(_panelOriginalPos.x, _panelOriginalPos.y - 40f);
        _panelCanvasGroup.alpha = 0f;

        // Slide up smoothly to its original position
        Sequence seq = DOTween.Sequence();

        seq.Join(_panelRect
            .DOAnchorPosY(_panelOriginalPos.y, slideDuration)
            .SetEase(Ease.OutBack));

        seq.Join(_panelCanvasGroup
            .DOFade(1f, slideDuration * 0.4f)
            .SetEase(Ease.OutQuad));
    }

    public void HideEventPanel()
    {
        _panelRect.DOKill();
        _panelCanvasGroup.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.Join(_panelRect
            .DOAnchorPosY(_panelOriginalPos.y - 300f, slideDuration)
            .SetEase(Ease.InBack));

        seq.Join(_panelCanvasGroup
            .DOFade(0f, slideDuration * 0.4f)
            .SetEase(Ease.InQuad));
        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
}
