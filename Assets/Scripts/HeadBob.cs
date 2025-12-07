using UnityEngine;

public class HeadBob : MonoBehaviour
{
    [Header("Bob Ayarları")]
    [SerializeField] private bool enableBob = true;
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.08f;
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    
    [Header("Referanslar")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CharacterController characterController;

    [Header("FOV Değişimi")]
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
        
        if (speed > 0.1f && characterController.isGrounded)
        {
            timer += Time.deltaTime * bobSpeed;
            
            Vector3 newPos = cameraTransform.localPosition;
            newPos.y = defaultYPos + Mathf.Sin(timer) * bobAmount;
            newPos.x = Mathf.Cos(timer * 0.5f) * bobAmount * 0.5f; 
            cameraTransform.localPosition = newPos;
        }
        else
        {
            timer = 0;
            Vector3 newPos = cameraTransform.localPosition;
            newPos.y = Mathf.Lerp(newPos.y, defaultYPos, Time.deltaTime * 5f);
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
    }
}