using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // Unity 6 için

public class PostProcessingController : MonoBehaviour
{
    [Header("Post Processing")]
    [SerializeField] private Volume globalVolume;
    
    [Header("Stamina Referansı")]
    [SerializeField] private StaminaSystem staminaSystem;
    
    [Header("Efekt Ayarları")]
    [SerializeField] private float normalVignetteIntensity = 0.35f;
    [SerializeField] private float lowStaminaVignetteIntensity = 0.6f;
    [SerializeField] private float transitionSpeed = 2f;
    
    private Vignette vignette;
    private ColorAdjustments colorAdjustments;
    
    void Start()
    {
        if (globalVolume != null && globalVolume.profile != null)
        {
            // Vignette efektini al
            globalVolume.profile.TryGet(out vignette);
            globalVolume.profile.TryGet(out colorAdjustments);
        }
    }
    
    void Update()
    {
        if (staminaSystem == null) return;
        
        UpdatePostProcessing();
    }
    
    void UpdatePostProcessing()
    {
        float staminaPercent = staminaSystem.GetStaminaPercentage();
        
        // Stamina düşükse ekran daha karanlık
        if (vignette != null)
        {
            float targetIntensity = staminaPercent < 20f ? lowStaminaVignetteIntensity : normalVignetteIntensity;
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetIntensity, Time.deltaTime * transitionSpeed);
        }
        
        // Renk azalt (opsiyonel)
        if (colorAdjustments != null && staminaPercent < 20f)
        {
            float targetSaturation = Mathf.Lerp(-30f, -20f, staminaPercent / 20f);
            colorAdjustments.saturation.value = Mathf.Lerp(colorAdjustments.saturation.value, targetSaturation, Time.deltaTime * transitionSpeed);
        }
    }
}
