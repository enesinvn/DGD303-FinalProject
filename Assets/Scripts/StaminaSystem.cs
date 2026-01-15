using UnityEngine;
using UnityEngine.InputSystem;

public class StaminaSystem : MonoBehaviour
{
    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 150f;
    [SerializeField] private float staminaDrainRate = 12f;
    [SerializeField] private float staminaRegenRate = 25f;
    [SerializeField] private float regenDelay = 0.5f;
    [SerializeField] private float minStaminaToSprint = 5f;
    
    [Header("Movement Reference")]
    [SerializeField] private CharacterController characterController;
    
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
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(0, currentStamina);
            timeSinceLastSprint = 0;
            
            if (currentStamina <= 0)
            {
                canSprint = false;
            }
        }
        else
        {
            timeSinceLastSprint += Time.deltaTime;
            
            if (timeSinceLastSprint >= regenDelay)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(maxStamina, currentStamina);
                
                if (currentStamina >= minStaminaToSprint)
                {
                    canSprint = true;
                }
            }
        }
    }
    
    void UpdateBreathing()
    {
        // Future: Add breathing sounds based on stamina
        // Low stamina = heavy breathing
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
    
    public void SetCurrentStamina(float stamina)
    {
        currentStamina = Mathf.Clamp(stamina, 0f, maxStamina);
    }
    
    public void ReduceStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Max(0f, currentStamina);
    }
}