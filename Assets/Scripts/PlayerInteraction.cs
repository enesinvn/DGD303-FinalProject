using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Etkileşim Ayarları")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactableLayer;
    
    [Header("UI")]
    [SerializeField] private GameObject interactionPrompt;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugRay = true;
    
    private IInteractable currentInteractable;
    
    void Start()
    {
        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cameraTransform = cam.transform;
        }
    }
    
    void Update()
    {
        CheckForInteractable();
        
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (currentInteractable != null)
            {
                currentInteractable.Interact();
            }
        }
    }
    
    void CheckForInteractable()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            
            if (interactable != null)
            {
                if (currentInteractable != interactable)
                {
                    currentInteractable = interactable;
                    ShowInteractionPrompt(true);
                    Debug.Log($"Etkileşilebilir obje bulundu: {hit.collider.name}");
                }
                return;
            }
        }
        
        if (currentInteractable != null)
        {
            currentInteractable = null;
            ShowInteractionPrompt(false);
        }
    }
    
    void ShowInteractionPrompt(bool show)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(show);
        }
    }
    
    void OnDrawGizmos()
    {
        if (showDebugRay && cameraTransform != null)
        {
            Gizmos.color = currentInteractable != null ? Color.green : Color.yellow;
            Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * interactionRange);
        }
    }
}

public interface IInteractable
{
    void Interact();
}