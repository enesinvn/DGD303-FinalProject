using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    
    [Header("UI Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider mouseSensitivitySlider;
    
    [Header("UI Dropdowns")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    
    [Header("UI Toggles")]
    [SerializeField] private Toggle fullscreenToggle;
    
    [Header("Referanslar")]
    [SerializeField] private PlayerController playerController;
    
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string MOUSE_SENSITIVITY_KEY = "MouseSensitivity";
    private const string QUALITY_KEY = "Quality";
    private const string RESOLUTION_KEY = "Resolution";
    private const string FULLSCREEN_KEY = "Fullscreen";
    
    void Start()
    {
        LoadSettings();
        SetupUI();
    }
    
    void SetupUI()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }
        
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
        }
        
        if (qualityDropdown != null)
        {
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }
        
        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
        }
        
        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }
    }
    
    public void SetMasterVolume(float volume)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        }
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, volume);
    }
    
    public void SetMusicVolume(float volume)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        }
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
    }
    
    public void SetSFXVolume(float volume)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        }
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
    }
    
    public void SetMouseSensitivity(float sensitivity)
    {
        if (playerController != null)
        {
            playerController.MouseSensitivity = sensitivity;
        }
        PlayerPrefs.SetFloat(MOUSE_SENSITIVITY_KEY, sensitivity);
    }
    
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt(QUALITY_KEY, qualityIndex);
    }
    
    public void SetResolution(int resolutionIndex)
    {
        Resolution[] resolutions = Screen.resolutions;
        if (resolutionIndex >= 0 && resolutionIndex < resolutions.Length)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            PlayerPrefs.SetInt(RESOLUTION_KEY, resolutionIndex);
        }
    }
    
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(FULLSCREEN_KEY, isFullscreen ? 1 : 0);
    }
    
    void LoadSettings()
    {
        // Ses ayarları
        float masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = masterVolume;
            SetMasterVolume(masterVolume);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolume;
            SetMusicVolume(musicVolume);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVolume;
            SetSFXVolume(sfxVolume);
        }
        
        // Mouse hassasiyeti
        float mouseSensitivity = PlayerPrefs.GetFloat(MOUSE_SENSITIVITY_KEY, 2f);
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.value = mouseSensitivity;
            SetMouseSensitivity(mouseSensitivity);
        }
        
        // Kalite
        int quality = PlayerPrefs.GetInt(QUALITY_KEY, QualitySettings.GetQualityLevel());
        if (qualityDropdown != null)
        {
            qualityDropdown.value = quality;
            SetQuality(quality);
        }
        
        // Fullscreen
        bool fullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, Screen.fullScreen ? 1 : 0) == 1;
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = fullscreen;
            SetFullscreen(fullscreen);
        }
    }
    
    public void SaveSettings()
    {
        PlayerPrefs.Save();
    }
}

