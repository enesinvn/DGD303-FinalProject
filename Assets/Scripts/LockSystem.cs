using UnityEngine;

public class LockSystem : MonoBehaviour
{
    [Header("Kilit Ayarları")]
    [SerializeField] private bool isLocked = true;
    [SerializeField] private string requiredKeyName = ""; // Gerekli anahtar adı (boşsa herhangi bir anahtar yeterli)
    [SerializeField] private bool requiresKey = true;
    
    [Header("Kilit Açma Mini-Oyunu (Opsiyonel)")]
    [SerializeField] private bool useMinigame = false;
    [SerializeField] private float lockpickDifficulty = 0.5f; // 0-1 arası (1 = çok zor)
    
    [Header("Ses")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip lockpickFailSound;
    
    [Header("Referanslar")]
    [SerializeField] private Door door; // Kilitli kapı
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
            return true; // Zaten açık
        }
        
        if (isUnlocking)
        {
            return false; // Zaten açılmaya çalışılıyor
        }
        
        // Anahtar kontrolü
        if (requiresKey)
        {
            if (inventorySystem == null)
            {
                Debug.LogWarning("Inventory System bulunamadı!");
                return false;
            }
            
            bool hasKey = false;
            
            if (string.IsNullOrEmpty(requiredKeyName))
            {
                // Herhangi bir anahtar yeterli
                hasKey = inventorySystem.HasItem("Key") || 
                        inventorySystem.HasItem("Master Key") ||
                        inventorySystem.HasItem("Keycard");
            }
            else
            {
                // Belirli bir anahtar gerekli
                hasKey = inventorySystem.HasItem(requiredKeyName);
            }
            
            if (!hasKey)
            {
                Debug.Log($"Kilit açmak için gerekli anahtar yok: {requiredKeyName}");
                return false;
            }
        }
        
        // Mini-oyun kullanılıyorsa
        if (useMinigame)
        {
            return TryLockpick();
        }
        
        // Direkt aç
        Unlock();
        return true;
    }
    
    bool TryLockpick()
    {
        // Basit bir şans sistemi (ileride mini-oyun eklenebilir)
        float successChance = 1f - lockpickDifficulty;
        successChance = Mathf.Clamp01(successChance);
        
        if (Random.value <= successChance)
        {
            Unlock();
            return true;
        }
        else
        {
            // Başarısız
            if (audioSource != null && lockpickFailSound != null)
            {
                audioSource.PlayOneShot(lockpickFailSound);
            }
            Debug.Log("Kilit açma başarısız! Tekrar deneyin.");
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
        
        Debug.Log("Kilit açıldı!");
    }
    
    public void Lock()
    {
        isLocked = true;
        
        if (door != null)
        {
            door.Lock();
        }
        
        Debug.Log("Kilitlendi!");
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

