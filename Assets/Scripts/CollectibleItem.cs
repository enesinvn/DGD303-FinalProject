using UnityEngine;

/// <summary>
/// Görev için olmayan, sadece toplanabilir objeler (örnek: koleksiyon parçaları, bonus itemler)
/// </summary>
public class CollectibleItem : MonoBehaviour, IInteractable
{
    [Header("Collectible Settings")]
    [SerializeField] private string collectibleName = "Collectible";
    [SerializeField] private string collectibleDescription = "A collectible item";
    [SerializeField] private Sprite collectibleIcon;
    [SerializeField] private int scoreValue = 10; // Toplandığında verilecek puan
    
    [Header("Visual Effects")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobAmount = 0.2f;
    [SerializeField] private GameObject glowEffect;
    [SerializeField] private Color glowColor = Color.cyan;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pickupSound;
    
    [Header("References")]
    [SerializeField] private InventorySystem inventorySystem;
    
    [Header("UI Feedback")]
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private float pickupEffectDuration = 1f;
    [SerializeField] private bool showNotification = true;
    
    private Vector3 startPosition;
    private bool isPickedUp = false;
    private static int totalCollected = 0;
    
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
        
        // Envantere ekle (opsiyonel - eğer inventorySystem null ise sadece toplanır)
        if (inventorySystem != null && !inventorySystem.IsInventoryFull())
        {
            inventorySystem.AddItem(collectibleName, collectibleDescription, collectibleIcon, ItemType.Misc, 1, false);
        }
        
        totalCollected++;
        
        PlayPickupSound();
        
        if (pickupEffect != null)
        {
            GameObject effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
            Destroy(effect, pickupEffectDuration);
        }
        
        PlayerSoundController soundController = FindFirstObjectByType<PlayerSoundController>();
        if (soundController != null)
        {
            soundController.EmitItemPickupSound(0.3f);
        }
        
        if (showNotification)
        {
            Debug.Log($"Collected: {collectibleName} (+{scoreValue} points) | Total: {totalCollected}");
        }
        
        // UI Manager'a bildir (eğer varsa)
        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null && showNotification)
        {
            // UIManager'da ShowNotification metodu varsa kullanılabilir
        }
        
        Destroy(gameObject, 0.1f);
        isPickedUp = true;
    }
    
    void PlayPickupSound()
    {
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
    }
    
    public static int GetTotalCollected()
    {
        return totalCollected;
    }
    
    public static void ResetCollectibleCount()
    {
        totalCollected = 0;
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = glowColor;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
