using UnityEngine;

/// <summary>
/// Represents required objects for objectives. The objective system is automatically updated when these objects are collected.
/// </summary>
public class QuestItemPickup : MonoBehaviour, IInteractable
{
    [Header("Quest Item Settings")]
    [SerializeField] private string questItemID; // Quest ID (e.g.: "collect_key", "collect_document")
    [SerializeField] private string itemName = "Quest Item";
    [SerializeField] private string itemDescription = "A quest item";
    [SerializeField] private Sprite itemIcon;
    
    [Header("Visual Effects")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobAmount = 0.2f;
    [SerializeField] private GameObject glowEffect;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pickupSound;
    
    [Header("References")]
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private ObjectiveSystem objectiveSystem;
    
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
        
        if (inventorySystem == null)
        {
            inventorySystem = FindFirstObjectByType<InventorySystem>();
        }
        
        if (objectiveSystem == null)
        {
            objectiveSystem = ObjectiveSystem.Instance;
        }
        
        if (glowEffect != null)
        {
            glowEffect.SetActive(true);
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
        
        if (inventorySystem == null)
        {
            Debug.LogWarning("Inventory System not found!");
            return;
        }
        
        if (inventorySystem.IsInventoryFull())
        {
            Debug.LogWarning("Inventory full! Cannot pickup quest item.");
            if (audioSource != null && pickupSound != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }
            return;
        }
        
        // Add to inventory
        bool added = inventorySystem.AddItem(itemName, itemDescription, itemIcon, ItemType.QuestItem, 1, false);
        
        if (added)
        {
            // Notify objective system
            if (objectiveSystem != null)
            {
                objectiveSystem.OnItemCollected(itemName);
            }
            
            PlayPickupSound();
            
            if (pickupEffect != null)
            {
                GameObject effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
                Destroy(effect, pickupEffectDuration);
            }
            
            PlayerSoundController soundController = FindFirstObjectByType<PlayerSoundController>();
            if (soundController != null)
            {
                soundController.EmitItemPickupSound(0.5f);
            }
            
            Debug.Log($"Quest item collected: {itemName}");
            
            Destroy(gameObject, 0.1f);
            isPickedUp = true;
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
