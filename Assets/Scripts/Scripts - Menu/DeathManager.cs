using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject deathPanel;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Restart Event")]
    public UnityEvent OnRestartRequested; // Arkadaşının sistemi bunu dinleyecek

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        restartButton.onClick.AddListener(RestartFromCheckpoint);
        mainMenuButton.onClick.AddListener(LoadMainMenu);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        deathPanel.SetActive(false);
    }

    public void ShowDeathScreen()
    {
        deathPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RestartFromCheckpoint()
    {
        deathPanel.SetActive(false);
        Time.timeScale = 1f;

        // Arkadaşının checkpoint sistemini tetikle
        OnRestartRequested?.Invoke();
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}