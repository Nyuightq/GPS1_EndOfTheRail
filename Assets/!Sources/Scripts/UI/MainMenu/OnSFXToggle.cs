using UnityEngine;
using UnityEngine.UI;

public class OnSFXToggle : MonoBehaviour
{
    [Header("Volume Settings")]
    [SerializeField] private Slider sfxSlider;
    private const string VolumePrefKey = "SFXVolume";

    [Header("Mute Settings")]
    [SerializeField] private Button muteVolumeIcon;
    [SerializeField] private Image muteVolumeImage;
    [SerializeField] private Sprite unMuteSprite;
    [SerializeField] private Sprite muteSprite;
    private const string MutedPrefKey = "IsMuted";

    private bool isMuted = false;
    private float lastVolume = 0f;

    private void Start()
    {
        //Load saved volume
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
        isMuted = PlayerPrefs.GetInt(MutedPrefKey, 0) == 1;

        lastVolume = savedVolume;

        //Set slider's value based on savedVolume
        sfxSlider.value = savedVolume;

        UpdateMuteStateUI();

        //Get from SoundManager
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSFXVolume(isMuted ? 0f : savedVolume);
        }

        //Listen for changes
        sfxSlider.onValueChanged.AddListener(UpdateVolume);

        //Mute sfx toggle
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
            SoundManager.Instance.SetSFXVolume(volume);
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
            SoundManager.Instance.PlaySFX("SFX_CheckOff");
        }
        else
        {
            SoundManager.Instance.PlaySFX("SFX_CheckOn");
        }

        //Change the sprite based on toggle mute volume
        UpdateMuteStateUI();

        //Save the mute state
        PlayerPrefs.SetInt(MutedPrefKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();

        //Update the SoundManager
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSFXVolume(isMuted ? 0f : lastVolume);
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
