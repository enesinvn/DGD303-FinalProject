using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Escape elevator. Activates when required parts are collected and triggers escape when entered.
/// </summary>
public class Elevator : MonoBehaviour
{
    [Header("Elevator Settings")]
    [SerializeField] private string[] requiredItemNames = new string[] { 
        "Nails", 
        "Keycard", 
        "Screwdriver",
        "Elevator Button",
        "Elevator Call Button"
    };
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject inactiveDoor; // Closed door with red light
    [SerializeField] private GameObject activeDoor; // Open door with green light
    [SerializeField] private Light elevatorLight;
    [SerializeField] private Color inactiveColor = Color.red;
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private GameObject elevatorPanel; // Panel with indicators
    
    [Header("UI")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private TMP_Text statusText; // Status display near elevator (3D or UI text)
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip activationSound;
    [SerializeField] private AudioClip doorOpenSound;
    [SerializeField] private AudioClip elevatorStartSound;
    [SerializeField] private AudioClip elevatorMovingSound;
    
    [Header("Animation")]
    [SerializeField] private Animator elevatorAnimator;
    [SerializeField] private string doorOpenTrigger = "OpenDoor";
    [SerializeField] private string elevatorStartTrigger = "Start";
    
    [Header("Escape Settings")]
    [SerializeField] private float escapeDelay = 3f; // Wait time for door closing and elevator movement
    
    [Header("References")]
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private ObjectiveSystem objectiveSystem;
    [SerializeField] private GameManager gameManager;
    
    private bool isActive = false;
    private bool playerInZone = false;
    private bool isEscaping = false;
    
    void Start()
    {
        if (inventorySystem == null)
        {
            inventorySystem = FindFirstObjectByType<InventorySystem>();
        }
        
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
            audioSource.maxDistance = 20f;
        }
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        // Subscribe to inventory changes
        if (inventorySystem != null)
        {
            inventorySystem.OnInventoryChanged += CheckIfCanActivate;
        }
        
        UpdateVisuals();
        UpdateStatusDisplay();
    }
    
    void OnDestroy()
    {
        if (inventorySystem != null)
        {
            inventorySystem.OnInventoryChanged -= CheckIfCanActivate;
        }
    }
    
    void Update()
    {
        if (!isActive && !isEscaping)
        {
            CheckIfCanActivate();
        }
        
        if (playerInZone && !isEscaping)
        {
            UpdatePrompt();
            
            // Press E to enter elevator
            if (isActive && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                EnterElevator();
            }
        }
    }
    
    void CheckIfCanActivate()
    {
        if (isActive || inventorySystem == null) return;
        
        bool hasAllParts = true;
        
        foreach (string itemName in requiredItemNames)
        {
            if (!inventorySystem.HasItem(itemName))
            {
                hasAllParts = false;
                break;
            }
        }
        
        if (hasAllParts)
        {
            // Activate "Repair Elevator" objective first
            if (objectiveSystem != null)
            {
                objectiveSystem.ActivateObjective("activate_elevator");
            }
            
            ActivateElevator();
        }
        
        UpdateStatusDisplay();
    }
    
    void ActivateElevator()
    {
        isActive = true;
        Debug.Log("[Elevator] All parts collected! Elevator is now active!");
        
        UpdateVisuals();
        UpdateStatusDisplay();
        
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }
        
        // Complete elevator activation objective and activate escape objective
        if (objectiveSystem != null)
        {
            objectiveSystem.CompleteObjective("activate_elevator");
            objectiveSystem.ActivateObjective("escape");
        }
        
        // Open elevator door animation
        if (elevatorAnimator != null && !string.IsNullOrEmpty(doorOpenTrigger))
        {
            elevatorAnimator.SetTrigger(doorOpenTrigger);
        }
        
        if (audioSource != null && doorOpenSound != null)
        {
            audioSource.PlayOneShot(doorOpenSound, 0.7f);
        }
    }
    
    void UpdateVisuals()
    {
        if (inactiveDoor != null)
        {
            inactiveDoor.SetActive(!isActive);
        }
        
        if (activeDoor != null)
        {
            activeDoor.SetActive(isActive);
        }
        
        if (elevatorLight != null)
        {
            elevatorLight.color = isActive ? activeColor : inactiveColor;
        }
    }
    
    void UpdateStatusDisplay()
    {
        if (statusText == null) return;
        
        if (isActive)
        {
            statusText.text = "ELEVATOR READY\n<color=green>● OPERATIONAL</color>";
        }
        else
        {
            string status = "ELEVATOR OFFLINE\n<color=red>● MISSING PARTS:</color>\n\n";
            
            if (inventorySystem != null)
            {
                foreach (string itemName in requiredItemNames)
                {
                    bool hasItem = inventorySystem.HasItem(itemName);
                    string checkmark = hasItem ? "<color=green>✓</color>" : "<color=red>✗</color>";
                    status += $"{checkmark} {itemName}\n";
                }
            }
            else
            {
                status += "System Error";
            }
            
            statusText.text = status;
        }
    }
    
    void UpdatePrompt()
    {
        if (promptText == null) return;
        
        if (isActive)
        {
            promptText.text = "E - Enter Elevator (ESCAPE!)";
        }
        else
        {
            int collectedCount = 0;
            int totalCount = requiredItemNames.Length;
            
            if (inventorySystem != null)
            {
                foreach (string itemName in requiredItemNames)
                {
                    if (inventorySystem.HasItem(itemName))
                    {
                        collectedCount++;
                    }
                }
            }
            
            promptText.text = $"Elevator Inactive - Missing Parts ({collectedCount}/{totalCount})";
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
    
    void EnterElevator()
    {
        if (isEscaping || !isActive) return;
        
        isEscaping = true;
        Debug.Log("[Elevator] Player entered elevator! Starting escape sequence...");
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        // Complete escape objective
        if (objectiveSystem != null)
        {
            objectiveSystem.CompleteObjective("escape");
        }
        
        // Play elevator start sound
        if (audioSource != null && elevatorStartSound != null)
        {
            audioSource.PlayOneShot(elevatorStartSound);
        }
        
        // Start elevator animation
        if (elevatorAnimator != null && !string.IsNullOrEmpty(elevatorStartTrigger))
        {
            elevatorAnimator.SetTrigger(elevatorStartTrigger);
        }
        
        // Play moving sound in loop
        if (audioSource != null && elevatorMovingSound != null)
        {
            audioSource.clip = elevatorMovingSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        // Disable player movement
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.enabled = false;
        }
        
        // Trigger victory after delay
        Invoke(nameof(TriggerVictory), escapeDelay);
    }
    
    void TriggerVictory()
    {
        Debug.Log("[Elevator] Escape successful!");
        
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        
        if (gameManager != null)
        {
            gameManager.TriggerVictory();
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = isActive ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
