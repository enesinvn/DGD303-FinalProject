using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// Oyunun başlangıcında hikaye ve kontrolleri gösteren intro/tutorial sistemi.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("Story/Intro UI")]
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private TextMeshProUGUI storySkipText; // Optional: "Press any key to skip"
    [SerializeField] private float storyFadeInDuration = 1f;
    [SerializeField] private float storyDisplayDuration = 8f;
    [SerializeField] private float storyFadeOutDuration = 1f;
    
    [Header("Tutorial UI")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI tutorialText; // Single text for everything
    
    [Header("Story Content")]
    [TextArea(5, 10)]
    [SerializeField] private string storyContent = "You were hiking in the woods with your friends but you stayed behind and suddenly you started to black out...\n\n" +
        "You wake up in a dark, abandoned house...\n\n" +
        "The last thing you remember is being captured.\n\n" +
        "Now, you must escape before it finds you.\n\n" +
        "Stay quiet. Stay hidden. Stay alive.";
    
    [Header("Tutorial Content")]
    [TextArea(10, 20)]
    [SerializeField] private string tutorialContent = 
        "== WELCOME ==\n\n" +
        "CONTROLS:\n" +
        "MOVEMENT: W A S D\n" +
        "LOOK: Mouse\n" +
        "SPRINT: Left Shift (uses stamina)\n" +
        "CROUCH: C or Left Ctrl (quieter)\n" +
        "FLASHLIGHT: F (enemies can see!)\n" +
        "INTERACT: E (doors, items, hiding spots)\n" +
        "PAUSE: ESC\n\n" +
        "== OBJECTIVE ==\n" +
        "Complete all tasks and reach the escape zone.\n" +
        "Avoid detection. Your survival depends on it.\n\n" +
        "Press SPACE or E to start...";
    
    [Header("Settings")]
    [SerializeField] private bool enableIntro = true;
    [SerializeField] private float tutorialStartDelay = 1f;
    [SerializeField] private bool forceShowIntro = false; // Force show even if completed before
    [SerializeField] private KeyCode resetIntroKey = KeyCode.F9; // Press F9 to reset intro
    
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    
    private enum IntroState
    {
        Story,
        Tutorial,
        Completed
    }
    
    private IntroState currentState = IntroState.Story;
    private bool introActive = false;
    private CanvasGroup storyCanvasGroup;
    private CanvasGroup tutorialCanvasGroup;
    
    private static TutorialManager instance;
    
    // Static property for other scripts to check if intro is active
    public static bool IsIntroActiveStatic { get; private set; } = false;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            IsIntroActiveStatic = false; // Reset on awake
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void OnDestroy()
    {
        // Clean up static flag when destroyed
        if (instance == this)
        {
            IsIntroActiveStatic = false;
        }
    }
    
    void Start()
    {
        Debug.Log("[TutorialManager] Starting...");
        
        // Find references if not assigned
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
            Debug.Log($"[TutorialManager] PlayerController found: {playerController != null}");
        }
        
        // Setup canvas groups for fading
        if (storyPanel != null)
        {
            storyCanvasGroup = storyPanel.GetComponent<CanvasGroup>();
            if (storyCanvasGroup == null)
            {
                storyCanvasGroup = storyPanel.AddComponent<CanvasGroup>();
            }
            storyPanel.SetActive(false);
            Debug.Log("[TutorialManager] Story Panel setup complete");
        }
        else
        {
            Debug.LogError("[TutorialManager] Story Panel is NULL! Please assign it in Inspector!");
        }
        
        if (tutorialPanel != null)
        {
            tutorialCanvasGroup = tutorialPanel.GetComponent<CanvasGroup>();
            if (tutorialCanvasGroup == null)
            {
                tutorialCanvasGroup = tutorialPanel.AddComponent<CanvasGroup>();
            }
            tutorialPanel.SetActive(false);
            Debug.Log("[TutorialManager] Tutorial Panel setup complete");
        }
        else
        {
            Debug.LogError("[TutorialManager] Tutorial Panel is NULL! Please assign it in Inspector!");
        }
        
        // Check if intro should be shown
        Debug.Log($"[TutorialManager] Enable Intro: {enableIntro}");
        Debug.Log($"[TutorialManager] Force Show Intro: {forceShowIntro}");
        
        if (enableIntro)
        {
            int introCompleted = PlayerPrefs.GetInt("IntroCompleted", 0);
            Debug.Log($"[TutorialManager] IntroCompleted PlayerPref: {introCompleted}");
            
            // Check if player has seen intro before (or force show)
            if (introCompleted == 0 || forceShowIntro)
            {
                if (forceShowIntro)
                {
                    Debug.Log("[TutorialManager] Force showing intro (forceShowIntro = true)");
                }
                else
                {
                    Debug.Log("[TutorialManager] Starting intro sequence...");
                }
                StartCoroutine(StartIntroSequence());
            }
            else
            {
                Debug.Log("[TutorialManager] Intro already seen, skipping. Press F9 to reset or enable 'Force Show Intro'.");
            }
        }
        else
        {
            Debug.Log("[TutorialManager] Intro is disabled in settings!");
        }
    }
    
    void Update()
    {
        // Debug: Reset intro with F9 key
        if (Input.GetKeyDown(resetIntroKey))
        {
            Debug.Log("[TutorialManager] Reset key pressed! Resetting intro...");
            ResetIntro();
            // Reload scene or restart intro
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }
        
        if (!introActive) return;
        
        // Check for skip input during story (skip to tutorial)
        if (currentState == IntroState.Story)
        {
            if (Keyboard.current != null && 
                (Keyboard.current.spaceKey.wasPressedThisFrame || 
                 Keyboard.current.enterKey.wasPressedThisFrame ||
                 Keyboard.current.escapeKey.wasPressedThisFrame))
            {
                // Skip story and go directly to tutorial
                StopAllCoroutines();
                StartCoroutine(SkipToTutorial());
            }
        }
        // Check for input during tutorial (start game)
        else if (currentState == IntroState.Tutorial)
        {
            if (Keyboard.current != null && 
                (Keyboard.current.spaceKey.wasPressedThisFrame || 
                 Keyboard.current.enterKey.wasPressedThisFrame ||
                 Keyboard.current.eKey.wasPressedThisFrame))
            {
                CompleteIntro();
            }
        }
    }
    
    IEnumerator SkipToTutorial()
    {
        // Hide story panel immediately
        if (storyPanel != null)
        {
            storyPanel.SetActive(false);
        }
        
        // Show tutorial
        currentState = IntroState.Tutorial;
        yield return StartCoroutine(ShowTutorial());
    }
    
    IEnumerator StartIntroSequence()
    {
        Debug.Log("[TutorialManager] StartIntroSequence() called");
        introActive = true;
        IsIntroActiveStatic = true; // Set static flag for other scripts
        
        // Disable player controls during intro
        if (playerController != null)
        {
            playerController.enabled = false;
            Debug.Log("[TutorialManager] Player controller disabled");
        }
        else
        {
            Debug.LogWarning("[TutorialManager] Player controller is NULL!");
        }
        
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log($"[TutorialManager] Waiting {tutorialStartDelay} seconds before showing story...");
        yield return new WaitForSeconds(tutorialStartDelay);
        
        // Show story
        Debug.Log("[TutorialManager] Changing state to Story");
        currentState = IntroState.Story;
        yield return StartCoroutine(ShowStory());
        
        // Show tutorial
        Debug.Log("[TutorialManager] Changing state to Tutorial");
        currentState = IntroState.Tutorial;
        yield return StartCoroutine(ShowTutorial());
        
        Debug.Log("[TutorialManager] Intro sequence complete, waiting for player input...");
        // Tutorial will wait for player input (handled in Update method)
        // CompleteIntro() will be called when player presses SPACE/E/ENTER
    }
    
    IEnumerator ShowStory()
    {
        Debug.Log("[TutorialManager] ShowStory() called");
        
        if (storyPanel == null)
        {
            Debug.LogError("[TutorialManager] Story Panel is NULL! Cannot show story.");
            yield break;
        }
        
        if (storyText == null)
        {
            Debug.LogError("[TutorialManager] Story Text is NULL! Cannot show story.");
            yield break;
        }
        
        Debug.Log("[TutorialManager] Showing story...");
        
        // Set story content
        storyText.text = storyContent;
        
        // Show skip text if available
        if (storySkipText != null)
        {
            storySkipText.text = "Press SPACE or ENTER to skip...";
        }
        
        // Show panel
        storyPanel.SetActive(true);
        
        // Fade in
        float elapsedTime = 0f;
        while (elapsedTime < storyFadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            if (storyCanvasGroup != null)
            {
                storyCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / storyFadeInDuration);
            }
            yield return null;
        }
        
        if (storyCanvasGroup != null)
        {
            storyCanvasGroup.alpha = 1f;
        }
        
        // Display
        yield return new WaitForSeconds(storyDisplayDuration);
        
        // Fade out
        elapsedTime = 0f;
        while (elapsedTime < storyFadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            if (storyCanvasGroup != null)
            {
                storyCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / storyFadeOutDuration);
            }
            yield return null;
        }
        
        if (storyCanvasGroup != null)
        {
            storyCanvasGroup.alpha = 0f;
        }
        
        // Hide panel
        storyPanel.SetActive(false);
        
        Debug.Log("[TutorialManager] Story completed!");
    }
    
    IEnumerator ShowTutorial()
    {
        Debug.Log("[TutorialManager] ShowTutorial() called");
        
        if (tutorialPanel == null)
        {
            Debug.LogError("[TutorialManager] Tutorial Panel is NULL! Cannot show tutorial.");
            yield break;
        }
        
        if (tutorialText == null)
        {
            Debug.LogError("[TutorialManager] Tutorial Text is NULL! Cannot show tutorial.");
            yield break;
        }
        
        Debug.Log("[TutorialManager] Showing tutorial...");
        
        // Set tutorial content
        tutorialText.text = tutorialContent;
        
        // Show panel
        tutorialPanel.SetActive(true);
        
        // Fade in
        float elapsedTime = 0f;
        float fadeInDuration = 1f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            if (tutorialCanvasGroup != null)
            {
                tutorialCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            }
            yield return null;
        }
        
        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.alpha = 1f;
        }
        
        Debug.Log("[TutorialManager] Tutorial shown, waiting for player input...");
        
        // Wait for player input (handled in Update)
    }
    
    void CompleteIntro()
    {
        introActive = false;
        IsIntroActiveStatic = false; // Clear static flag - game can start now!
        currentState = IntroState.Completed;
        
        // Fade out tutorial
        StartCoroutine(FadeOutAndComplete());
    }
    
    IEnumerator FadeOutAndComplete()
    {
        // Fade out
        float elapsedTime = 0f;
        float fadeOutDuration = 0.5f;
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            if (tutorialCanvasGroup != null)
            {
                tutorialCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            }
            yield return null;
        }
        
        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.alpha = 0f;
        }
        
        // Hide panel
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
        
        // Enable player controls
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        // Unlock cursor (will be locked again by player controller)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Save intro completion
        PlayerPrefs.SetInt("IntroCompleted", 1);
        PlayerPrefs.Save();
        
        Debug.Log("[TutorialManager] Intro/Tutorial completed! Game started!");
    }
    
    // Public methods for manual control
    public void SkipIntro()
    {
        StopAllCoroutines();
        CompleteIntro();
    }
    
    public void ResetIntro()
    {
        PlayerPrefs.SetInt("IntroCompleted", 0);
        PlayerPrefs.Save();
        
        currentState = IntroState.Story;
        introActive = false;
        
        Debug.Log("[TutorialManager] Intro reset!");
    }
    
    public bool IsIntroActive()
    {
        return introActive;
    }
    
    public bool IsIntroCompleted()
    {
        return PlayerPrefs.GetInt("IntroCompleted", 0) == 1;
    }
}
