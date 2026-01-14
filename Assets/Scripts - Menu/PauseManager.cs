using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject pauseSettingsPanel;

    [Header("Pause Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Settings")]
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private Slider volumeSlider;

    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Ana menü scene adı

    [Header("Volume Icon Controller")]
    [SerializeField] private VolumeIconController volumeIconController;

    private bool isPaused = false;

    private void Start()
    {
        // Buton listener'ları
        resumeButton.onClick.AddListener(ResumeGame);
        settingsButton.onClick.AddListener(OpenSettings);
        mainMenuButton.onClick.AddListener(LoadMainMenu);
        quitButton.onClick.AddListener(QuitGame);
        settingsBackButton.onClick.AddListener(CloseSettings);

        // Volume
        volumeSlider.onValueChanged.AddListener(ChangeVolume);
        volumeSlider.value = AudioListener.volume;

        // Başlangıçta pause menü kapalı
        pausePanel.SetActive(false);
        pauseSettingsPanel.SetActive(false);

        // Başlangıçta icon'u ayarla
        if (volumeIconController != null)
        {
            volumeIconController.UpdateVolumeIcon(volumeSlider.value);
        }
    }

    private void Update()
    {
        // ESC tuşu kontrolü
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        pauseSettingsPanel.SetActive(false);
        Time.timeScale = 0f; // Oyunu durdur
        isPaused = true;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        pauseSettingsPanel.SetActive(false);
        Time.timeScale = 1f; // Oyunu devam ettir
        isPaused = false;
    }

    public void OpenSettings()
    {
        pausePanel.SetActive(false);
        pauseSettingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        pauseSettingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Time scale'i normale al
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ChangeVolume(float volume)
    {
    AudioListener.volume = volume;
    
    // Icon'u güncelle
    if (volumeIconController != null)
    {
        volumeIconController.UpdateVolumeIcon(volume);
    }
    
    PlayerPrefs.SetFloat("MasterVolume", volume);
    PlayerPrefs.Save();
    }

    public void QuitGame()
    {
        Time.timeScale = 1f; // Time scale'i normale al
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}