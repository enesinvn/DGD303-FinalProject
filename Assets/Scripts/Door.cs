using UnityEngine;
using UnityEngine.AI;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [SerializeField] private bool isLocked = false;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private bool slideMode = false; 
    [SerializeField] private Vector3 slideDirection = new Vector3(0, 0, 1);
    [SerializeField] private float slideDistance = 2f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip lockedSound;
    
    [Header("Lock System (Optional)")]
    [SerializeField] private LockSystem lockSystem;
    
    [Header("Objectives (Optional)")]
    [SerializeField] private bool triggerObjectivesOnUnlock = false;
    [SerializeField] private string unlockObjectiveID = "unlock_door";
    [SerializeField] private string[] objectivesToActivateOnUnlock = new string[0];
    
    [Header("NavMesh Settings")]
    [SerializeField] private bool useNavMeshObstacle = true;
    [SerializeField] private bool autoSetupNavMesh = true;
    
    [Header("Collision Settings")]
    [SerializeField] private bool disableColliderWhenOpen = true;
    
    [Header("NPC Door Interaction")]
    [SerializeField] private bool npcCanOpen = true;
    [SerializeField] private bool onlyOpenOnChase = true;
    [SerializeField] private float autoCloseDelay = 3f;
    [SerializeField] private float npcDetectionRadius = 2.5f;
    
    [SerializeField] private bool isOpen = false;
    private bool isMoving = false;
    private NavMeshObstacle navMeshObstacle;
    private Collider doorCollider;
    private Coroutine autoCloseCoroutine;
    private bool openedByNPC = false;
    private bool soundPlayed = false;
    private float soundTriggerProgress = 0.15f; // Ses %15 açılınca çalar
    
    public bool IsOpen { get { return isOpen; } }
    public bool IsLocked 
    { 
        get 
        { 
            if (lockSystem != null)
            {
                return lockSystem.IsLocked();
            }
            return isLocked; 
        } 
    }
    
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    
    void Start()
    {
        // First check if AudioSource exists on GameObject
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        // If still null, create new one
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.maxDistance = 10f;
        }
        
        if (lockSystem == null)
        {
            lockSystem = GetComponent<LockSystem>();
        }
        
        // Get collider and make sure it's not a trigger
        doorCollider = GetComponent<Collider>();
        if (doorCollider != null)
        {
            doorCollider.isTrigger = false;
            Debug.Log($"[Door] {gameObject.name} - Collider found and set to non-trigger");
        }
        else
        {
            Debug.LogWarning($"[Door] {gameObject.name} - No Collider found! Door won't block player. Please add a BoxCollider.");
        }
        
        if (useNavMeshObstacle)
        {
            SetupNavMeshObstacle();
        }
        
        if (slideMode)
        {
            closedPosition = transform.position;
            openPosition = closedPosition + slideDirection.normalized * slideDistance;
        }
        else
        {
            closedRotation = transform.rotation;
            openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
        }
        
        UpdateNavMeshObstacle();
    }
    
    void Update()
    {
        if (npcCanOpen && !isOpen && !isLocked)
        {
            CheckForNearbyNPC();
        }
        
        if (!isMoving) return;
        
        if (slideMode)
        {
            Vector3 targetPosition = isOpen ? openPosition : closedPosition;
            Vector3 startPosition = isOpen ? closedPosition : openPosition;
            
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * openSpeed);
            
            // Calculate progress and play sound
            float progress = CalculateSlideProgress(startPosition, targetPosition);
            CheckAndPlaySound(progress);
            
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
                soundPlayed = false; // Reset for next movement
                UpdateNavMeshObstacle();
            }
        }
        else
        {
            Quaternion targetRotation = isOpen ? openRotation : closedRotation;
            Quaternion startRotation = isOpen ? closedRotation : openRotation;
            
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
            
            // Calculate progress and play sound
            float progress = CalculateRotationProgress(startRotation, targetRotation);
            CheckAndPlaySound(progress);
            
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.rotation = targetRotation;
                isMoving = false;
                soundPlayed = false; // Reset for next movement
                UpdateNavMeshObstacle();
            }
        }
    }
    
    float CalculateSlideProgress(Vector3 start, Vector3 target)
    {
        float totalDistance = Vector3.Distance(start, target);
        if (totalDistance < 0.01f) return 1f;
        
        float currentDistance = Vector3.Distance(start, transform.position);
        return Mathf.Clamp01(currentDistance / totalDistance);
    }
    
    float CalculateRotationProgress(Quaternion start, Quaternion target)
    {
        float totalAngle = Quaternion.Angle(start, target);
        if (totalAngle < 0.1f) return 1f;
        
        float currentAngle = Quaternion.Angle(start, transform.rotation);
        return Mathf.Clamp01(currentAngle / totalAngle);
    }
    
    void CheckAndPlaySound(float progress)
    {
        if (!soundPlayed && progress >= soundTriggerProgress)
        {
            PlaySound(isOpen ? openSound : closeSound);
            EmitDoorSound(isOpen ? 0.6f : 0.5f);
            soundPlayed = true;
        }
    }
    
    void CheckForNearbyNPC()
    {
        if (IsLocked)
        {
            return;
        }
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, npcDetectionRadius);
        
        foreach (Collider col in colliders)
        {
            EnemyAI enemy = col.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                bool shouldOpen = false;
                
                if (onlyOpenOnChase)
                {
                    string currentState = enemy.GetCurrentStateName();
                    Debug.Log($"[Door] {name} - NPC found! State: {currentState}, Distance: {Vector3.Distance(transform.position, col.transform.position):F2}m");
                    
                    if (currentState == "Chase" || currentState == "Attack" || currentState == "Search")
                    {
                        shouldOpen = true;
                        Debug.Log($"[Door] {name} - NPC in {currentState} mode, door will open!");
                    }
                    else
                    {
                        Debug.Log($"[Door] {name} - NPC in {currentState} mode, door won't open (Only Open On Chase active)");
                    }
                }
                else
                {
                    shouldOpen = true;
                    Debug.Log($"[Door] {name} - NPC found, door will open (Only Open On Chase inactive)");
                }
                
                if (shouldOpen)
                {
                    OpenForNPC();
                    return;
                }
            }
        }
    }
    
    void OpenForNPC()
    {
        if (isMoving || isOpen) return;
        
        isOpen = true;
        isMoving = true;
        openedByNPC = true;
        soundPlayed = false; // Reset sound flag for new movement
        
        UpdateNavMeshObstacle();
        // Sound will play automatically in Update() when door reaches soundTriggerProgress
        
        Debug.Log($"[Door] {gameObject.name} opened by NPC");
        
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
        }
        autoCloseCoroutine = StartCoroutine(AutoCloseAfterDelay());
    }
    
    System.Collections.IEnumerator AutoCloseAfterDelay()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, npcDetectionRadius);
        bool npcNearby = false;
        
        foreach (Collider col in colliders)
        {
            if (col.GetComponent<EnemyAI>() != null)
            {
                npcNearby = true;
                break;
            }
        }
        
        if (npcNearby)
        {
            yield return new WaitForSeconds(1f);
        }
        
        if (isOpen && openedByNPC && !isMoving)
        {
            isOpen = false;
            isMoving = true;
            openedByNPC = false;
            soundPlayed = false; // Reset sound flag for closing
            
            UpdateNavMeshObstacle();
            // Sound will play automatically in Update() when door reaches soundTriggerProgress
            
            Debug.Log($"[Door] {gameObject.name} auto-closed");
        }
    }
    
    void SetupNavMeshObstacle()
    {
        navMeshObstacle = GetComponent<NavMeshObstacle>();
        
        if (navMeshObstacle == null && autoSetupNavMesh)
        {
            navMeshObstacle = gameObject.AddComponent<NavMeshObstacle>();
            
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                if (col is BoxCollider)
                {
                    BoxCollider boxCol = col as BoxCollider;
                    navMeshObstacle.size = boxCol.size;
                    navMeshObstacle.center = boxCol.center;
                }
                else if (col is CapsuleCollider)
                {
                    CapsuleCollider capsuleCol = col as CapsuleCollider;
                    navMeshObstacle.radius = capsuleCol.radius;
                    navMeshObstacle.height = capsuleCol.height;
                    navMeshObstacle.center = capsuleCol.center;
                }
            }
            else
            {
                navMeshObstacle.size = new Vector3(1f, 2f, 0.1f);
            }
            
                navMeshObstacle.shape = NavMeshObstacleShape.Box;
            Debug.Log($"[Door] NavMeshObstacle added and configured for {gameObject.name}.");
        }
    }
    
    void UpdateNavMeshObstacle()
    {
        if (navMeshObstacle != null && useNavMeshObstacle)
        {
            navMeshObstacle.enabled = !isOpen;
        }
        
        // Kapı açıkken collider'ı kapat, kapalıyken aç
        if (doorCollider != null && disableColliderWhenOpen)
        {
            doorCollider.enabled = !isOpen;
        }
        
        if (isOpen)
        {
            Debug.Log($"[Door] {gameObject.name} open - Collider disabled (Player can pass)");
        }
        else
        {
            Debug.Log($"[Door] {gameObject.name} closed - Collider enabled (Player blocked)");
        }
    }
    
    public void Interact()
    {
        if (lockSystem != null && lockSystem.IsLocked())
        {
            InventorySystem inventory = FindFirstObjectByType<InventorySystem>();
            if (inventory != null)
            {
                string requiredKey = lockSystem.GetRequiredKeyName();
                bool hasKey = string.IsNullOrEmpty(requiredKey) ? 
                    (inventory.HasItem("Key") || inventory.HasItem("Master Key")) : 
                    inventory.HasItem(requiredKey);
                
                if (hasKey)
                {
                    bool unlocked = lockSystem.TryUnlock(requiredKey);
                    if (unlocked)
                    {
                        isLocked = false;
                        
                        // Trigger objectives when door is unlocked
                        if (triggerObjectivesOnUnlock)
                        {
                            ObjectiveSystem objSystem = ObjectiveSystem.Instance;
                            if (objSystem != null)
                            {
                                // Complete unlock objective
                                if (!string.IsNullOrEmpty(unlockObjectiveID))
                                {
                                    objSystem.CompleteObjective(unlockObjectiveID);
                                    Debug.Log($"[Door] Completed objective: {unlockObjectiveID}");
                                }
                                
                                // Activate follow-up objectives (elevator + parts)
                                foreach (string objID in objectivesToActivateOnUnlock)
                                {
                                    if (!string.IsNullOrEmpty(objID))
                                    {
                                        objSystem.ActivateObjective(objID);
                                        Debug.Log($"[Door] Activated objective: {objID}");
                                        
                                        // "find_elevator" görevini hemen tamamla (kapıyı açtıysan zaten elevator'ü buldun)
                                        if (objID == "find_elevator")
                                        {
                                            objSystem.CompleteObjective("find_elevator");
                                            Debug.Log($"[Door] Auto-completed 'find_elevator' objective");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        PlaySound(lockedSound);
                        AudioManager audioManager = AudioManager.Instance;
                        if (audioManager != null)
                        {
                            audioManager.PlayDoorLockSound(audioSource);
                        }
                        Debug.Log($"{gameObject.name} could not be unlocked!");
                        return;
                    }
                }
                else
                {
                    PlaySound(lockedSound);
                    AudioManager audioManager = AudioManager.Instance;
                    if (audioManager != null)
                    {
                        audioManager.PlayDoorLockSound(audioSource);
                    }
                    Debug.Log($"{gameObject.name} is locked! Required key: {requiredKey}");
                    return;
                }
            }
            else
            {
                PlaySound(lockedSound);
                AudioManager audioManager = AudioManager.Instance;
                if (audioManager != null)
                {
                    audioManager.PlayDoorLockSound(audioSource);
                }
                Debug.Log($"{gameObject.name} is locked! Key required.");
                return;
            }
        }
        else if (isLocked)
        {
            PlaySound(lockedSound);
            AudioManager audioManager = AudioManager.Instance;
            if (audioManager != null)
            {
                audioManager.PlayDoorLockSound(audioSource);
            }
            Debug.Log($"{gameObject.name} is locked!");
            return;
        }
        
        if (isMoving) return;
        
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
        
        isOpen = !isOpen;
        isMoving = true;
        openedByNPC = false;
        soundPlayed = false; // Reset sound flag for new movement
        
        UpdateNavMeshObstacle();
        // Sound will play automatically in Update() when door reaches soundTriggerProgress
        
        Debug.Log($"{gameObject.name} {(isOpen ? "opened" : "closed")} (Player)");
    }
    
    void EmitDoorSound(float strength)
    {
        SoundEmitter soundEmitter = GetComponent<SoundEmitter>();
        if (soundEmitter == null)
        {
            soundEmitter = gameObject.AddComponent<SoundEmitter>();
        }
        
        if (soundEmitter != null)
        {
            soundEmitter.soundType = isOpen ? SoundEmitter.SoundType.DoorOpen : SoundEmitter.SoundType.DoorClose;
            soundEmitter.soundRadius = 10f;
            soundEmitter.EmitSound(strength);
        }
        
        PlayerSoundController soundController = FindFirstObjectByType<PlayerSoundController>();
        if (soundController != null)
        {
            soundController.EmitInteractionSound(strength * 0.8f);
        }
    }
    
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    public void Unlock()
    {
        isLocked = false;
        Debug.Log($"{gameObject.name} unlocked!");
    }
    
    public void Lock()
    {
        isLocked = true;
        Debug.Log($"{gameObject.name} locked!");
    }
    
    void OnDrawGizmosSelected()
    {
        if (!npcCanOpen) return;
        
        Gizmos.color = onlyOpenOnChase ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, npcDetectionRadius);
    }
}