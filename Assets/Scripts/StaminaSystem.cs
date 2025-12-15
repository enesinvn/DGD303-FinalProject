using UnityEngine;
using UnityEngine.InputSystem;

public class StaminaSystem : MonoBehaviour
{
    [Header("Stamina Ayarları")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 20f; // Koşarken saniyede azalan
    [SerializeField] private float staminaRegenRate = 15f; // Dinlenirken saniyede artan
    [SerializeField] private float regenDelay = 1f; // Koşmayı bıraktıktan kaç saniye sonra dolmaya başlar
    [SerializeField] private float minStaminaToSprint = 10f; // Koşmak için minimum stamina
    
    [Header("Hareket Referansı")]
    [SerializeField] private CharacterController characterController;

    [Header("Ses Referansı")]
    [SerializeField] private AudioManager audioManager;
    
    private float currentStamina;
    private float timeSinceLastSprint;
    private bool canSprint = true;
    
    void Start()
    {
        currentStamina = maxStamina;
        
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
    }
    
    void Update()
    {
        HandleStamina();
        UpdateBreathing();
    }
    
    void HandleStamina()
    {
        bool isMoving = characterController.velocity.magnitude > 0.1f;
        bool wantsToSprint = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        bool isSprinting = isMoving && wantsToSprint && canSprint;
        
        if (isSprinting && currentStamina > 0)
        {
            // Koşarken stamina azalt
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(0, currentStamina);
            timeSinceLastSprint = 0;
            
            // Stamina bitti
            if (currentStamina <= 0)
            {
                canSprint = false;
            }
        }
        else
        {
            // Koşmuyor - stamina doldur (delay sonra)
            timeSinceLastSprint += Time.deltaTime;
            
            if (timeSinceLastSprint >= regenDelay)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(maxStamina, currentStamina);
                
                // Yeterli stamina varsa tekrar koşabilir
                if (currentStamina >= minStaminaToSprint)
                {
                    canSprint = true;
                }
            }
        }
    }
    void UpdateBreathing()
    {
    if (audioManager != null)
    {
        float staminaPercent = GetStaminaPercentage();
        audioManager.SetBreathingState(staminaPercent < 30f, staminaPercent);
    }
    }

    public bool CanSprint()
    {
        return canSprint && currentStamina > 0;
    }
    
    public float GetStaminaPercentage()
    {
        return (currentStamina / maxStamina) * 100f;
    }
    
    public float GetCurrentStamina()
    {
        return currentStamina;
    }
}