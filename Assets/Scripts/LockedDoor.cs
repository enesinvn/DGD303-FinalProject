using UnityEngine;

public class LockedDoor : MonoBehaviour, IInteractable
{
    [Header("Door Reference")]
    [SerializeField] private Door door;
    
    [Header("Lock System")]
    [SerializeField] private LockSystem lockSystem;
    
    [Header("UI")]
    [SerializeField] private GameObject lockPrompt;
    
    void Start()
    {
        if (door == null)
        {
            door = GetComponent<Door>();
        }
        
        if (lockSystem == null)
        {
            lockSystem = GetComponent<LockSystem>();
        }
        
        if (lockPrompt != null)
        {
            lockPrompt.SetActive(false);
        }
    }
    
    public void Interact()
    {
        if (lockSystem == null)
        {
            if (door != null)
            {
                door.Interact();
            }
            return;
        }
        
        if (lockSystem.IsLocked())
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
                    if (unlocked && door != null)
                    {
                        door.Interact();
                    }
                }
                else
                {
                    Debug.Log($"Door is locked! Required key: {requiredKey}");
                }
            }
            else
            {
                Debug.Log("Door is locked! Key required.");
            }
        }
        else
        {
            if (door != null)
            {
                door.Interact();
            }
        }
    }
}

