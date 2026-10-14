using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class OnTrainLight : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DayCycleScript dayCycle;
    [SerializeField] private Light2D trainLight;
    [SerializeField] private GameObject trainOrb;

    [Header("Light/Orb Settings")]
    [SerializeField] private float dayIntensity = 0f;
    [SerializeField] private float nightIntensity = 2f;
    [SerializeField] private float orbDayAlpha = 0f;
    [SerializeField] private float orbNightAlpha = 0.627f;

    [Header("Transition Duration")]
    [SerializeField] private float transitionDuration = 2.5f;
    [SerializeField] private float secondTransitionDuration = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float splitRatio = 0.5f;

    private SpriteRenderer orbRenderer;
    private bool dayToNightTween = false;
    private bool nightToDayTween = false;
    private bool skipInProgress = false;

    private void Start()
    {
        if (dayCycle == null)
            dayCycle = FindFirstObjectByType<DayCycleScript>();

        if (trainOrb != null)
            orbRenderer = trainOrb.GetComponent<SpriteRenderer>();

        // Start the game at day settings
        if (trainLight != null)
            trainLight.intensity = dayIntensity;

        if (orbRenderer != null)
            orbRenderer.color = new Color(1, 1, 1, orbDayAlpha);
    }

    private void Update()
    {
        if (dayCycle == null || trainLight == null || orbRenderer == null)
            return;

        // Determine max tiles based on current phase (matching OnDayToNight logic)
        float maxTiles = (dayCycle.CurrentTime == DayCycleScript.TimeState.Day)
            ? dayCycle.DayLength + dayCycle.DayLengthMod
            : dayCycle.NightLength;

        float tilesMoved = Mathf.Clamp(dayCycle.TilesMoved, 0, maxTiles);
        float progression = maxTiles > 0 ? tilesMoved / maxTiles : 0f;

        //Day to Night transition (Triggers at 65%)
        if (!dayToNightTween && progression >= 0.65f && dayCycle.CurrentTime == DayCycleScript.TimeState.Day)
        {
            dayToNightTween = true;
            nightToDayTween = false;

            //Kill existing tweens
            DOTween.Kill(trainLight.gameObject);
            DOTween.Kill(orbRenderer.gameObject);

            RunTwoStageTransition(nightIntensity, orbNightAlpha);
        }

        //Night to Day transition (Triggers at 50%)
        if (!nightToDayTween && progression >= 0.50f && dayCycle.CurrentTime == DayCycleScript.TimeState.Night)
        {
            nightToDayTween = true;
            dayToNightTween = false;

            //Kill existing tweens
            DOTween.Kill(trainLight.gameObject);
            DOTween.Kill(orbRenderer.gameObject);

            RunTwoStageTransition(dayIntensity, orbDayAlpha);
        }
    }

    private void RunTwoStageTransition(float targetLightIntensity, float targetOrbAlpha)
    {
        //Determine values for two-stage split
        float startLightIntensity = trainLight.intensity;
        float intermediateLightIntensity = Mathf.Lerp(startLightIntensity, targetLightIntensity, splitRatio);

        float startOrbAlpha = orbRenderer.color.a;
        float intermediateOrbAlpha = Mathf.Lerp(startOrbAlpha, targetOrbAlpha, splitRatio);

        //Light Sequence
        Sequence lightSequence = DOTween.Sequence();
        lightSequence.SetTarget(trainLight.gameObject);

        lightSequence.Append(DOTween.To(() => trainLight.intensity, x => trainLight.intensity = x, intermediateLightIntensity, transitionDuration)
            .SetEase(Ease.InSine));
        lightSequence.Append(DOTween.To(() => trainLight.intensity, x => trainLight.intensity = x, targetLightIntensity, secondTransitionDuration)
            .SetEase(Ease.OutSine));

        //Train Orb Sequence
        Sequence orbSequence = DOTween.Sequence();
        orbSequence.SetTarget(orbRenderer.gameObject);

        orbSequence.Append(DOTween.To(() => orbRenderer.color.a, a =>
        {
            var c = orbRenderer.color;
            c.a = a;
            orbRenderer.color = c;
        }, intermediateOrbAlpha, transitionDuration)
        .SetEase(Ease.InSine));

        //Instant 0, no tween
        orbSequence.AppendCallback(() =>
        {
            var c = orbRenderer.color;
            c.a = targetOrbAlpha;
            orbRenderer.color = c;
        });
    }

    public void ForceResetToDay()
    {
        skipInProgress = true;

        DOTween.Kill(trainLight.gameObject);
        DOTween.Kill(orbRenderer.gameObject);

        if (trainLight != null)
            trainLight.intensity = dayIntensity;

        if (orbRenderer != null)
        {
            var c = orbRenderer.color;
            c.a = orbDayAlpha;
            orbRenderer.color = c;
        }

        dayToNightTween = false;
        nightToDayTween = false;

        skipInProgress = false;
    }
}
