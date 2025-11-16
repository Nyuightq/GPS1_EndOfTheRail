using UnityEngine;
using UnityEngine.Rendering.Universal;

public class OnDayToNight : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DayCycleScript dayCycle;
    [SerializeField] private Light2D dayNightLight;

    [Header("Light Settings")]
    [SerializeField] private float dayIntensity = 10f;
    [SerializeField] private float nightIntensity = 0.10f;

    private SpriteRenderer trainPixelOrbRenderer;
    
    private void Start()
    {
        if (dayCycle == null)
        {
            dayCycle = FindObjectOfType<DayCycleScript>();
        }

        if (dayNightLight == null)
        {
            dayNightLight = FindObjectOfType<Light2D>();
        }
    }

    private void Update()
    {
        if (dayCycle == null || dayNightLight == null)
        {
            return;
        }

        float maxTiles = dayCycle.DayLength + dayCycle.DayLengthMod;
        float tilesMoved = Mathf.Clamp(dayCycle.TilesMoved, 0, maxTiles);

        if (dayCycle.CurrentTime == DayCycleScript.TimeState.Day)
        {
            //Day to night
            dayNightLight.intensity = Mathf.Lerp(dayIntensity, nightIntensity, tilesMoved / maxTiles);
        }
        else
        {
            //Night to day
            dayNightLight.intensity = nightIntensity;
        }
    }

    /// <summary>
    /// Forces the lighting back to day intensity. Used when skipping night phase.
    /// </summary>
    public void ForceResetToDay()
    {
        if (dayNightLight != null)
        {
            dayNightLight.intensity = dayIntensity;
            Debug.Log($"[OnDayToNight] Light intensity forced to day value: {dayIntensity}");
        }
    }
}