using UnityEngine;
using UnityEngine.UI;

public class OnVolumeToggle : MonoBehaviour
{
    [Header("Volume Settings")]
    [SerializeField] private Slider volumeSlider;
    private const string VolumePrefKey = "MasterVolume";

    [Header("Mute Settings")]
    [SerializeField] private Button muteVolumeIcon;
    [SerializeField] private Image muteVolumeImage;
    [SerializeField] private Sprite unMuteSprite;
    [SerializeField] private Sprite muteSprite;
    private const string MutedPrefKey = "IsMuted";

    [Header("Sound Effects")]
    [SerializeField] private AudioSource checkOnAudio;
    [SerializeField] private AudioSource checkOffAudio;

    private bool isMuted = false;
    private float lastVolume = 0f;

    private void Start()
    {
        //Load saved volume
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 0f);
        isMuted = PlayerPrefs.GetInt(MutedPrefKey, 0) == 1;

        lastVolume = savedVolume;

        //Set slider's value based on savedVolume
        volumeSlider.value = savedVolume;

        UpdateMuteStateUI();

        //Get from SoundManager
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMasterVolume(isMuted ? 0f : savedVolume);
        }

        //Listen for changes
        volumeSlider.onValueChanged.AddListener(UpdateVolume);

        //Mute volume toggle
        if (muteVolumeIcon != null)
        {
            muteVolumeIcon.onClick.AddListener(ToggleMute);
        }
    }

    private void UpdateVolume(float volume)
    {
        lastVolume = volume;

        //Update the SoundManager
        if (!isMuted && SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMasterVolume(volume);
        }

        PlayerPrefs.SetFloat(VolumePrefKey, volume);
        PlayerPrefs.Save();
    }

    private void ToggleMute()
    {
        isMuted = !isMuted;

        //Sfx based on toggle stats
        if (isMuted)
        {
            checkOffAudio?.Play();
        }
        else
        {
            checkOnAudio?.Play();
        }

        //Change the sprite based on toggle mute volume
        UpdateMuteStateUI();

        //Update the SoundManager
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMasterVolume(isMuted ? 0f : lastVolume);
        }
    }

    private void UpdateMuteStateUI()
    {
        if (muteVolumeIcon != null)
        {
            muteVolumeImage.sprite = isMuted ? muteSprite : unMuteSprite;
        }
    }
}
