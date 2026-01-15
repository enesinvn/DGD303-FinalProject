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
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Main menu scene name

    [Header("Volume Icon Controller")]
    [SerializeField] private VolumeIconController volumeIconController;

    private bool isPaused = false;
    private AudioManager audioManager;
    private PlayerController playerController;

    private void Start()
    {
        // Get AudioManager reference
        audioManager = AudioManager.Instance;
        
        // Get PlayerController reference
        playerController = FindFirstObjectByType<PlayerController>();
        
        // Button listeners
        resumeButton.onClick.AddListener(ResumeGame);
        settingsButton.onClick.AddListener(OpenSettings);
        mainMenuButton.onClick.AddListener(LoadMainMenu);
        quitButton.onClick.AddListener(QuitGame);
        settingsBackButton.onClick.AddListener(CloseSettings);

        // Volume
        volumeSlider.onValueChanged.AddListener(ChangeVolume);
        volumeSlider.value = AudioListener.volume;

        // Pause menu is closed at start
        pausePanel.SetActive(false);
        pauseSettingsPanel.SetActive(false);

        // Set icon at start
        if (volumeIconController != null)
        {
            volumeIconController.UpdateVolumeIcon(volumeSlider.value);
        }
    }

    private void Update()
    {
        // ESC key check
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Don't allow pause during game over
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null && gameManager.IsGameOver())
            {
                return;
            }
            
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
        Time.timeScale = 0f; // Pause the game
        isPaused = true;
        
        // Disable PlayerController to prevent cursor locking
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Pause all enemy sounds
        PauseAllEnemySounds();
        
        // Pause all player sounds
        PauseAllPlayerSounds();
        
        // Show cursor - FORCE it!
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("[PauseManager] Game paused - Cursor unlocked");
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        pauseSettingsPanel.SetActive(false);
        Time.timeScale = 1f; // Resume the game
        isPaused = false;
        
        // Re-enable PlayerController
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        // Hide cursor - PlayerController will handle it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log("[PauseManager] Game resumed - Player enabled");
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
        Time.timeScale = 1f; // Reset time scale to normal
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ChangeVolume(float volume)
    {
    AudioListener.volume = volume;
    
    // Update icon
    if (volumeIconController != null)
    {
        volumeIconController.UpdateVolumeIcon(volume);
    }
    
    PlayerPrefs.SetFloat("MasterVolume", volume);
    PlayerPrefs.Save();
    }

    public void QuitGame()
    {
        Time.timeScale = 1f; // Reset time scale to normal
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    void PauseAllEnemySounds()
    {
        // Find all enemies and pause their sounds
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (EnemyAI enemy in enemies)
        {
            AudioSource enemyAudio = enemy.GetComponent<AudioSource>();
            if (enemyAudio != null && enemyAudio.isPlaying)
            {
                enemyAudio.Pause();
            }
        }
        
        Debug.Log($"[PauseManager] Paused {enemies.Length} enemy audio sources");
    }
    
    void PauseAllPlayerSounds()
    {
        // Find player GameObject
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        // Pause all AudioSources on player and children
        AudioSource[] playerAudioSources = player.GetComponentsInChildren<AudioSource>();
        foreach (AudioSource audioSource in playerAudioSources)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }
        
        Debug.Log($"[PauseManager] Paused {playerAudioSources.Length} player audio sources");
    }
}