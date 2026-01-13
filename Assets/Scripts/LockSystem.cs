using UnityEngine;

public class LockSystem : MonoBehaviour
{
    [Header("Lock Settings")]
    [SerializeField] private bool isLocked = true;
    [SerializeField] private string requiredKeyName = "";
    [SerializeField] private bool requiresKey = true;
    
    [Header("Lockpick Minigame (Optional)")]
    [SerializeField] private bool useMinigame = false;
    [SerializeField] private float lockpickDifficulty = 0.5f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip lockpickFailSound;
    
    [Header("References")]
    [SerializeField] private Door door;
    [SerializeField] private InventorySystem inventorySystem;
    
    private bool isUnlocking = false;
    
    void Start()
    {
        if (door == null)
        {
            door = GetComponent<Door>();
        }
        
        if (inventorySystem == null)
        {
            inventorySystem = FindFirstObjectByType<InventorySystem>();
        }
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.maxDistance = 10f;
        }
    }
    
    public bool TryUnlock(string keyName = "")
    {
        if (!isLocked)
        {
            return true;
        }
        
        if (isUnlocking)
        {
            return false;
        }
        
        if (requiresKey)
        {
            if (inventorySystem == null)
            {
                Debug.LogWarning("Inventory System not found!");
                return false;
            }
            
            bool hasKey = false;
            
            if (string.IsNullOrEmpty(requiredKeyName))
            {
                hasKey = inventorySystem.HasItem("Key") || 
                        inventorySystem.HasItem("Master Key") ||
                        inventorySystem.HasItem("Keycard");
            }
            else
            {
                hasKey = inventorySystem.HasItem(requiredKeyName);
            }
            
            if (!hasKey)
            {
                Debug.Log($"Required key to unlock: {requiredKeyName}");
                return false;
            }
        }
        
        if (useMinigame)
        {
            return TryLockpick();
        }
        
        Unlock();
        return true;
    }
    
    bool TryLockpick()
    {
        float successChance = 1f - lockpickDifficulty;
        successChance = Mathf.Clamp01(successChance);
        
        if (Random.value <= successChance)
        {
            Unlock();
            return true;
        }
        else
        {
            if (audioSource != null && lockpickFailSound != null)
            {
                audioSource.PlayOneShot(lockpickFailSound);
            }
            Debug.Log("Lockpick failed! Try again.");
            return false;
        }
    }
    
    void Unlock()
    {
        isLocked = false;
        isUnlocking = false;
        
        if (door != null)
        {
            door.Unlock();
        }
        
        if (audioSource != null && unlockSound != null)
        {
            audioSource.PlayOneShot(unlockSound);
        }
        
        ObjectiveSystem objectiveSystem = ObjectiveSystem.Instance;
        if (objectiveSystem != null)
        {
            string doorID = gameObject.name;
            objectiveSystem.OnDoorUnlocked(doorID);
        }
        
        Debug.Log("Lock unlocked!");
    }
    
    public void Lock()
    {
        isLocked = true;
        
        if (door != null)
        {
            door.Lock();
        }
        
        Debug.Log("Locked!");
    }
    
    public bool IsLocked()
    {
        return isLocked;
    }
    
    public string GetRequiredKeyName()
    {
        return requiredKeyName;
    }
}

