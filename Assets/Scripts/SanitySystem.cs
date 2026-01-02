using UnityEngine;

public class SanitySystem : MonoBehaviour
{
    [Header("Sanity Ayarları")]
    [SerializeField] private float maxSanity = 100f;
    [SerializeField] private float currentSanity;
    [SerializeField] private float sanityDrainRate = 0.5f; // Saniyede azalan
    [SerializeField] private float lowSanityThreshold = 30f;
    [SerializeField] private float criticalSanityThreshold = 15f;
    
    [Header("Drain Koşulları")]
    [SerializeField] private bool drainInDarkness = true;
    [SerializeField] private float darknessDrainRate = 1f;
    [SerializeField] private bool drainNearEnemy = true;
    [SerializeField] private float enemyProximityDrainRate = 2f;
    [SerializeField] private float enemyProximityRange = 10f;
    
    [Header("Efektler")]
    [SerializeField] private bool enableVisualEffects = true;
    [SerializeField] private bool enableAudioEffects = true;
    
    [Header("Post Processing")]
    [SerializeField] private PostProcessingController postProcessingController;
    
    [Header("Referanslar")]
    [SerializeField] private Flashlight flashlight;
    [SerializeField] private LayerMask enemyLayer;
    
    [Header("UI")]
    [SerializeField] private GameObject sanityUI;
    
    private bool isLowSanity = false;
    private bool isCriticalSanity = false;
    
    public delegate void SanityChangedDelegate(float currentSanity, float maxSanity);
    public event SanityChangedDelegate OnSanityChanged;
    
    public delegate void LowSanityDelegate();
    public event LowSanityDelegate OnLowSanity;
    
    public delegate void CriticalSanityDelegate();
    public event CriticalSanityDelegate OnCriticalSanity;
    
    void Start()
    {
        currentSanity = maxSanity;
        
        if (flashlight == null)
        {
            flashlight = FindFirstObjectByType<Flashlight>();
        }
        
        if (postProcessingController == null)
        {
            postProcessingController = FindFirstObjectByType<PostProcessingController>();
        }
    }
    
    void Update()
    {
        HandleSanityDrain();
        CheckSanityLevels();
        ApplySanityEffects();
    }
    
    void HandleSanityDrain()
    {
        float drainAmount = 0f;
        
        // Karanlıkta azalma
        if (drainInDarkness)
        {
            bool isInDarkness = flashlight == null || !flashlight.IsOn();
            if (isInDarkness)
            {
                drainAmount += darknessDrainRate * Time.deltaTime;
            }
        }
        
        // NPC yakınında azalma
        if (drainNearEnemy)
        {
            Collider[] enemies = Physics.OverlapSphere(transform.position, enemyProximityRange, enemyLayer);
            if (enemies.Length > 0)
            {
                drainAmount += enemyProximityDrainRate * Time.deltaTime;
            }
        }
        
        // Genel azalma
        drainAmount += sanityDrainRate * Time.deltaTime;
        
        // Sanity azalt
        if (drainAmount > 0f)
        {
            ReduceSanity(drainAmount);
        }
    }
    
    void CheckSanityLevels()
    {
        float sanityPercent = (currentSanity / maxSanity) * 100f;
        
        if (sanityPercent <= criticalSanityThreshold && !isCriticalSanity)
        {
            isCriticalSanity = true;
            OnCriticalSanity?.Invoke();
            Debug.LogWarning("Kritik sanity seviyesi!");
        }
        else if (sanityPercent > criticalSanityThreshold)
        {
            isCriticalSanity = false;
        }
        
        if (sanityPercent <= lowSanityThreshold && !isLowSanity)
        {
            isLowSanity = true;
            OnLowSanity?.Invoke();
            Debug.Log("Düşük sanity seviyesi!");
        }
        else if (sanityPercent > lowSanityThreshold)
        {
            isLowSanity = false;
        }
    }
    
    void ApplySanityEffects()
    {
        if (!enableVisualEffects && !enableAudioEffects) return;
        
        float sanityPercent = (currentSanity / maxSanity) * 100f;
        
        // Post-processing efektleri (ileride eklenebilir)
        if (postProcessingController != null && enableVisualEffects)
        {
            // Vignette, color adjustments vb. eklenebilir
        }
    }
    
    public void ReduceSanity(float amount)
    {
        currentSanity -= amount;
        currentSanity = Mathf.Max(0f, currentSanity);
        
        OnSanityChanged?.Invoke(currentSanity, maxSanity);
    }
    
    public void RestoreSanity(float amount)
    {
        currentSanity += amount;
        currentSanity = Mathf.Min(maxSanity, currentSanity);
        
        OnSanityChanged?.Invoke(currentSanity, maxSanity);
    }
    
    public float GetSanityPercentage()
    {
        return (currentSanity / maxSanity) * 100f;
    }
    
    public float GetCurrentSanity()
    {
        return currentSanity;
    }
    
    public bool IsLowSanity()
    {
        return isLowSanity;
    }
    
    public bool IsCriticalSanity()
    {
        return isCriticalSanity;
    }
}

