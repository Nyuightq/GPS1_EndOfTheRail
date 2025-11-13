using UnityEngine;
using UnityEngine.Rendering.Universal;

public class OnTrainLight : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DayCycleScript dayCycle;
    [SerializeField] private Light2D trainLight;
    [SerializeField] private GameObject trainOrb;

    [Header("Light / Orb Settings")]
    [SerializeField] private float dayIntensity = 0f; //This is for the day, light is not light
    [SerializeField] private float nightIntensity = 1.2f; //This is for the night, light is bright
    [SerializeField] private float orbDayAlpha = -0.25f; //This is for the day orb, it is transaparent
    [SerializeField] private float orbNightAlpha = 1f; //This is for the night orb, it is full 255 alpha

    private SpriteRenderer orbRenderer;

    private void Start()
    {
        // Auto-assign if missing
        if (dayCycle == null)
            dayCycle = FindObjectOfType<DayCycleScript>();
        if (trainOrb != null)
            orbRenderer = trainOrb.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (dayCycle == null)
            return;

        //Determine max tiles based on the current phase
        float maxTiles = (dayCycle.CurrentTime == DayCycleScript.TimeState.Day)
            ? dayCycle.DayLength + dayCycle.DayLengthMod
            : dayCycle.NightLength;

        //Clmap the tiles moved
        float tilesMoved = Mathf.Clamp(dayCycle.TilesMoved, 0, maxTiles);

        //Normalize the progression
        float t = tilesMoved / maxTiles;

        //Half the day, start the lerp for the light/orb (because I don't want them to start so soon)
        float halfDay = maxTiles / 2;
        float lerping = 0f;

        if (tilesMoved > halfDay)
        {
            lerping = (tilesMoved - halfDay) / halfDay;
        }

        //Train light intensity
        if (trainLight != null)
        {
            //Start the lerp during half and then half again of the day
            if (dayCycle.CurrentTime == DayCycleScript.TimeState.Day)
            {
                trainLight.intensity = Mathf.Lerp(dayIntensity, nightIntensity, lerping / 2);
            }
            else
                trainLight.intensity = nightIntensity;
        }

        //Train orb transparency
        if (orbRenderer != null)
        {
            //Start the lerp during half of the day
            if (dayCycle.CurrentTime == DayCycleScript.TimeState.Day)
                orbRenderer.color = new Color(orbRenderer.color.r, orbRenderer.color.g, orbRenderer.color.b,
                                              Mathf.Lerp(orbDayAlpha, orbNightAlpha, lerping));
            else
                orbRenderer.color = new Color(orbRenderer.color.r, orbRenderer.color.g, orbRenderer.color.b,
                                              orbDayAlpha);
        }
    }
}

