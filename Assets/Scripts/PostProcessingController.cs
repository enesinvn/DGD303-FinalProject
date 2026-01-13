using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // Unity 6 için

public class PostProcessingController : MonoBehaviour
{
    [Header("Post Processing")]
    [SerializeField] private Volume globalVolume;
    
    [Header("Stamina Reference")]
    [SerializeField] private StaminaSystem staminaSystem;
    
    [Header("Sanity Reference")]
    [SerializeField] private SanitySystem sanitySystem;
    
    [Header("Effect Settings")]
    [SerializeField] private float normalVignetteIntensity = 0.35f;
    [SerializeField] private float lowStaminaVignetteIntensity = 0.6f;
    [SerializeField] private float lowSanityVignetteIntensity = 0.8f;
    [SerializeField] private float criticalSanityVignetteIntensity = 1f;
    [SerializeField] private float transitionSpeed = 2f;
    
    private Vignette vignette;
    private ColorAdjustments colorAdjustments;
    
    void Start()
    {
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out vignette);
            globalVolume.profile.TryGet(out colorAdjustments);
        }
        
        if (sanitySystem == null)
        {
            sanitySystem = FindFirstObjectByType<SanitySystem>();
        }
    }
    
    void Update()
    {
        UpdatePostProcessing();
    }
    
    void UpdatePostProcessing()
    {
        float targetVignette = normalVignetteIntensity;
        float targetSaturation = 0f;
        
        if (staminaSystem != null)
        {
            float staminaPercent = staminaSystem.GetStaminaPercentage();
            
            if (staminaPercent < 20f)
            {
                targetVignette = Mathf.Max(targetVignette, lowStaminaVignetteIntensity);
                targetSaturation = Mathf.Lerp(-30f, -20f, staminaPercent / 20f);
            }
        }
        
        if (sanitySystem != null)
        {
            float sanityPercent = sanitySystem.GetSanityPercentage();
            
            if (sanitySystem.IsCriticalSanity())
            {
                targetVignette = criticalSanityVignetteIntensity;
                targetSaturation = -50f;
            }
            else if (sanitySystem.IsLowSanity())
            {
                targetVignette = Mathf.Lerp(lowSanityVignetteIntensity, criticalSanityVignetteIntensity, 1f - (sanityPercent / 30f));
                targetSaturation = Mathf.Lerp(-40f, -30f, sanityPercent / 30f);
            }
        }
        
        if (vignette != null)
        {
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetVignette, Time.deltaTime * transitionSpeed);
        }
        
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = Mathf.Lerp(colorAdjustments.saturation.value, targetSaturation, Time.deltaTime * transitionSpeed);
        }
    }
}
