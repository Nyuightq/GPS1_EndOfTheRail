using UnityEngine;

public class ApplySavedCRT : MonoBehaviour
{
    public static ApplySavedCRT Instance;
    public bool toggleShaderOn = true;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
