using UnityEngine;

public class SanitySystem : MonoBehaviour
{
    private static SanitySystem instance;
    public static SanitySystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<SanitySystem>();
            }
            return instance;
        }
    }
    
    [Header("Sanity Settings")]
    [SerializeField] private float maxSanity = 100f;
    [SerializeField] private float currentSanity;
    [SerializeField] private float sanityDrainRate = 0.5f;
    [SerializeField] private float lowSanityThreshold = 30f;
    [SerializeField] private float criticalSanityThreshold = 15f;
    
    [Header("Drain Conditions")]
    [SerializeField] private bool drainInDarkness = true;
    [SerializeField] private float darknessDrainRate = 1f;
    [SerializeField] private bool drainNearEnemy = true;
    [SerializeField] private float enemyProximityDrainRate = 2f;
    [SerializeField] private float enemyProximityRange = 10f;
    
    [Header("Effects")]
    [SerializeField] private bool enableAudioEffects = true;
    
    [Header("Post Processing")]
    [SerializeField] private PostProcessingController postProcessingController;
    
    [Header("References")]
    [SerializeField] private Flashlight flashlight;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Transform playerTransform;
    
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
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        currentSanity = maxSanity;
        
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
        
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
        
        if (drainInDarkness)
        {
            bool isInDarkness = flashlight == null || !flashlight.IsOn();
            if (isInDarkness)
            {
                drainAmount += darknessDrainRate * Time.deltaTime;
            }
        }
        
        if (drainNearEnemy)
        {
            Vector3 checkPosition = (playerTransform != null) ? playerTransform.position : transform.position;
            Collider[] enemies = Physics.OverlapSphere(checkPosition, enemyProximityRange, enemyLayer);
            if (enemies.Length > 0)
            {
                drainAmount += enemyProximityDrainRate * Time.deltaTime;
            }
        }
        
        drainAmount += sanityDrainRate * Time.deltaTime;
        
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
            Debug.LogWarning("Critical sanity level!");
        }
        else if (sanityPercent > criticalSanityThreshold)
        {
            isCriticalSanity = false;
        }
        
        if (sanityPercent <= lowSanityThreshold && !isLowSanity)
        {
            isLowSanity = true;
            OnLowSanity?.Invoke();
            Debug.Log("Low sanity level!");
        }
        else if (sanityPercent > lowSanityThreshold)
        {
            isLowSanity = false;
        }
    }
    
    void ApplySanityEffects()
    {
        if (enableAudioEffects)
        {
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

