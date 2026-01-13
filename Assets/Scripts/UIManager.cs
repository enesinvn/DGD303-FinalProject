using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Battery UI")]
    [SerializeField] private RectTransform batteryFillTransform;
    [SerializeField] private TextMeshProUGUI batteryText;
    
    [Header("Stamina UI")]
    [SerializeField] private RectTransform staminaFillTransform;
    
    [Header("Health UI")]
    [SerializeField] private RectTransform healthFillTransform;
    [SerializeField] private TextMeshProUGUI healthText;
    
    [Header("Sanity UI")]
    [SerializeField] private RectTransform sanityFillTransform;
    [SerializeField] private TextMeshProUGUI sanityText;
    
    [Header("Crouch Status UI")]
    [SerializeField] private GameObject crouchIcon;
    [SerializeField] private GameObject standingIcon;
    [SerializeField] private float crouchIconDisplayTime = 2f;
    [SerializeField] private float crouchIconFadeTime = 0.5f;
    
    [Header("References")]
    [SerializeField] private Flashlight flashlight;
    [SerializeField] private StaminaSystem staminaSystem;
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private SanitySystem sanitySystem;
    [SerializeField] private PlayerController playerController;
    
    private Image batteryFillImage;
    private Image staminaFillImage;
    private Image healthFillImage;
    private Image sanityFillImage;
    
    private bool wasCrouching = false;
    private float crouchIconTimer = 0f;
    private float standingIconTimer = 0f;
    private CanvasGroup crouchIconCanvasGroup;
    private CanvasGroup standingIconCanvasGroup;
    
    void Start()
    {
        if (batteryFillTransform != null)
        {
            batteryFillImage = batteryFillTransform.GetComponent<Image>();
        }
        
        if (staminaFillTransform != null)
        {
            staminaFillImage = staminaFillTransform.GetComponent<Image>();
        }
        
        if (healthFillTransform != null)
        {
            healthFillImage = healthFillTransform.GetComponent<Image>();
        }
        
        if (sanityFillTransform != null)
        {
            sanityFillImage = sanityFillTransform.GetComponent<Image>();
        }
        
        if (sanitySystem == null)
        {
            sanitySystem = FindFirstObjectByType<SanitySystem>();
        }
        
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
        }
        
        if (crouchIcon != null)
        {
            crouchIconCanvasGroup = crouchIcon.GetComponent<CanvasGroup>();
            if (crouchIconCanvasGroup == null)
            {
                crouchIconCanvasGroup = crouchIcon.AddComponent<CanvasGroup>();
            }
            crouchIcon.SetActive(false);
        }
        
        if (standingIcon != null)
        {
            standingIconCanvasGroup = standingIcon.GetComponent<CanvasGroup>();
            if (standingIconCanvasGroup == null)
            {
                standingIconCanvasGroup = standingIcon.AddComponent<CanvasGroup>();
            }
            standingIcon.SetActive(false);
        }
    }
    
    void Update()
    {
        UpdateBatteryUI();
        UpdateStaminaUI();
        UpdateHealthUI();
        UpdateSanityUI();
        UpdateCrouchUI();
    }
    
    void UpdateBatteryUI()
    {
        if (flashlight == null || batteryFillTransform == null) return;
        
        float batteryPercent = flashlight.GetBatteryPercentage();
        
        Vector3 scale = batteryFillTransform.localScale;
        scale.x = batteryPercent / 100f;
        batteryFillTransform.localScale = scale;
        
        if (batteryText != null)
        {
            batteryText.text = $"{Mathf.RoundToInt(batteryPercent)}%";
        }
        
        if (batteryFillImage != null)
        {
            if (batteryPercent < 20f)
            {
                batteryFillImage.color = Color.red;
            }
            else if (batteryPercent < 50f)
            {
                batteryFillImage.color = Color.yellow;
            }
            else
            {
                batteryFillImage.color = new Color(0.3f, 1f, 0.3f);
            }
        }
    }
    
    void UpdateStaminaUI()
    {
        if (staminaSystem == null || staminaFillTransform == null) return;
        
        float staminaPercent = staminaSystem.GetStaminaPercentage();
        
        Vector3 scale = staminaFillTransform.localScale;
        scale.x = staminaPercent / 100f;
        staminaFillTransform.localScale = scale;
        
        if (staminaFillImage != null)
        {
            if (staminaPercent < 20f)
            {
                staminaFillImage.color = Color.red;
            }
            else if (staminaPercent < 50f)
            {
                staminaFillImage.color = Color.yellow;
            }
            else
            {
                staminaFillImage.color = new Color(0f, 0.9f, 1f);
            }
        }
    }
    
    void UpdateHealthUI()
    {
        if (healthSystem == null || healthFillTransform == null) return;
        
        float healthPercent = healthSystem.GetHealthPercentage();
        
        Vector3 scale = healthFillTransform.localScale;
        scale.x = healthPercent / 100f;
        healthFillTransform.localScale = scale;
        
        if (healthText != null)
        {
            healthText.text = $"{Mathf.RoundToInt(healthSystem.GetCurrentHealth())}/{Mathf.RoundToInt(healthSystem.GetMaxHealth())}";
        }
        
        if (healthFillImage != null)
        {
            if (healthPercent < 25f)
            {
                healthFillImage.color = Color.red;
            }
            else if (healthPercent < 50f)
            {
                healthFillImage.color = Color.yellow;
            }
            else
            {
                healthFillImage.color = new Color(0.2f, 1f, 0.2f);
            }
        }
    }
    
    void UpdateSanityUI()
    {
        if (sanitySystem == null || sanityFillTransform == null) return;
        
        float sanityPercent = sanitySystem.GetSanityPercentage();
        
        Vector3 scale = sanityFillTransform.localScale;
        scale.x = sanityPercent / 100f;
        sanityFillTransform.localScale = scale;
        
        if (sanityText != null)
        {
            sanityText.text = $"{Mathf.RoundToInt(sanityPercent)}%";
        }
        
        if (sanityFillImage != null)
        {
            if (sanitySystem.IsCriticalSanity())
            {
                sanityFillImage.color = Color.red;
            }
            else if (sanitySystem.IsLowSanity())
            {
                sanityFillImage.color = Color.yellow;
            }
            else
            {
                sanityFillImage.color = new Color(0.5f, 0.2f, 1f);
            }
        }
    }
    
    void UpdateCrouchUI()
    {
        if (playerController == null) return;
        
        bool isCrouching = playerController.IsCrouching;
        
        if (isCrouching != wasCrouching)
        {
            wasCrouching = isCrouching;
            
            if (isCrouching)
            {
                crouchIconTimer = crouchIconDisplayTime + crouchIconFadeTime;
                standingIconTimer = 0f;
                
                if (crouchIcon != null)
                {
                    crouchIcon.SetActive(true);
                }
            }
            else
            {
                standingIconTimer = crouchIconDisplayTime + crouchIconFadeTime;
                crouchIconTimer = 0f;
                
                if (standingIcon != null)
                {
                    standingIcon.SetActive(true);
                }
            }
        }
        
        if (crouchIconTimer > 0f)
        {
            crouchIconTimer -= Time.deltaTime;
            
            if (crouchIcon != null && crouchIcon.activeSelf)
            {
                if (crouchIconCanvasGroup != null)
                {
                    if (crouchIconTimer <= crouchIconFadeTime)
                    {
                        crouchIconCanvasGroup.alpha = crouchIconTimer / crouchIconFadeTime;
                    }
                    else
                    {
                        crouchIconCanvasGroup.alpha = 1f;
                    }
                }
            }
        }
        else
        {
            if (crouchIcon != null && crouchIcon.activeSelf)
            {
                crouchIcon.SetActive(false);
            }
        }
        
        if (standingIconTimer > 0f)
        {
            standingIconTimer -= Time.deltaTime;
            
            if (standingIcon != null && standingIcon.activeSelf)
            {
                if (standingIconCanvasGroup != null)
                {
                    if (standingIconTimer <= crouchIconFadeTime)
                    {
                        standingIconCanvasGroup.alpha = standingIconTimer / crouchIconFadeTime;
                    }
                    else
                    {
                        standingIconCanvasGroup.alpha = 1f;
                    }
                }
            }
        }
        else
        {
            if (standingIcon != null && standingIcon.activeSelf)
            {
                standingIcon.SetActive(false);
            }
        }
    }
}
