using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float crouchWalkSpeed = 1.5f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Kamera Ayarları")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;
    
    public float MouseSensitivity 
    { 
        get { return mouseSensitivity; } 
        set { mouseSensitivity = Mathf.Clamp(value, 0.1f, 10f); } 
    }
    
    [Header("Eğilme Ayarları")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchTransitionSpeed = 5f;
    
    [Header("Ses Ayarları")]
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float footstepInterval = 0.5f;

    [Header("Stamina Referansı")]
    [SerializeField] private StaminaSystem staminaSystem;
    
    [Header("Saklanma Referansı")]
    [SerializeField] private PlayerHiding playerHiding;
    
    private CharacterController characterController;
    private Vector3 moveDirection;
    private float verticalVelocity;
    private float cameraPitch;
    private bool isCrouching;
    private float nextFootstepTime;
    private Vector3 initialCameraPosition;
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (cameraTransform == null)
        {
            Debug.LogWarning("Kamera transform atanmamış! Main Camera'yı cameraTransform'a sürükleyin.");
        }
        else
        {
            initialCameraPosition = cameraTransform.localPosition;
        }
    }
    
    void Update()
    {
        HandleMovement();
        HandleCamera();
        HandleCrouch();
        HandleFootsteps();
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    
    void HandleMovement()
    {
        // Saklanıyorsa hareket etme
        if (playerHiding != null && playerHiding.IsHiding())
        {
            return;
        }
        
        float moveX = 0f;
        float moveZ = 0f;
        
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveZ = 1f;
            if (Keyboard.current.sKey.isPressed) moveZ = -1f;
            if (Keyboard.current.aKey.isPressed) moveX = -1f;
            if (Keyboard.current.dKey.isPressed) moveX = 1f;
        }
        
        bool wantsToSprint = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        bool canSprint = staminaSystem != null ? staminaSystem.CanSprint() : true;
        bool isSprinting = wantsToSprint && canSprint && !isCrouching;
        
        float currentSpeed = isCrouching ? crouchWalkSpeed : (isSprinting ? sprintSpeed : walkSpeed);
        
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        moveDirection = move.normalized * currentSpeed;
        
        if (characterController.isGrounded)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        
        moveDirection.y = verticalVelocity;
        characterController.Move(moveDirection * Time.deltaTime);
    }
    
    void HandleCamera()
    {
        if (cameraTransform == null) return;
        
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        transform.Rotate(Vector3.up * mouseX);
        
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }
    
    void HandleCrouch()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.cKey.wasPressedThisFrame || 
                Keyboard.current.leftCtrlKey.wasPressedThisFrame)
            {
                isCrouching = !isCrouching;
            }
        }
        
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        characterController.height = Mathf.Lerp(characterController.height, targetHeight, 
                                                Time.deltaTime * crouchTransitionSpeed);
        
        if (cameraTransform != null)
        {
            Vector3 cameraPos = cameraTransform.localPosition;
            cameraPos.y = initialCameraPosition.y + (characterController.height - standingHeight);
            cameraTransform.localPosition = cameraPos;
        }
    }
    
    void HandleFootsteps()
    {
        if (characterController.isGrounded && characterController.velocity.magnitude > 0.1f)
        {
            if (Time.time >= nextFootstepTime)
            {
                PlayFootstep();
                
                bool isSprinting = Input.GetKey(KeyCode.LeftShift) && !isCrouching;
                float interval = isSprinting ? footstepInterval * 0.6f : footstepInterval;
                nextFootstepTime = Time.time + interval;
            }
        }
    }
    
    void PlayFootstep()
    {
        if (footstepAudioSource != null && footstepSounds != null && footstepSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, footstepSounds.Length);
            footstepAudioSource.PlayOneShot(footstepSounds[randomIndex]);
        }
    }
}