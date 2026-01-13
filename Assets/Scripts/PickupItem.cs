using UnityEngine;

public enum PickupType
{
    Battery,
    HealthPack,
    Key
}

public class PickupItem : MonoBehaviour, IInteractable
{
    [Header("Item Settings")]
    public PickupType pickupType = PickupType.Battery;
    [SerializeField] private float value = 50f;
    
    [Header("Visual Effects")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobAmount = 0.2f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pickupSound;
    
    [Header("References")]
    [SerializeField] private Flashlight flashlight;
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private InventorySystem inventorySystem;
    
    [Header("Inventory Item")]
    public bool addToInventory = true;
    [SerializeField] private string itemName = "Item";
    [SerializeField] private string itemDescription = "A useful item";
    [SerializeField] private Sprite itemIcon;
    
    [Header("UI Feedback")]
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private float pickupEffectDuration = 1f;
    
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
        
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    public void Interact()
    {
        if (isPickedUp) return;
        
        if (addToInventory)
        {
            InventorySystem inventory = FindFirstObjectByType<InventorySystem>();
            if (inventory != null)
            {
                if (inventory.IsInventoryFull())
                {
                    Debug.LogWarning("Inventory full! Cannot pickup item.");
                    if (audioSource != null && pickupSound != null)
                    {
                        audioSource.PlayOneShot(pickupSound);
                    }
                    return;
                }
            }
        }
        
        ApplyPickup();
        PlayPickupSound();
        
        if (pickupEffect != null)
        {
            GameObject effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
            Destroy(effect, pickupEffectDuration);
        }
        
        Destroy(gameObject, 0.1f);
        isPickedUp = true;
    }
    
    void ApplyPickup()
    {
        PlayerSoundController soundController = FindFirstObjectByType<PlayerSoundController>();
        if (soundController != null)
        {
            soundController.EmitItemPickupSound(0.3f);
        }
        
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
                Debug.Log($"{itemName} added to inventory!");
                
                ObjectiveSystem objectiveSystem = ObjectiveSystem.Instance;
                if (objectiveSystem != null)
                {
                    objectiveSystem.OnItemCollected(itemName);
                }
                
                return;
            }
        }
        
        switch (pickupType)
        {
            case PickupType.Battery:
                if (flashlight != null)
                {
                    flashlight.RechargeBattery(value);
                    Debug.Log($"Battery recharged: +{value}%");
                }
                break;
                
            case PickupType.HealthPack:
                if (healthSystem != null)
                {
                    healthSystem.Heal(value);
                    Debug.Log($"Health restored: +{value}");
                }
                break;
                
            case PickupType.Key:
                if (inventorySystem != null)
                {
                    bool added = inventorySystem.AddItem(itemName, itemDescription, itemIcon, ItemType.Key, 1, false);
                    if (added)
                    {
                        Debug.Log($"{itemName} added to inventory!");
                        
                        ObjectiveSystem objectiveSystem = ObjectiveSystem.Instance;
                        if (objectiveSystem != null)
                        {
                            objectiveSystem.OnItemCollected(itemName);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Inventory full! Key cannot be picked up.");
                    }
                }
                else
                {
                    Debug.LogWarning("Inventory System not found! Key cannot be picked up.");
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

