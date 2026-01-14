using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game State")]
    [SerializeField] private bool isPaused = false;
    [SerializeField] private bool gameOver = false;
    [SerializeField] private bool gameWon = false;
    
    [Header("UI")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject winMenu;
    
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private HealthSystem playerHealth;
    [SerializeField] private ObjectiveSystem objectiveSystem;
    
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
        
        if (objectiveSystem == null)
        {
            objectiveSystem = FindFirstObjectByType<ObjectiveSystem>();
        }
        
        if (objectiveSystem != null)
        {
            objectiveSystem.OnObjectiveCompleted += CheckWinCondition;
        }
        
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        
        if (gameOverMenu != null)
        {
            gameOverMenu.SetActive(false);
        }
        
        if (winMenu != null)
        {
            winMenu.SetActive(false);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !gameOver && !gameWon)
        {
            TogglePause();
        }
        
        if (Input.GetKeyDown(KeyCode.F5))
        {
            if (SaveSystem.Instance != null && !isPaused && !gameOver)
            {
                SaveSystem.Instance.SaveGame();
                Debug.Log("Game saved! (F5)");
                
                SaveLoadUI saveLoadUI = FindFirstObjectByType<SaveLoadUI>();
                if (saveLoadUI != null)
                {
                    saveLoadUI.ShowNotification("Game saved! (F5)");
                }
            }
        }
        
        if (Input.GetKeyDown(KeyCode.F9))
        {
            if (SaveSystem.Instance != null && !isPaused && !gameOver)
            {
                bool success = SaveSystem.Instance.LoadGame();
                if (success)
                {
                    Debug.Log("Game loaded! (F9)");
                    
                    SaveLoadUI saveLoadUI = FindFirstObjectByType<SaveLoadUI>();
                    if (saveLoadUI != null)
                    {
                        saveLoadUI.ShowNotification("Game loaded! (F9)");
                    }
                    
                    ResumeGame();
                }
                else
                {
                    Debug.LogWarning("Save file not found!");
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
    
    void CheckWinCondition(string objectiveID)
    {
        if (objectiveSystem == null || gameWon || gameOver) return;
        
        Objective completedObj = objectiveSystem.GetAllObjectives().Find(o => o.objectiveID == objectiveID);
        if (completedObj != null && completedObj.type == ObjectiveType.Escape)
        {
            HandleGameWin();
        }
    }
    
    void HandleGameWin()
    {
        gameWon = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (winMenu != null)
        {
            winMenu.SetActive(true);
        }
        
        Debug.Log("YOU ESCAPED! Game Won!");
    }
    
    public bool IsGameWon()
    {
        return gameWon;
    }
    
    void OnDestroy()
    {
        if (objectiveSystem != null)
        {
            objectiveSystem.OnObjectiveCompleted -= CheckWinCondition;
        }
        
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= HandlePlayerDeath;
        }
    }
}

