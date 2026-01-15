using UnityEngine;

public enum PickupType
{
    Battery,
    HealthPack,
    Key,
    QuestItem,
    Collectible,
    // Elevator Escape Items (5 parts)
    Nails,
    Keycard,
    Screwdriver,
    ElevatorButton,
    ElevatorCallButton
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
    
    [Header("Glow Effect")]
    [SerializeField] private bool enableGlow = true;
    [SerializeField] private Color glowColor = new Color(1f, 0.8f, 0.3f); // Warm yellow/orange
    [SerializeField] private float glowIntensity = 2f;
    [SerializeField] private float glowSpeed = 2f;
    [SerializeField] private float minGlowIntensity = 0.5f;
    [SerializeField] private float maxGlowIntensity = 3f;
    [SerializeField] private bool addPointLight = true;
    [SerializeField] private float lightRange = 3f;
    [SerializeField] private float lightIntensity = 1f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pickupSound;
    
    [Header("References")]
    [SerializeField] private Flashlight flashlight;
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private SanitySystem sanitySystem;
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
    
    // Glow effect variables
    private Renderer itemRenderer;
    private Material glowMaterial;
    private Light pointLight;
    private float currentGlowIntensity;
    private bool glowIncreasing = true;
    
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
        
        if (sanitySystem == null)
        {
            sanitySystem = FindFirstObjectByType<SanitySystem>();
        }
        
        if (inventorySystem == null)
        {
            inventorySystem = FindFirstObjectByType<InventorySystem>();
        }
        
        // Setup glow effect
        if (enableGlow)
        {
            SetupGlowEffect();
        }
    }
    
    void SetupGlowEffect()
    {
        // Get or create renderer
        itemRenderer = GetComponentInChildren<Renderer>();
        if (itemRenderer == null)
        {
            itemRenderer = GetComponent<Renderer>();
        }
        
        if (itemRenderer != null)
        {
            // Create a copy of the material to avoid modifying the original
            glowMaterial = new Material(itemRenderer.material);
            itemRenderer.material = glowMaterial;
            
            // Enable emission
            glowMaterial.EnableKeyword("_EMISSION");
            
            // Set initial glow
            currentGlowIntensity = minGlowIntensity;
            UpdateGlowEmission();
        }
        else
        {
            Debug.LogWarning($"[PickupItem] No Renderer found on {gameObject.name}. Glow effect disabled.");
            enableGlow = false;
        }
        
        // Add point light if enabled
        if (addPointLight)
        {
            pointLight = GetComponentInChildren<Light>();
            if (pointLight == null)
            {
                GameObject lightObj = new GameObject("PickupLight");
                lightObj.transform.SetParent(transform);
                lightObj.transform.localPosition = Vector3.zero;
                
                pointLight = lightObj.AddComponent<Light>();
                pointLight.type = LightType.Point;
                pointLight.range = lightRange;
                pointLight.intensity = lightIntensity;
                pointLight.color = glowColor;
                pointLight.renderMode = LightRenderMode.ForcePixel;
            }
        }
    }
    
    void UpdateGlowEmission()
    {
        if (glowMaterial == null) return;
        
        Color finalColor = glowColor * Mathf.LinearToGammaSpace(currentGlowIntensity);
        glowMaterial.SetColor("_EmissionColor", finalColor);
        
        // Update point light intensity if it exists
        if (pointLight != null)
        {
            pointLight.intensity = lightIntensity * (currentGlowIntensity / maxGlowIntensity);
        }
    }
    
    void Update()
    {
        if (isPickedUp) return;
        
        // Rotation animation
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // Bob animation
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
        // Glow animation
        if (enableGlow && glowMaterial != null)
        {
            UpdateGlowAnimation();
        }
    }
    
    void UpdateGlowAnimation()
    {
        // Pulse the glow intensity
        if (glowIncreasing)
        {
            currentGlowIntensity += glowSpeed * Time.deltaTime;
            if (currentGlowIntensity >= maxGlowIntensity)
            {
                currentGlowIntensity = maxGlowIntensity;
                glowIncreasing = false;
            }
        }
        else
        {
            currentGlowIntensity -= glowSpeed * Time.deltaTime;
            if (currentGlowIntensity <= minGlowIntensity)
            {
                currentGlowIntensity = minGlowIntensity;
                glowIncreasing = true;
            }
        }
        
        UpdateGlowEmission();
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
                case PickupType.QuestItem:
                    itemType = ItemType.QuestItem;
                    isUsable = false;
                    break;
                case PickupType.Collectible:
                    itemType = ItemType.Misc;
                    isUsable = false;
                    break;
                case PickupType.Nails:
                    itemType = ItemType.Nails;
                    isUsable = false;
                    break;
                case PickupType.Keycard:
                    itemType = ItemType.Keycard;
                    isUsable = false;
                    break;
                case PickupType.Screwdriver:
                    itemType = ItemType.Screwdriver;
                    isUsable = false;
                    break;
                case PickupType.ElevatorButton:
                    itemType = ItemType.ElevatorButton;
                    isUsable = false;
                    break;
                case PickupType.ElevatorCallButton:
                    itemType = ItemType.ElevatorCallButton;
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
                
                // Health pack also restores sanity
                if (sanitySystem != null)
                {
                    float sanityAmount = value * 0.5f; // Health pack restores 50% of its value as sanity
                    sanitySystem.RestoreSanity(sanityAmount);
                    Debug.Log($"Sanity restored: +{sanityAmount}");
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
                            
                            // Activate 'unlock_door' objective when Hidden Room Key is collected
                            if (itemName.Contains("Hidden Room") || itemName.Contains("hidden room"))
                            {
                                objectiveSystem.ActivateObjective("unlock_door");
                                Debug.Log("[PickupItem] Hidden Room Key collected! Activated 'unlock_door' objective.");
                            }
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
                
                case PickupType.QuestItem:
                if (inventorySystem != null)
                {
                    bool added = inventorySystem.AddItem(itemName, itemDescription, itemIcon, ItemType.QuestItem, 1, false);
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
                        Debug.LogWarning("Inventory full! Quest item cannot be picked up.");
                    }
                }
                else
                {
                    Debug.LogWarning("Inventory System not found! Quest item cannot be picked up.");
                }
                break;
                
                case PickupType.Collectible:
                if (inventorySystem != null)
                {
                    bool added = inventorySystem.AddItem(itemName, itemDescription, itemIcon, ItemType.Misc, 1, false);
                    if (added)
                    {
                        Debug.Log($"{itemName} collected!");
                    }
                    else
                    {
                        Debug.LogWarning("Inventory full! Collectible cannot be picked up.");
                    }
                }
                break;
        }
    }
    
    void PlayPickupSound()
    {
        // Try AudioManager first for better sound management
        AudioManager audioManager = AudioManager.Instance;
        if (audioManager != null)
        {
            audioManager.PlayItemPickupSound(audioSource);
        }
        else if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
    
    void OnDestroy()
    {
        // Clean up the duplicated material to avoid memory leaks
        if (glowMaterial != null)
        {
            Destroy(glowMaterial);
        }
    }
}

