using UnityEngine;

public enum PickupType
{
    Battery,
    HealthPack,
    Key
}

public class PickupItem : MonoBehaviour, IInteractable
{
    [Header("Eşya Ayarları")]
    [SerializeField] private PickupType pickupType = PickupType.Battery;
    [SerializeField] private float value = 50f; // Batarya için %, Sağlık için miktar
    
    [Header("Görsel Efektler")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobAmount = 0.2f;
    
    [Header("Ses")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pickupSound;
    
    [Header("Referanslar")]
    [SerializeField] private Flashlight flashlight;
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private InventorySystem inventorySystem;
    
    [Header("Envanter Eşyası")]
    [SerializeField] private bool addToInventory = false; // Envantere eklensin mi?
    [SerializeField] private string itemName = "Item";
    [SerializeField] private string itemDescription = "A useful item";
    [SerializeField] private Sprite itemIcon;
    
    private Vector3 startPosition;
    private bool isPickedUp = false;
    
    void Start()
    {
        startPosition = transform.position;
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.maxDistance = 10f;
        }
        
        // Referansları bul
        if (flashlight == null)
        {
            flashlight = FindFirstObjectByType<Flashlight>();
        }
        
        if (healthSystem == null)
        {
            healthSystem = FindFirstObjectByType<HealthSystem>();
        }
        
        if (inventorySystem == null)
        {
            inventorySystem = FindFirstObjectByType<InventorySystem>();
        }
    }
    
    void Update()
    {
        if (isPickedUp) return;
        
        // Döndürme animasyonu
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // Yukarı-aşağı hareket
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    public void Interact()
    {
        if (isPickedUp) return;
        
        ApplyPickup();
        PlayPickupSound();
        Destroy(gameObject, 0.1f); // Ses çalması için kısa bir gecikme
        isPickedUp = true;
    }
    
    void ApplyPickup()
    {
        // Ses çıkar
        PlayerSoundController soundController = FindFirstObjectByType<PlayerSoundController>();
        if (soundController != null)
        {
            soundController.EmitItemPickupSound(0.3f);
        }
        
        // Envantere ekle
        if (addToInventory && inventorySystem != null)
        {
            ItemType itemType = ItemType.Misc;
            bool isUsable = false;
            
            switch (pickupType)
            {
                case PickupType.Battery:
                    itemType = ItemType.Battery;
                    isUsable = true;
                    break;
                case PickupType.HealthPack:
                    itemType = ItemType.HealthPack;
                    isUsable = true;
                    break;
                case PickupType.Key:
                    itemType = ItemType.Key;
                    isUsable = false;
                    break;
            }
            
            bool added = inventorySystem.AddItem(itemName, itemDescription, itemIcon, itemType, 1, isUsable);
            if (added)
            {
                Debug.Log($"{itemName} envantere eklendi!");
                return; // Envantere eklendiyse direkt kullanma
            }
        }
        
        // Direkt kullan (envantere eklenmediyse)
        switch (pickupType)
        {
            case PickupType.Battery:
                if (flashlight != null)
                {
                    flashlight.RechargeBattery(value);
                    Debug.Log($"Batarya şarj edildi: +{value}%");
                }
                break;
                
            case PickupType.HealthPack:
                if (healthSystem != null)
                {
                    healthSystem.Heal(value);
                    Debug.Log($"Sağlık yenilendi: +{value}");
                }
                break;
                
            case PickupType.Key:
                // Envantere eklenmediyse direkt kullanılamaz
                if (inventorySystem != null)
                {
                    inventorySystem.AddItem(itemName, itemDescription, itemIcon, ItemType.Key, 1, false);
                    Debug.Log("Anahtar envantere eklendi!");
                }
                break;
        }
    }
    
    void PlayPickupSound()
    {
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}

