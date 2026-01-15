using UnityEngine;

public class HeadBob : MonoBehaviour
{
    [Header("Bob Settings")]
    [SerializeField] private bool enableBob = true;
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.08f;
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerHiding playerHiding;

    [Header("FOV Change")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float sprintFOV = 70f;
    [SerializeField] private float fovTransitionSpeed = 10f;
    
    private float defaultYPos;
    private float timer;
    
    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = GetComponentInChildren<Camera>().transform;
        }
        
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
        
        if (playerHiding == null)
        {
            playerHiding = GetComponent<PlayerHiding>();
        }
        
        defaultYPos = cameraTransform.localPosition.y;

        if (mainCamera == null)
        {
            mainCamera = GetComponentInChildren<Camera>();
        }
        
        if (mainCamera != null)
        {
            normalFOV = mainCamera.fieldOfView;
        }
    }
    
    void Update()
    {
        if (!enableBob) return;
        if (cameraTransform == null || characterController == null) return;
        
        if (!characterController.enabled) return;
        
        // Stop head bob effect while hiding
        if (playerHiding != null && playerHiding.IsHiding())
        {
            timer = 0;
            Vector3 resetPos = cameraTransform.localPosition;
            resetPos.x = Mathf.Lerp(resetPos.x, 0f, Time.deltaTime * 10f);
            cameraTransform.localPosition = resetPos;
            return;
        }
        
        float speed = characterController.velocity.magnitude;
        
        bool isSprinting = UnityEngine.InputSystem.Keyboard.current != null && 
                          UnityEngine.InputSystem.Keyboard.current.leftShiftKey.isPressed;
        bool isCrouching = UnityEngine.InputSystem.Keyboard.current != null && 
                          (UnityEngine.InputSystem.Keyboard.current.cKey.isPressed || 
                           UnityEngine.InputSystem.Keyboard.current.leftCtrlKey.isPressed);
        
        float bobSpeed = walkBobSpeed;
        float bobAmount = walkBobAmount;
        
        if (isSprinting && !isCrouching)
        {
            bobSpeed = sprintBobSpeed;
            bobAmount = sprintBobAmount;
        }
        else if (isCrouching)
        {
            bobSpeed = crouchBobSpeed;
            bobAmount = crouchBobAmount;
        }
        
        float currentBaseY = cameraTransform.localPosition.y;
        
        if (speed > 0.1f && characterController.isGrounded)
        {
            timer += Time.deltaTime * bobSpeed;
            
            Vector3 newPos = cameraTransform.localPosition;
            newPos.y = currentBaseY + Mathf.Sin(timer) * bobAmount;
            newPos.x = Mathf.Cos(timer * 0.5f) * bobAmount * 0.5f; 
            cameraTransform.localPosition = newPos;
        }
        else
        {
            timer = 0;
            Vector3 newPos = cameraTransform.localPosition;
            newPos.x = Mathf.Lerp(newPos.x, 0f, Time.deltaTime * 5f);
            cameraTransform.localPosition = newPos;
        }
        
        if (mainCamera != null)
        {
            float targetFOV = (isSprinting && speed > 0.1f) ? sprintFOV : normalFOV;
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, 
                                               Time.deltaTime * fovTransitionSpeed);
        }
    }
    
    public void SetBobEnabled(bool enabled)
    {
        enableBob = enabled;
        
        if (!enabled && cameraTransform != null)
        {
            Vector3 resetPos = cameraTransform.localPosition;
            resetPos.y = defaultYPos;
            resetPos.x = 0f;
            cameraTransform.localPosition = resetPos;
            timer = 0f;
        }
    }
}