using UnityEngine;

public class LockedDoor : MonoBehaviour, IInteractable
{
    [Header("Kapı Referansı")]
    [SerializeField] private Door door;
    
    [Header("Kilit Sistemi")]
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
            // Kilit sistemi yoksa normal kapı gibi çalış
            if (door != null)
            {
                door.Interact();
            }
            return;
        }
        
        if (lockSystem.IsLocked())
        {
            // Kilitli - açmaya çalış
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
                    Debug.Log($"Kapı kilitli! Gerekli anahtar: {requiredKey}");
                }
            }
            else
            {
                Debug.Log("Kapı kilitli! Anahtar gerekli.");
            }
        }
        else
        {
            // Kilitli değil - normal kapı gibi çalış
            if (door != null)
            {
                door.Interact();
            }
        }
    }
}

