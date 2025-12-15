using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Ambient Sounds")]
    [SerializeField] private AudioSource ambientAudioSource;
    [SerializeField] private AudioClip[] ambientSounds;
    [SerializeField] private float ambientVolume = 0.3f;
    
    [Header("Music")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private float musicVolume = 0.2f;
    
    [Header("Breath")]
    [SerializeField] private AudioSource breathingAudioSource;
    [SerializeField] private AudioClip normalBreathing;
    [SerializeField] private AudioClip heavyBreathing;
    [SerializeField] private float breathingVolume = 0.5f;
    
    private bool isBreathingHeavy = false;
    
    void Start()
    {
        SetupAudioSources();
        PlayAmbientSound();
        PlayBackgroundMusic();
    }
    
    void SetupAudioSources()
    {
        // Ambient Audio Source setup
        if (ambientAudioSource != null)
        {
            ambientAudioSource.loop = true;
            ambientAudioSource.volume = ambientVolume;
            ambientAudioSource.spatialBlend = 0f;
        }
        
        // Music Audio Source setup
        if (musicAudioSource != null)
        {
            musicAudioSource.loop = true;
            musicAudioSource.volume = musicVolume;
            musicAudioSource.spatialBlend = 0f;
        }
        
        // Breathing Audio Source setup
        if (breathingAudioSource != null)
        {
            breathingAudioSource.loop = true;
            breathingAudioSource.volume = 0f;
            breathingAudioSource.spatialBlend = 0f;
        }
    }
    
    void PlayAmbientSound()
    {
        if (ambientAudioSource != null && ambientSounds != null && ambientSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, ambientSounds.Length);
            ambientAudioSource.clip = ambientSounds[randomIndex];
            ambientAudioSource.Play();
        }
    }
    
    void PlayBackgroundMusic()
    {
        if (musicAudioSource != null && backgroundMusic != null)
        {
            musicAudioSource.clip = backgroundMusic;
            musicAudioSource.Play();
        }
    }
    
    public void SetBreathingState(bool isHeavy, float staminaPercent)
    {
        if (breathingAudioSource == null) return;
        
        if (staminaPercent < 30f && !isBreathingHeavy)
        {
            isBreathingHeavy = true;
            if (heavyBreathing != null)
            {
                breathingAudioSource.clip = heavyBreathing;
                breathingAudioSource.Play();
            }
            // Fade in
            breathingAudioSource.volume = breathingVolume;
        }
        else if (staminaPercent > 60f && isBreathingHeavy)
        {
            isBreathingHeavy = false;
            // Fade out
            breathingAudioSource.volume = 0f;
            breathingAudioSource.Stop();
        }
    }
    
    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (ambientAudioSource != null && clip != null)
        {
            ambientAudioSource.PlayOneShot(clip, volume);
        }
    }
}