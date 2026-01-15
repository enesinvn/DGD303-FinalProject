using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHiding : MonoBehaviour
{
    [Header("Hiding Settings")]
    [SerializeField] private LayerMask hidingSpotLayer;
    
    [Header("Camera Settings")]
    [SerializeField] private Transform playerCamera;
    
    [Header("Breath Holding System")]
    [SerializeField] private float maxBreathHoldTime = 10f;
    [SerializeField] private float breathRecoveryRate = 1f;
    [SerializeField] private KeyCode holdBreathKey = KeyCode.Space;
    
    [Header("UI (Optional)")]
    [SerializeField] private UnityEngine.UI.Image breathBar;
    [SerializeField] private GameObject breathHoldPrompt;
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource breathingAudioSource;
    [SerializeField] private AudioClip normalBreathingSound;
    [SerializeField] private AudioClip holdingBreathSound;
    [SerializeField] private AudioClip gaspingSound;
    
    [Header("Movement Control")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Flashlight flashlight;
    
    private bool isHiding = false;
    private HidingSpot currentHidingSpot;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Vector3 originalPlayerPosition;
    
    private bool isHoldingBreath = false;
    private float currentBreathTime;
    
    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>()?.transform;
        }
        
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
        
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }
        
        if (flashlight == null)
        {
            flashlight = GetComponentInChildren<Flashlight>();
        }
        
        currentBreathTime = maxBreathHoldTime;
        
        if (breathHoldPrompt != null)
        {
            breathHoldPrompt.SetActive(false);
        }
        
        if (breathBar != null)
        {
            breathBar.gameObject.SetActive(false);
        }
    }
    
    void Update()
    {
        if (isHiding)
        {
            // Saklanırken tüm hareketi durdur
            if (characterController != null && characterController.enabled)
            {
                characterController.Move(Vector3.zero);
            }
            
            HandleBreathHolding();
        }
        else
        {
            if (currentBreathTime < maxBreathHoldTime)
            {
                currentBreathTime += breathRecoveryRate * Time.deltaTime;
                currentBreathTime = Mathf.Min(currentBreathTime, maxBreathHoldTime);
            }
        }
    }
    
    void HandleBreathHolding()
    {
        bool wantsToHoldBreath = Input.GetKey(holdBreathKey);
        
        if (wantsToHoldBreath && currentBreathTime > 0)
        {
            if (!isHoldingBreath)
            {
                isHoldingBreath = true;
                PlayBreathSound(holdingBreathSound);
                Debug.Log("[PlayerHiding] Started holding breath");
            }
            
            currentBreathTime -= Time.deltaTime;
            
            if (currentBreathTime <= 0)
            {
                currentBreathTime = 0;
                isHoldingBreath = false;
                PlayBreathSound(gaspingSound);
                Debug.Log("[PlayerHiding] Breath ran out! Forced to breathe");
            }
        }
        else
        {
            if (isHoldingBreath)
            {
                isHoldingBreath = false;
                PlayBreathSound(normalBreathingSound);
                Debug.Log("[PlayerHiding] Stopped holding breath");
            }
            
            if (currentBreathTime < maxBreathHoldTime)
            {
                currentBreathTime += breathRecoveryRate * Time.deltaTime;
                currentBreathTime = Mathf.Min(currentBreathTime, maxBreathHoldTime);
            }
        }
        
        UpdateBreathUI();
    }
    
    void UpdateBreathUI()
    {
        if (breathBar != null)
        {
            breathBar.fillAmount = currentBreathTime / maxBreathHoldTime;
        }
    }
    
    void PlayBreathSound(AudioClip clip)
    {
        // Try AudioManager first for breath sounds
        AudioManager audioManager = AudioManager.Instance;
        if (audioManager != null && clip == holdingBreathSound)
        {
            audioManager.PlayBreathSound(breathingAudioSource);
        }
        else if (breathingAudioSource != null && clip != null)
        {
            breathingAudioSource.PlayOneShot(clip);
        }
    }
    
    public bool IsHoldingBreath()
    {
        return isHoldingBreath && isHiding;
    }
    
    public HidingSpot GetCurrentHidingSpot()
    {
        return currentHidingSpot;
    }
    
    public bool IsHiding()
    {
        return isHiding;
    }
    
    public float GetBreathPercentage()
    {
        return (currentBreathTime / maxBreathHoldTime) * 100f;
    }
    
    public void EnterHidingSpot(HidingSpot spot)
    {
        if (isHiding || spot == null) return;
        
        Debug.Log($"[PlayerHiding] Entering hiding spot: {spot.name}");
        
        currentHidingSpot = spot;
        isHiding = true;
        
        if (breathHoldPrompt != null)
        {
            breathHoldPrompt.SetActive(true);
        }
        
        if (breathBar != null)
        {
            breathBar.gameObject.SetActive(true);
        }
        
        originalPlayerPosition = transform.position;
        originalCameraPosition = playerCamera.localPosition;
        originalCameraRotation = playerCamera.localRotation;
        
        // Hareketi tamamen durdur
        if (characterController != null)
        {
            characterController.Move(Vector3.zero);
            Debug.Log("[PlayerHiding] Movement stopped");
        }
        
        if (playerController != null)
        {
            playerController.enabled = false;
            Debug.Log("[PlayerHiding] PlayerController disabled");
        }
        
        if (flashlight != null && flashlight.IsOn())
        {
            var toggleMethod = flashlight.GetType().GetMethod("ToggleFlashlight", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (toggleMethod != null)
            {
                toggleMethod.Invoke(flashlight, null);
            }
            Debug.Log("[PlayerHiding] Flashlight turned off");
        }
        
        // CharacterController'ı geçici olarak kapat
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        
        // Oyuncuyu saklanma pozisyonuna taşı
        transform.position = spot.GetPlayerPosition();
        transform.rotation = spot.GetPlayerRotation();
        
        if (spot.HasCustomCameraPosition())
        {
            playerCamera.localPosition = spot.GetCameraOffset();
            playerCamera.localRotation = Quaternion.Euler(spot.GetCameraRotation());
        }
        
        // CharacterController'ı aktif et ama PlayerController kapalı kalsın
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        
        PlayBreathSound(normalBreathingSound);
        
        Debug.Log($"[PlayerHiding] Hiding complete. IsHiding: {isHiding}, Spot: {currentHidingSpot?.name ?? "null"}");
    }
    
    public void ExitHidingSpot()
    {
        if (!isHiding || currentHidingSpot == null) 
        {
            Debug.LogWarning("[PlayerHiding] ExitHidingSpot called but not hiding!");
            return;
        }
        
        Debug.Log($"[PlayerHiding] Exiting hiding spot: {currentHidingSpot.name}");
        
        if (breathHoldPrompt != null)
        {
            breathHoldPrompt.SetActive(false);
        }
        
        if (breathBar != null)
        {
            breathBar.gameObject.SetActive(false);
        }
        
        isHoldingBreath = false;
        
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        
        Vector3 exitPosition = currentHidingSpot.GetExitPosition();
        
        if (characterController != null)
        {
            float groundOffset = characterController.height / 2f + characterController.skinWidth;
            if (exitPosition.y < 1f)
            {
                exitPosition.y = groundOffset;
            }
        }
        
        Debug.Log($"[PlayerHiding] Exit position: {exitPosition}");
        
        transform.position = exitPosition;
        
        playerCamera.localPosition = originalCameraPosition;
        playerCamera.localRotation = originalCameraRotation;
        
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        
        if (playerController != null)
        {
            playerController.enabled = true;
            Debug.Log("[PlayerHiding] PlayerController enabled");
        }
        
        HidingSpot spotToRelease = currentHidingSpot;
        currentHidingSpot = null;
        isHiding = false;
        
        spotToRelease.SetOccupied(false);
        
        Debug.Log($"[PlayerHiding] Exit complete. IsHiding: {isHiding}, Position: {transform.position}");
    }
    
    void OnDrawGizmos()
    {
        if (isHiding && currentHidingSpot != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(currentHidingSpot.GetExitPosition(), 0.3f);
            Gizmos.DrawLine(transform.position, currentHidingSpot.GetExitPosition());
            
            if (isHoldingBreath)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position + Vector3.up * 2, 0.2f);
            }
        }
    }
}