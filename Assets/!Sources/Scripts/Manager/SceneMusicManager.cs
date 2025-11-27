using UnityEngine;

public class SceneMusicManager : MonoBehaviour
{
    [SerializeField] private string musicName;
    [SerializeField] private float fadeDuration = 0.5f;
    void Start()
    {
        if (SoundManager.Instance != null)
        {
            //Play this music at the start :D
            SoundManager.Instance.PlayMusicWithFade(musicName, fadeDuration);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
