using UnityEngine;

public class DistractionItem : MonoBehaviour, IInteractable
{
    [Header("Dikkat Dağıtma Ayarları")]
    [SerializeField] private float distractionRadius = 15f;
    [SerializeField] private float distractionDuration = 5f; // NPC ne kadar süre oraya gider
    [SerializeField] private float soundIntensity = 1f;
    
    [Header("Fırlatma")]
    [SerializeField] private bool canThrow = true;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float throwUpwardForce = 2f;
    
    [Header("Ses")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip throwSound;
    [SerializeField] private AudioClip impactSound;
    
    [Header("Görsel")]
    [SerializeField] private GameObject impactEffect;
    
    [Header("Layer")]
    [SerializeField] private LayerMask enemyLayer;
    
    private Rigidbody rb;
    private bool isThrown = false;
    private Vector3 throwPosition;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (rb == null && canThrow)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 0.5f;
        }
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.maxDistance = 20f;
        }
    }
    
    public void Interact()
    {
        if (canThrow && !isThrown)
        {
            ThrowItem();
        }
        else
        {
            // Yerdeyse tekrar al
            PickupItem();
        }
    }
    
    void ThrowItem()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        Transform playerCamera = player.GetComponentInChildren<Camera>()?.transform;
        if (playerCamera == null) return;
        
        // Fırlat
        if (rb != null)
        {
            rb.isKinematic = false;
            Vector3 throwDirection = playerCamera.forward;
            rb.AddForce(throwDirection * throwForce + Vector3.up * throwUpwardForce, ForceMode.Impulse);
        }
        
        isThrown = true;
        throwPosition = transform.position;
        
        // Ses çıkar
        PlayerSoundController soundController = player.GetComponent<PlayerSoundController>();
        if (soundController != null)
        {
            soundController.EmitThrowSound(0.8f);
        }
        
        if (audioSource != null && throwSound != null)
        {
            audioSource.PlayOneShot(throwSound);
        }
        
        // Collider'ı etkinleştir (çarpışma için)
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = false;
        }
    }
    
    void PickupItem()
    {
        // Envantere ekle (ileride)
        InventorySystem inventory = FindFirstObjectByType<InventorySystem>();
        if (inventory != null)
        {
            // Envantere eklenebilir
        }
        
        Destroy(gameObject);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (isThrown && collision.contacts.Length > 0)
        {
            // Yere çarptı - dikkat dağıt
            CreateDistraction(collision.contacts[0].point);
        }
    }
    
    void CreateDistraction(Vector3 position)
    {
        // Ses çıkar
        if (audioSource != null && impactSound != null)
        {
            audioSource.PlayOneShot(impactSound);
        }
        
        // Görsel efekt
        if (impactEffect != null)
        {
            Instantiate(impactEffect, position, Quaternion.identity);
        }
        
        // NPC'lere ses sinyali gönder
        Collider[] enemies = Physics.OverlapSphere(position, distractionRadius, enemyLayer);
        
        foreach (Collider enemy in enemies)
        {
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                // NPC'yi ses kaynağına gönder
                float distance = Vector3.Distance(position, enemy.transform.position);
                float normalizedDistance = distance / distractionRadius;
                float soundStrength = (1f - normalizedDistance) * soundIntensity;
                
                enemyAI.OnSoundDetected(position, soundStrength, SoundEmitter.SoundType.Throw);
            }
        }
        
        // SoundEmitter ekle (sürekli ses için)
        SoundEmitter emitter = gameObject.AddComponent<SoundEmitter>();
        if (emitter != null)
        {
            emitter.soundRadius = distractionRadius;
            emitter.soundIntensity = soundIntensity;
            emitter.soundType = SoundEmitter.SoundType.Throw;
            emitter.soundDuration = distractionDuration;
            emitter.EmitSound(soundIntensity);
        }
        
        Debug.Log($"Dikkat dağıtıcı aktif: {distractionDuration} saniye");
        
        // Belirli süre sonra yok et
        Destroy(gameObject, distractionDuration + 1f);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, distractionRadius);
    }
}

