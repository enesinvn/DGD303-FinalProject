using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSoundController : MonoBehaviour
{
    [Header("Sound Settings")]
    [SerializeField] private float walkSoundRadius = 5f;
    [SerializeField] private float sprintSoundRadius = 15f;
    [SerializeField] private float crouchSoundRadius = 2f;
    [SerializeField] private float walkSoundIntensity = 0.3f;
    [SerializeField] private float sprintSoundIntensity = 1f;
    [SerializeField] private float crouchSoundIntensity = 0.1f;
    
    [Header("Sound Emission Intervals")]
    [SerializeField] private float walkSoundInterval = 0.6f;
    [SerializeField] private float sprintSoundInterval = 0.4f;
    [SerializeField] private float crouchSoundInterval = 1f;
    
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerHiding playerHiding;
    [SerializeField] private SoundEmitter soundEmitter;
    
    [Header("Layer")]
    [SerializeField] private LayerMask enemyLayer;
    
    [Header("Debug/Test")]
    [SerializeField] private bool enableDebugControls = false;
    
    private float nextSoundTime = 0f;
    private bool wasMoving = false;
    
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
        
        if (soundEmitter == null)
        {
            soundEmitter = GetComponent<SoundEmitter>();
        }
        
        if (soundEmitter == null)
        {
            soundEmitter = gameObject.AddComponent<SoundEmitter>();
            Debug.Log("[PlayerSoundController] SoundEmitter automatically added!");
        }
        else
        {
            Debug.Log("[PlayerSoundController] SoundEmitter already exists.");
        }
        
        if (soundEmitter != null)
        {
            soundEmitter.soundRadius = sprintSoundRadius;
            soundEmitter.soundType = SoundEmitter.SoundType.Footstep;
            soundEmitter.isContinuous = false;
            soundEmitter.showDebugVisuals = false;
            soundEmitter.SetEnemyLayer(enemyLayer);
            Debug.Log("[PlayerSoundController] SoundEmitter configured!");
        }
        else
        {
            Debug.LogError("[PlayerSoundController] SoundEmitter could not be added!");
        }
    }
    
    void Update()
    {
        if (playerHiding != null && playerHiding.IsHiding())
        {
            return;
        }
        
        HandleFootstepSounds();
        
        if (enableDebugControls && Keyboard.current != null)
        {
            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                Debug.Log("[PlayerSoundController] Test sound emitted (Footstep)");
                EmitFootstepSound(sprintSoundRadius, sprintSoundIntensity);
            }
            
            if (Keyboard.current.yKey.wasPressedThisFrame)
            {
                Debug.Log("[PlayerSoundController] Test sound emitted (Interaction)");
                EmitInteractionSound(0.8f);
            }
            
            if (Keyboard.current.uKey.wasPressedThisFrame)
            {
                Debug.Log("[PlayerSoundController] Test sound emitted (Throw)");
                EmitThrowSound(1f);
            }
        }
    }
    
    void HandleFootstepSounds()
    {
        if (characterController == null) return;
        
        bool isMoving = characterController.velocity.magnitude > 0.1f;
        bool isGrounded = characterController.isGrounded;
        
        if (isMoving && isGrounded && Time.time >= nextSoundTime)
        {
            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            bool isCrouching = Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl);
            
            float soundRadius;
            float soundIntensity;
            float soundInterval;
            
            if (isCrouching)
            {
                soundRadius = crouchSoundRadius;
                soundIntensity = crouchSoundIntensity;
                soundInterval = crouchSoundInterval;
            }
            else if (isSprinting)
            {
                soundRadius = sprintSoundRadius;
                soundIntensity = sprintSoundIntensity;
                soundInterval = sprintSoundInterval;
            }
            else
            {
                soundRadius = walkSoundRadius;
                soundIntensity = walkSoundIntensity;
                soundInterval = walkSoundInterval;
            }
            
            EmitFootstepSound(soundRadius, soundIntensity);
            
            nextSoundTime = Time.time + soundInterval;
        }
        
        wasMoving = isMoving;
    }
    
    void EmitFootstepSound(float radius, float intensity)
    {
        if (soundEmitter == null)
        {
            soundEmitter = GetComponent<SoundEmitter>();
        }
        
        if (soundEmitter == null)
        {
            Debug.LogWarning("[PlayerSoundController] SoundEmitter not found! Cannot emit sound.");
            return;
        }
        
        soundEmitter.soundRadius = radius;
        soundEmitter.soundIntensity = intensity;
        soundEmitter.soundType = SoundEmitter.SoundType.Footstep;
        
        soundEmitter.EmitSound(intensity);
    }
    
    public void EmitInteractionSound(float intensity = 0.5f)
    {
        if (soundEmitter == null)
        {
            soundEmitter = GetComponent<SoundEmitter>();
        }
        
        if (soundEmitter == null) return;
        
        soundEmitter.soundType = SoundEmitter.SoundType.Interaction;
        soundEmitter.soundRadius = 8f;
        soundEmitter.EmitSound(intensity);
    }
    
    public void EmitItemPickupSound(float intensity = 0.3f)
    {
        if (soundEmitter == null)
        {
            soundEmitter = GetComponent<SoundEmitter>();
        }
        
        if (soundEmitter == null) return;
        
        soundEmitter.soundType = SoundEmitter.SoundType.ItemPickup;
        soundEmitter.soundRadius = 5f;
        soundEmitter.EmitSound(intensity);
    }
    
    public void EmitThrowSound(float intensity = 1f)
    {
        if (soundEmitter == null)
        {
            soundEmitter = GetComponent<SoundEmitter>();
        }
        
        if (soundEmitter == null) return;
        
        soundEmitter.soundType = SoundEmitter.SoundType.Throw;
        soundEmitter.soundRadius = 12f;
        soundEmitter.EmitSound(intensity);
    }
}

