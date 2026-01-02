using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Oyun Durumu")]
    [SerializeField] private bool isPaused = false;
    [SerializeField] private bool gameOver = false;
    
    [Header("UI")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverMenu;
    
    [Header("Referanslar")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private HealthSystem playerHealth;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath += HandlePlayerDeath;
        }
        
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        
        if (gameOverMenu != null)
        {
            gameOverMenu.SetActive(false);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !gameOver)
        {
            TogglePause();
        }
        
        // F5 ile kaydet
        if (Input.GetKeyDown(KeyCode.F5))
        {
            if (SaveSystem.Instance != null && !isPaused && !gameOver)
            {
                SaveSystem.Instance.SaveGame();
                Debug.Log("Oyun kaydedildi! (F5)");
                
                // UI bildirimi göster
                SaveLoadUI saveLoadUI = FindFirstObjectByType<SaveLoadUI>();
                if (saveLoadUI != null)
                {
                    saveLoadUI.ShowNotification("Oyun kaydedildi! (F5)");
                }
            }
        }
        
        // F9 ile yükle
        if (Input.GetKeyDown(KeyCode.F9))
        {
            if (SaveSystem.Instance != null && !isPaused && !gameOver)
            {
                bool success = SaveSystem.Instance.LoadGame();
                if (success)
                {
                    Debug.Log("Oyun yüklendi! (F9)");
                    
                    // UI bildirimi göster
                    SaveLoadUI saveLoadUI = FindFirstObjectByType<SaveLoadUI>();
                    if (saveLoadUI != null)
                    {
                        saveLoadUI.ShowNotification("Oyun yüklendi! (F9)");
                    }
                    
                    // Oyunu devam ettir
                    ResumeGame();
                }
                else
                {
                    Debug.LogWarning("Kayıt dosyası bulunamadı!");
                }
            }
        }
    }
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        
        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }
    
    void PauseGame()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
        }
    }
    
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        
        isPaused = false;
    }
    
    void HandlePlayerDeath()
    {
        gameOver = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (gameOverMenu != null)
        {
            gameOverMenu.SetActive(true);
        }
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void QuitGame()
    {
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    public bool IsPaused()
    {
        return isPaused;
    }
    
    public bool IsGameOver()
    {
        return gameOver;
    }
}

