How to add sound effects/ musics in sound manager?
1. Go to Sound Manager.
2. Create a dictionary based on your audio. (Is it SFX? / Is it Music?)
3. Add the name of the audio. [Case Sensitive, Used by your script]
4. Put your audio clip in the drop down list accordingly.
5. Adjust Settings.

---- Code Side----
6. Go to your script that you wanted to add the audio.
7. Select with audio type you would like to add. (See below Usage Examples or in bottom of SoundManager.cs)
8. Add Line that plays audio.
9. Done.



#region Usage Examples
// Play sound effects from any script
// SoundManager.Instance.PlaySFX("Jump");
// SoundManager.Instance.PlaySFX("Coin", 0.5f); // With custom volume

// Play music
// SoundManager.Instance.PlayMusic("LevelTheme");
// SoundManager.Instance.PlayMusicWithFade("BossTheme", 2f);

// Control volume
// SoundManager.Instance.SetMasterVolume(0.8f);
// SoundManager.Instance.SetMusicVolume(0.5f);
// SoundManager.Instance.SetSFXVolume(0.7f);

// Music controls
// SoundManager.Instance.PauseMusic();
// SoundManager.Instance.ResumeMusic();
// SoundManager.Instance.StopMusic();

// Play random variation
//SoundManager.Instance.PlayRandomSFX("Hit1", "Hit2", "Hit3");
#endregion
