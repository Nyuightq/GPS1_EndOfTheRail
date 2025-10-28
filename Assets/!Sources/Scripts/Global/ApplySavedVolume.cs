using UnityEngine;

public class ApplySavedVolume : MonoBehaviour
{
    private void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 0.2f);

        AudioSource music = GetComponent<AudioSource>();

        if (music != null)
        {
            music.volume = savedVolume;

            if (!music.isPlaying)
            {
                music.Play();
            }
        }
    }
}
