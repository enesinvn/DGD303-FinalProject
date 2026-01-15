using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Oyunun genel durumunu yöneten merkezi manager.
/// Oyun bitişi, kazanma/kaybetme ekranları, pause vb.
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameManager>();
            }
            return instance;
        }
    }
    
    [Header("Game State")]
    [SerializeField] private bool isGameOver = false;
    [SerializeField] private bool hasWon = false;
    
    [Header("UI Screens")]
    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private GameObject defeatScreen;
    [SerializeField] private TextMeshProUGUI victoryMessageText;
    [SerializeField] private TextMeshProUGUI defeatMessageText;
    
    [Header("Victory Settings")]
    [SerializeField] private string victoryMessage = "You Escaped!\nAll objectives completed!";
    [SerializeField] private float victoryDelay = 1f;
    
    [Header("Defeat Settings")]
    [SerializeField] private string defeatMessage = "Game Over\nYou were caught!";
    
    [Header("References")]
    [SerializeField] private ObjectiveSystem objectiveSystem;
    [SerializeField] private PlayerController playerController;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip defeatSound;
    
    private AudioManager audioManager;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        if (objectiveSystem == null)
        {
            objectiveSystem = ObjectiveSystem.Instance;
        }
        
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
        }
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioManager = AudioManager.Instance;
        
        // Subscribe to objective system events
        if (objectiveSystem != null)
        {
            objectiveSystem.OnAllObjectivesCompleted += OnAllObjectivesCompleted;
        }
        
        // Hide game over screens
        if (victoryScreen != null) victoryScreen.SetActive(false);
        if (defeatScreen != null) defeatScreen.SetActive(false);
    }
    
    void OnDestroy()
    {
        if (objectiveSystem != null)
        {
            objectiveSystem.OnAllObjectivesCompleted -= OnAllObjectivesCompleted;
        }
    }
    
    void OnAllObjectivesCompleted()
    {
        Debug.Log("[GameManager] All objectives completed! Checking for victory...");
        // Victory will be triggered by escape trigger
    }
    
    public void TriggerVictory()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        hasWon = true;
        
        Debug.Log("[GameManager] Victory!");
        
        // Stop all enemy sounds
        StopAllEnemySounds();
        
        Invoke(nameof(ShowVictoryScreen), victoryDelay);
        
        // Play victory sound
        if (audioManager != null)
        {
            audioManager.PlayVictorySound();
            audioManager.StopMusic();
        }
        else if (audioSource != null && victorySound != null)
        {
            audioSource.PlayOneShot(victorySound);
        }
        
        // Disable player movement
        DisablePlayerControls();
    }
    
    public void TriggerDefeat(string reason = "")
    {
        if (isGameOver) return;
        
        isGameOver = true;
        hasWon = false;
        
        Debug.Log($"[GameManager] Defeat! Reason: {reason}");
        
        if (!string.IsNullOrEmpty(reason))
        {
            defeatMessage = reason;
        }
        
        // Stop all enemy sounds
        StopAllEnemySounds();
        
        Invoke(nameof(ShowDefeatScreen), 0.5f);
        
        // Play defeat sound
        if (audioManager != null)
        {
            audioManager.PlayDefeatSound();
            audioManager.StopMusic();
        }
        else if (audioSource != null && defeatSound != null)
        {
            audioSource.PlayOneShot(defeatSound);
        }
        
        // Disable player movement
        DisablePlayerControls();
    }
    
    void ShowVictoryScreen()
    {
        if (victoryScreen != null)
        {
            victoryScreen.SetActive(true);
            
            if (victoryMessageText != null)
            {
                victoryMessageText.text = victoryMessage;
            }
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }
    
    void ShowDefeatScreen()
    {
        if (defeatScreen != null)
        {
            defeatScreen.SetActive(true);
            
            if (defeatMessageText != null)
            {
                defeatMessageText.text = defeatMessage;
            }
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }
    
    void DisablePlayerControls()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Disable other player components if needed
        PlayerHiding hiding = FindFirstObjectByType<PlayerHiding>();
        if (hiding != null) hiding.enabled = false;
        
        PlayerInteraction interaction = FindFirstObjectByType<PlayerInteraction>();
        if (interaction != null) interaction.enabled = false;
        
        // Stop all player audio sources
        StopAllPlayerSounds();
    }
    
    void StopAllPlayerSounds()
    {
        // Find player GameObject
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        // Stop all AudioSources on player and children
        AudioSource[] playerAudioSources = player.GetComponentsInChildren<AudioSource>();
        foreach (AudioSource audioSource in playerAudioSources)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
        
        Debug.Log($"[GameManager] Stopped {playerAudioSources.Length} player audio sources");
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Assuming main menu is scene 0
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
    
    public void ResumeGame()
    {
        // Resume game after loading save
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Re-enable player controls if they were disabled
        if (playerController != null && !isGameOver)
        {
            playerController.enabled = true;
        }
        
        PlayerHiding hiding = FindFirstObjectByType<PlayerHiding>();
        if (hiding != null && !isGameOver)
        {
            hiding.enabled = true;
        }
        
        PlayerInteraction interaction = FindFirstObjectByType<PlayerInteraction>();
        if (interaction != null && !isGameOver)
        {
            interaction.enabled = true;
        }
        
        Debug.Log("[GameManager] Game resumed!");
    }
    
    public bool IsGameOver()
    {
        return isGameOver;
    }
    
    public bool HasWon()
    {
        return hasWon;
    }
    
    void StopAllEnemySounds()
    {
        // Find all enemies and stop their sounds
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (EnemyAI enemy in enemies)
        {
            AudioSource enemyAudio = enemy.GetComponent<AudioSource>();
            if (enemyAudio != null && enemyAudio.isPlaying)
            {
                enemyAudio.Stop();
            }
        }
        
        Debug.Log($"[GameManager] Stopped {enemies.Length} enemy audio sources");
    }
}
