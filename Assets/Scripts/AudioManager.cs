using UnityEngine;

/// <summary>
/// Manager that centrally manages all sound effects and music in the game.
/// Controls background music, ambient sounds, and audio transitions.
/// </summary>
public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AudioManager>();
            }
            return instance;
        }
    }
    
    [Header("Background Music")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip normalBackgroundMusic;
    [SerializeField] private AudioClip tensionMusic;
    [SerializeField] private AudioClip chaseMusic;
    [SerializeField] private float musicVolume = 0.3f;
    [SerializeField] private float musicFadeSpeed = 1f;
    
    [Header("Ambient Sounds")]
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioClip horrorAmbient;
    [SerializeField] private float ambientVolume = 0.2f;
    
    [Header("UI Sounds")]
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip buttonHoverSound;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip defeatSound;
    
    [Header("Player Sounds")]
    [SerializeField] private AudioClip breathSound;
    [SerializeField] private AudioClip itemPickupSound;
    [SerializeField] private AudioClip doorLockSound;
    
    private MusicState currentMusicState = MusicState.Normal;
    private float targetVolume = 0f;
    
    public enum MusicState
    {
        Normal,
        Tension,
        Chase
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        SetupAudioSources();
    }
    
    void SetupAudioSources()
    {
        // Music source
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
        }
        
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0f;
        targetVolume = musicVolume;
        
        // Ambient source
        if (ambientSource == null)
        {
            GameObject ambientObj = new GameObject("AmbientSource");
            ambientObj.transform.SetParent(transform);
            ambientSource = ambientObj.AddComponent<AudioSource>();
        }
        
        ambientSource.loop = true;
        ambientSource.playOnAwake = false;
        ambientSource.volume = ambientVolume;
        
        // UI source
        if (uiSource == null)
        {
            GameObject uiObj = new GameObject("UISource");
            uiObj.transform.SetParent(transform);
            uiSource = uiObj.AddComponent<AudioSource>();
        }
        
        uiSource.playOnAwake = false;
        uiSource.volume = 0.7f;
    }
    
    void Start()
    {
        // Start normal background music
        PlayMusic(MusicState.Normal);
        
        // Start ambient sounds
        if (horrorAmbient != null)
        {
            ambientSource.clip = horrorAmbient;
            ambientSource.Play();
        }
    }
    
    void Update()
    {
        // Smooth volume transitions
        if (musicSource.volume != targetVolume)
        {
            musicSource.volume = Mathf.MoveTowards(musicSource.volume, targetVolume, Time.deltaTime * musicFadeSpeed);
        }
    }
    
    public void PlayMusic(MusicState newState)
    {
        if (currentMusicState == newState && musicSource.isPlaying) return;
        
        currentMusicState = newState;
        
        AudioClip targetClip = null;
        
        switch (newState)
        {
            case MusicState.Normal:
                targetClip = normalBackgroundMusic;
                break;
            case MusicState.Tension:
                targetClip = tensionMusic;
                break;
            case MusicState.Chase:
                targetClip = chaseMusic;
                break;
        }
        
        if (targetClip != null)
        {
            if (musicSource.clip != targetClip)
            {
                StartCoroutine(CrossfadeMusic(targetClip));
            }
        }
        
        Debug.Log($"[AudioManager] Switching to {newState} music");
    }
    
    System.Collections.IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        // Fade out
        float originalVolume = targetVolume;
        
        while (musicSource.volume > 0.01f)
        {
            musicSource.volume -= Time.deltaTime * musicFadeSpeed;
            yield return null;
        }
        
        // Change clip
        musicSource.clip = newClip;
        musicSource.Play();
        
        // Fade in
        targetVolume = originalVolume;
        while (musicSource.volume < targetVolume - 0.01f)
        {
            musicSource.volume += Time.deltaTime * musicFadeSpeed;
            yield return null;
        }
        
        musicSource.volume = targetVolume;
    }
    
    public void StopMusic()
    {
        targetVolume = 0f;
    }
    
    public void ResumeMusic()
    {
        targetVolume = musicVolume;
    }
    
    public void PlayUISound(AudioClip clip)
    {
        if (uiSource != null && clip != null)
        {
            uiSource.PlayOneShot(clip);
        }
    }
    
    public void PlayButtonClick()
    {
        PlayUISound(buttonClickSound);
    }
    
    public void PlayButtonHover()
    {
        if (uiSource != null && buttonHoverSound != null)
        {
            uiSource.PlayOneShot(buttonHoverSound, 0.3f); // Lower volume for hover
        }
    }
    
    public void PlayVictorySound()
    {
        PlayUISound(victorySound);
    }
    
    public void PlayDefeatSound()
    {
        PlayUISound(defeatSound);
    }
    
    public void PlayBreathSound(AudioSource source)
    {
        if (source != null && breathSound != null)
        {
            source.PlayOneShot(breathSound, 0.5f);
        }
    }
    
    public void PlayItemPickupSound(AudioSource source)
    {
        if (source != null && itemPickupSound != null)
        {
            source.PlayOneShot(itemPickupSound);
        }
    }
    
    public void PlayDoorLockSound(AudioSource source)
    {
        if (source != null && doorLockSound != null)
        {
            source.PlayOneShot(doorLockSound);
        }
    }
    
    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = Mathf.Clamp01(volume);
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        targetVolume = musicVolume;
    }
    
    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        if (ambientSource != null)
        {
            ambientSource.volume = ambientVolume;
        }
    }
}
