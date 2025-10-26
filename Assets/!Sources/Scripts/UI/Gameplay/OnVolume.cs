using UnityEngine;
using UnityEngine.UI;

public class OnVolume : MonoBehaviour
{
    [SerializeField] private AudioSource mainMenuMusic;
    [SerializeField] private Slider volumeSlider;

    private const string VolumePrefKey = "MasterVolume";
    private void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 0.5f);
        volumeSlider.value = savedVolume;

        if (mainMenuMusic != null)
        {
            mainMenuMusic.volume = savedVolume;

            if (!mainMenuMusic.isPlaying)
            {
                mainMenuMusic.Play();
            }
        }

        volumeSlider.onValueChanged.AddListener(UpdateVolume);
    }

    private void UpdateVolume(float volume)
    {
        if (mainMenuMusic != null)
        {
            mainMenuMusic.volume = volume;
        }

        PlayerPrefs.SetFloat(VolumePrefKey, volume);
        PlayerPrefs.Save();
    }
}
