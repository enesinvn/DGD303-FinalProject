using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHiding : MonoBehaviour
{
    [Header("Saklanma Ayarları")]
    [SerializeField] private bool isHiding = false;
    [SerializeField] private HidingSpot currentHidingSpot;
    
    [Header("Nefes Tutma")]
    [SerializeField] private bool isHoldingBreath = false;
    [SerializeField] private float breathHoldStaminaCost = 15f; // Saniyede azalan stamina
    [SerializeField] private float minStaminaToHoldBreath = 10f; // Nefes tutmak için minimum stamina
    
    [Header("Referanslar")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private StaminaSystem staminaSystem;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera playerCamera;
    
    [Header("UI")]
    [SerializeField] private GameObject hidingUI;
    [SerializeField] private GameObject breathHoldUI;
    
    [Header("Ses")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip breathHoldSound;
    [SerializeField] private AudioClip breathReleaseSound;
    
    private float originalWalkSpeed;
    private float originalSprintSpeed;
    
    void Start()
    {
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }
        
        if (staminaSystem == null)
        {
            staminaSystem = GetComponent<StaminaSystem>();
        }
        
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
        
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // UI'ları gizle
        if (hidingUI != null)
        {
            hidingUI.SetActive(false);
        }
        
        if (breathHoldUI != null)
        {
            breathHoldUI.SetActive(false);
        }
    }
    
    void Update()
    {
        if (isHiding)
        {
            HandleHidingInput();
            HandleBreathHolding();
        }
    }
    
    void HandleHidingInput()
    {
        // E tuşu ile çık
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (currentHidingSpot != null)
            {
                currentHidingSpot.Interact(); // Exit hiding
            }
        }
        
        // Nefes tut (Space veya Shift)
        if (currentHidingSpot != null && currentHidingSpot.GetHidingType() != HidingSpot.HidingType.Corner)
        {
            if (Keyboard.current != null)
            {
                bool wantsToHoldBreath = Keyboard.current.spaceKey.isPressed || 
                                        Keyboard.current.leftShiftKey.isPressed;
                
                if (wantsToHoldBreath && !isHoldingBreath && CanHoldBreath())
                {
                    StartBreathHolding();
                }
                else if (!wantsToHoldBreath && isHoldingBreath)
                {
                    StopBreathHolding();
                }
            }
        }
    }
    
    void HandleBreathHolding()
    {
        if (isHoldingBreath && staminaSystem != null)
        {
            // Stamina azalt
            float currentStamina = staminaSystem.GetCurrentStamina();
            currentStamina -= breathHoldStaminaCost * Time.deltaTime;
            
            // Stamina bittiğinde nefes tutmayı bırak
            if (currentStamina <= 0f)
            {
                StopBreathHolding();
            }
        }
    }
    
    public void StartHiding(HidingSpot hidingSpot, bool requiresBreath, float breathDrainRate)
    {
        isHiding = true;
        currentHidingSpot = hidingSpot;
        isHoldingBreath = false;
        
        // Hareketi durdur
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        
        // UI göster
        if (hidingUI != null)
        {
            hidingUI.SetActive(true);
        }
        
        // Kamera kontrolünü sınırla (opsiyonel)
        if (playerCamera != null)
        {
            // Kamerayı sınırlayabilirsiniz
        }
        
        Debug.Log("Saklanıyorsunuz! (E tuşu ile çıkın)");
    }
    
    public void StopHiding()
    {
        isHiding = false;
        currentHidingSpot = null;
        isHoldingBreath = false;
        
        // Hareketi tekrar etkinleştir
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        
        // UI gizle
        if (hidingUI != null)
        {
            hidingUI.SetActive(false);
        }
        
        if (breathHoldUI != null)
        {
            breathHoldUI.SetActive(false);
        }
        
        Debug.Log("Saklanmayı bıraktınız!");
    }
    
    void StartBreathHolding()
    {
        if (!CanHoldBreath()) return;
        
        isHoldingBreath = true;
        
        // UI göster
        if (breathHoldUI != null)
        {
            breathHoldUI.SetActive(true);
        }
        
        // Ses efekti
        if (audioSource != null && breathHoldSound != null)
        {
            audioSource.PlayOneShot(breathHoldSound);
        }
        
        Debug.Log("Nefes tutuyorsunuz...");
    }
    
    void StopBreathHolding()
    {
        isHoldingBreath = false;
        
        // UI gizle
        if (breathHoldUI != null)
        {
            breathHoldUI.SetActive(false);
        }
        
        // Ses efekti
        if (audioSource != null && breathReleaseSound != null)
        {
            audioSource.PlayOneShot(breathReleaseSound);
        }
    }
    
    bool CanHoldBreath()
    {
        if (staminaSystem == null) return false;
        return staminaSystem.GetCurrentStamina() >= minStaminaToHoldBreath;
    }
    
    public bool CanHide()
    {
        return !isHiding;
    }
    
    public bool IsHiding()
    {
        return isHiding;
    }
    
    public bool IsHoldingBreath()
    {
        return isHoldingBreath;
    }
    
    public HidingSpot GetCurrentHidingSpot()
    {
        return currentHidingSpot;
    }
}

