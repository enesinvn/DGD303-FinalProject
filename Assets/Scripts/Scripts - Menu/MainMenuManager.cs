using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    [Header("Settings Panel Buttons")]
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private Button creditsBackButton;

    [Header("Audio")]
    [SerializeField] private Slider volumeSlider;

    [Header("Volume Icon Controller")]
    [SerializeField] private VolumeIconController volumeIconController;

    private void Start()
    {
        // Add button listeners
        playButton.onClick.AddListener(PlayGame);
        settingsButton.onClick.AddListener(OpenSettings);
        creditsButton.onClick.AddListener(OpenCredits);
        quitButton.onClick.AddListener(QuitGame);
        settingsBackButton.onClick.AddListener(CloseSettings);
        creditsBackButton.onClick.AddListener(CloseCredits);

        // Volume slider setup
        volumeSlider.onValueChanged.AddListener(ChangeVolume);
        volumeSlider.value = AudioListener.volume;

        // Only main menu is open at start
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);

        // Set icon at start
        if (volumeIconController != null)
        {
            volumeIconController.UpdateVolumeIcon(volumeSlider.value);
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene"); // Scene name
    }

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OpenCredits()
    {
        mainMenuPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void CloseCredits()
    {
        creditsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void ChangeVolume(float volume)
    {
    AudioListener.volume = volume;
    
    // Update the icon
    if (volumeIconController != null)
    {
        volumeIconController.UpdateVolumeIcon(volume);
    }
    
    PlayerPrefs.SetFloat("MasterVolume", volume);
    PlayerPrefs.Save();
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
}