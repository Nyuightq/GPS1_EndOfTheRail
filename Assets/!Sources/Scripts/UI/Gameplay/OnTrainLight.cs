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
    [SerializeField] private float transitionDuration = 3f;

    private SpriteRenderer orbRenderer;
    private bool trainLightTween = false;
    private bool trainOrbTween = false;

    private DayCycleScript.TimeState lastPhase;

    private void Start()
    {
        if (dayCycle == null)
            dayCycle = FindObjectOfType<DayCycleScript>();

        if (trainOrb != null)
            orbRenderer = trainOrb.GetComponent<SpriteRenderer>();

        // Start the game at day settings
        if (trainLight != null)
            trainLight.intensity = dayIntensity;

        if (orbRenderer != null)
            orbRenderer.color = new Color(1, 1, 1, orbDayAlpha);

        lastPhase = dayCycle.CurrentTime;
    }

    private void Update()
    {
        if (dayCycle == null || trainLight == null || orbRenderer == null)
            return;

        // Total tiles in this phase
        float maxTiles = (dayCycle.CurrentTime == DayCycleScript.TimeState.Day)
            ? dayCycle.DayLength + dayCycle.DayLengthMod
            : dayCycle.NightLength;

        float progression = Mathf.Clamp(dayCycle.TilesMoved / maxTiles, 0f, 1f);

        // Detect phase change
        if (dayCycle.CurrentTime != lastPhase)
        {
            trainLightTween = false;
            trainOrbTween = false;
            lastPhase = dayCycle.CurrentTime;
        }

        // Only start tween at 85%
        if (progression < 0.85f)
            return;

        // ---- LIGHT TWEEN ----
        if (!trainLightTween && !trainOrbTween)
        {
            // Mark both as started
            trainLightTween = true;
            trainOrbTween = true;

            bool isDay = dayCycle.CurrentTime == DayCycleScript.TimeState.Day;

            // Kill existing tweens for both
            DOTween.Kill(trainLight.gameObject);
            DOTween.Kill(orbRenderer.gameObject);

            // --- Determine targets ---
            float targetIntensity = isDay ? nightIntensity : dayIntensity;
            float targetAlpha = isDay ? orbNightAlpha : orbDayAlpha;

            // --- LIGHT Tween ---
            DOTween.To(
                () => trainLight.intensity,
                x => trainLight.intensity = x,
                targetIntensity,
                transitionDuration
            )
            .SetEase(Ease.InOutSine)
            .SetTarget(trainLight.gameObject);

            // --- ORB Tween (your shorter duration for going back to day) ---
            float orbDuration = isDay ? transitionDuration : 0.15f;

            DOTween.To(
                () => orbRenderer.color.a,
                a => {
                    var c = orbRenderer.color;
                    c.a = a;
                    orbRenderer.color = c;
                },
                targetAlpha,
                orbDuration
            )
            .SetEase(isDay ? Ease.InOutSine : Ease.Linear)
            .SetTarget(orbRenderer.gameObject);
        }
    }

    public void ForceResetToDay()
    {
        DOTween.Kill(trainLight.gameObject);
        DOTween.Kill(orbRenderer.gameObject);

        trainLight.intensity = dayIntensity;

        var c = orbRenderer.color;
        c.a = orbDayAlpha;
        orbRenderer.color = c;

        trainLightTween = false;
        trainOrbTween = false;
    }
}
