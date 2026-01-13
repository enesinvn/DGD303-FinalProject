using UnityEngine;

public class StealthSystem : MonoBehaviour
{
    [Header("Stealth Settings")]
    [SerializeField] private float stealthLevel = 1f;
    [SerializeField] private float detectionRisk = 0f;
    
    [Header("Light/Shadow")]
    [SerializeField] private bool useLightSystem = true;
    [SerializeField] private float lightCheckRadius = 1f;
    [SerializeField] private LayerMask lightLayer;
    
    [Header("Movement Modifier")]
    [SerializeField] private float crouchStealthBonus = 0.3f;
    [SerializeField] private float sprintStealthPenalty = 0.5f;
    
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerHiding playerHiding;
    [SerializeField] private Flashlight flashlight;
    
    [Header("UI")]
    [SerializeField] private GameObject stealthIndicator;
    
    void Start()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
        
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }
        
        if (playerHiding == null)
        {
            playerHiding = GetComponent<PlayerHiding>();
        }
        
        if (flashlight == null)
        {
            flashlight = FindFirstObjectByType<Flashlight>();
        }
    }
    
    void Update()
    {
        CalculateStealthLevel();
        UpdateUI();
    }
    
    void CalculateStealthLevel()
    {
        float baseStealth = 1f;
        
        if (playerHiding != null && playerHiding.IsHiding())
        {
            stealthLevel = 0f;
            detectionRisk = 0f;
            return;
        }
        
        if (useLightSystem)
        {
            bool isInLight = CheckIfInLight();
            if (isInLight)
            {
                baseStealth = 1f;
            }
            else
            {
                baseStealth = 0.3f;
            }
        }
        
        if (flashlight != null && flashlight.IsOn())
        {
            baseStealth = 1f;
        }
        
        if (characterController != null && characterController.enabled)
        {
            float speed = characterController.velocity.magnitude;
            float maxSpeed = 6f;
            if (playerController != null)
            {
                maxSpeed = playerController.SprintSpeed;
            }
            
            if (speed > 0.1f)
            {
                bool isCrouching = Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl);
                bool isSprinting = Input.GetKey(KeyCode.LeftShift);
                
                if (isCrouching)
                {
                    baseStealth -= crouchStealthBonus;
                }
                else if (isSprinting)
                {
                    baseStealth += sprintStealthPenalty;
                }
                
                float speedRatio = speed / maxSpeed;
                baseStealth += speedRatio * 0.2f;
            }
        }
        
        stealthLevel = Mathf.Clamp01(baseStealth);
        
        detectionRisk = stealthLevel;
    }
    
    bool CheckIfInLight()
    {
        Collider[] lights = Physics.OverlapSphere(transform.position, lightCheckRadius, lightLayer);
        return lights.Length > 0;
    }
    
    void UpdateUI()
    {
        if (stealthIndicator != null)
        {
        }
    }
    
    
    public float GetStealthLevel()
    {
        return stealthLevel;
    }
    
    public float GetDetectionRisk()
    {
        return detectionRisk;
    }
    
    public bool IsHidden()
    {
        return stealthLevel < 0.3f;
    }
    
    public bool IsVisible()
    {
        return stealthLevel > 0.7f;
    }
}

