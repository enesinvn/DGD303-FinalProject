using UnityEngine;
using TMPro;

/// <summary>
/// Escape point. When all objectives are completed, entering here wins the game.
/// </summary>
public class EscapeZone : MonoBehaviour
{
    [Header("Escape Settings")]
    [SerializeField] private bool requireAllObjectives = true;
    [SerializeField] private string[] requiredObjectiveIDs; // Alternative: check specific objectives
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject lockedIndicator; // Red light, closed door, etc.
    [SerializeField] private GameObject unlockedIndicator; // Green light, open door, etc.
    [SerializeField] private Light escapeLight;
    [SerializeField] private Color lockedColor = Color.red;
    [SerializeField] private Color unlockedColor = Color.green;
    
    [Header("UI")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI promptText;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip escapeSound;
    
    [Header("References")]
    [SerializeField] private ObjectiveSystem objectiveSystem;
    [SerializeField] private GameManager gameManager;
    
    private bool isUnlocked = false;
    private bool playerInZone = false;
    
    void Start()
    {
        if (objectiveSystem == null)
        {
            objectiveSystem = ObjectiveSystem.Instance;
        }
        
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.maxDistance = 15f;
        }
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        // Subscribe to objective events
        if (objectiveSystem != null)
        {
            objectiveSystem.OnObjectiveCompleted += OnObjectiveCompleted;
            objectiveSystem.OnAllObjectivesCompleted += OnAllObjectivesCompleted;
        }
        
        UpdateVisuals();
    }
    
    void OnDestroy()
    {
        if (objectiveSystem != null)
        {
            objectiveSystem.OnObjectiveCompleted -= OnObjectiveCompleted;
            objectiveSystem.OnAllObjectivesCompleted -= OnAllObjectivesCompleted;
        }
    }
    
    void Update()
    {
        if (!isUnlocked)
        {
            CheckIfUnlocked();
        }
        
        if (playerInZone)
        {
            UpdatePrompt();
        }
    }
    
    void CheckIfUnlocked()
    {
        if (objectiveSystem == null) return;
        
        bool shouldUnlock = false;
        
        if (requireAllObjectives)
        {
            // Are all objectives completed?
            var allObjectives = objectiveSystem.GetAllObjectives();
            if (allObjectives.Count > 0)
            {
                shouldUnlock = allObjectives.TrueForAll(obj => obj.isCompleted);
            }
        }
        else if (requiredObjectiveIDs != null && requiredObjectiveIDs.Length > 0)
        {
            // Are specific objectives completed?
            shouldUnlock = true;
            foreach (string objID in requiredObjectiveIDs)
            {
                if (!objectiveSystem.IsObjectiveCompleted(objID))
                {
                    shouldUnlock = false;
                    break;
                }
            }
        }
        
        if (shouldUnlock && !isUnlocked)
        {
            UnlockEscape();
        }
    }
    
    void UnlockEscape()
    {
        isUnlocked = true;
        Debug.Log("[EscapeZone] Escape route unlocked!");
        
        UpdateVisuals();
        
        if (audioSource != null && unlockSound != null)
        {
            audioSource.PlayOneShot(unlockSound);
        }
    }
    
    void UpdateVisuals()
    {
        if (lockedIndicator != null)
        {
            lockedIndicator.SetActive(!isUnlocked);
        }
        
        if (unlockedIndicator != null)
        {
            unlockedIndicator.SetActive(isUnlocked);
        }
        
        if (escapeLight != null)
        {
            escapeLight.color = isUnlocked ? unlockedColor : lockedColor;
        }
    }
    
    void UpdatePrompt()
    {
        if (promptText == null) return;
        
        if (isUnlocked)
        {
            promptText.text = "E - ESCAPE!";
        }
        else
        {
            int completedCount = 0;
            int totalCount = 0;
            
            if (objectiveSystem != null)
            {
                var allObjectives = objectiveSystem.GetAllObjectives();
                totalCount = allObjectives.Count;
                completedCount = allObjectives.FindAll(obj => obj.isCompleted).Count;
            }
            
            promptText.text = $"Locked - Complete all objectives ({completedCount}/{totalCount})";
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
            
            UpdatePrompt();
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && isUnlocked)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                TriggerEscape();
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
    
    void TriggerEscape()
    {
        Debug.Log("[EscapeZone] Player escaped!");
        
        // Complete escape objective
        if (objectiveSystem != null)
        {
            objectiveSystem.CompleteObjective("escape");
        }
        
        if (audioSource != null && escapeSound != null)
        {
            audioSource.PlayOneShot(escapeSound);
        }
        
        if (gameManager != null)
        {
            gameManager.TriggerVictory();
        }
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
    
    void OnObjectiveCompleted(string objectiveID)
    {
        if (playerInZone)
        {
            UpdatePrompt();
        }
    }
    
    void OnAllObjectivesCompleted()
    {
        Debug.Log("[EscapeZone] All objectives completed! Escape is now available.");
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = isUnlocked ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
