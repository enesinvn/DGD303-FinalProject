using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private float hidingSpotRange = 1.5f; // Shorter range for hiding
    [SerializeField] private float hidingSpotAngle = 45f; // Narrower angle for hiding
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private LayerMask obstacleLayer; // For wall checks, etc.
    
    [Header("UI")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI interactionText;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugRay = true;
    
    [Header("References")]
    [SerializeField] private PlayerHiding playerHiding;
    
    private IInteractable currentInteractable;
    private GameObject currentInteractableObject;
    
    void Start()
    {
        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cameraTransform = cam.transform;
        }
        
        if (playerHiding == null)
        {
            playerHiding = GetComponent<PlayerHiding>();
        }
    }
    
    void Update()
    {
        if (playerHiding != null && playerHiding.IsHiding())
        {
            HandleHidingInteraction();
            return;
        }
        
        CheckForInteractable();
        
        if (currentInteractable != null)
        {
            UpdateInteractionPrompt();
        }
        
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (currentInteractable != null)
            {
                currentInteractable.Interact();
            }
        }
    }
    
    void HandleHidingInteraction()
    {
        HidingSpot currentHidingSpot = playerHiding.GetCurrentHidingSpot();
        
        if (currentHidingSpot != null)
        {
            if (interactionText != null)
            {
                interactionText.text = "E - Exit Hiding Spot";
            }
            ShowInteractionPrompt(true);
            
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                currentHidingSpot.Interact();
            }
        }
        else
        {
            ShowInteractionPrompt(false);
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
                // Distance check for HidingSpot
                HidingSpot hidingSpot = hit.collider.GetComponent<HidingSpot>();
                if (hidingSpot != null && hit.distance > hidingSpotRange)
                {
                    // HidingSpot too far away, continue
                }
                else
                {
                    if (currentInteractable != interactable)
                    {
                        currentInteractable = interactable;
                        currentInteractableObject = hit.collider.gameObject;
                        UpdateInteractionPrompt();
                        ShowInteractionPrompt(true);
                    }
                    return;
                }
            }
        }
        
        // Use normal interaction range as maximum distance
        float searchRadius = interactionRange * 0.5f;
        Collider[] nearbyColliders = Physics.OverlapSphere(cameraTransform.position + cameraTransform.forward * searchRadius, searchRadius);
        
        IInteractable closestInteractable = null;
        GameObject closestObject = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider col in nearbyColliders)
        {
            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable != null)
            {
                Vector3 directionToObject = (col.transform.position - cameraTransform.position).normalized;
                float angle = Vector3.Angle(cameraTransform.forward, directionToObject);
                float distance = Vector3.Distance(cameraTransform.position, col.transform.position);
                
                // Special checks for HidingSpot
                HidingSpot hidingSpot = col.GetComponent<HidingSpot>();
                if (hidingSpot != null)
                {
                    // Shorter range and narrower angle for hiding
                    if (angle > hidingSpotAngle || distance > hidingSpotRange)
                    {
                        continue;
                    }
                    
                    // Check if there's a wall in between
                    if (!HasLineOfSight(cameraTransform.position, col.transform.position))
                    {
                        Debug.Log($"[PlayerInteraction] HidingSpot {col.name} blocked by obstacle!");
                        continue;
                    }
                }
                else
                {
                    // Standard angle check for normal objects
                    if (angle > 60f || distance > interactionRange)
                    {
                        continue;
                    }
                }
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                    closestObject = col.gameObject;
                }
            }
        }
        
        if (closestInteractable != null)
        {
            if (currentInteractable != closestInteractable)
            {
                currentInteractable = closestInteractable;
                currentInteractableObject = closestObject;
                UpdateInteractionPrompt();
                ShowInteractionPrompt(true);
            }
        }
        else
        {
            if (currentInteractable != null)
            {
                currentInteractable = null;
                currentInteractableObject = null;
                ShowInteractionPrompt(false);
            }
        }
    }
    
    bool HasLineOfSight(Vector3 from, Vector3 to)
    {
        Vector3 direction = to - from;
        float distance = direction.magnitude;
        
        // Check if there's an obstacle in between using raycast
        RaycastHit hit;
        if (Physics.Raycast(from, direction.normalized, out hit, distance, ~0, QueryTriggerInteraction.Ignore))
        {
            // If raycast hit something before reaching the target
            // and the hit object is not the hiding spot itself
            HidingSpot hitSpot = hit.collider.GetComponent<HidingSpot>();
            if (hitSpot == null)
            {
                // There's an obstacle in between
                return false;
            }
        }
        
        return true;
    }
    
    void UpdateInteractionPrompt()
    {
        if (interactionText == null || currentInteractableObject == null) return;
        
        string promptText = "E - Interact";
        
        Door door = currentInteractableObject.GetComponent<Door>();
        LockedDoor lockedDoor = currentInteractableObject.GetComponent<LockedDoor>();
        LockSystem lockSystem = currentInteractableObject.GetComponent<LockSystem>();
        
        if (door != null || lockedDoor != null)
        {
            bool isLocked = false;
            
            if (lockSystem != null)
            {
                isLocked = lockSystem.IsLocked();
            }
            else if (door != null)
            {
                isLocked = door.IsLocked;
            }
            
            if (isLocked)
            {
                InventorySystem inventory = FindFirstObjectByType<InventorySystem>();
                string requiredKey = "";
                
                if (lockSystem != null)
                {
                    requiredKey = lockSystem.GetRequiredKeyName();
                }
                
                if (inventory != null)
                {
                    bool hasKey = string.IsNullOrEmpty(requiredKey) ? 
                        (inventory.HasItem("Key") || inventory.HasItem("Master Key")) : 
                        inventory.HasItem(requiredKey);
                    
                    if (hasKey)
                    {
                        promptText = "E - Unlock";
                    }
                    else
                    {
                        promptText = string.IsNullOrEmpty(requiredKey) ? 
                            "E - Locked (Key Required)" : 
                            $"E - Locked ({requiredKey} Required)";
                    }
                }
                else
                {
                    promptText = "E - Locked";
                }
            }
            else
            {
                if (door != null)
                {
                    promptText = door.IsOpen ? "E - Close Door" : "E - Open Door";
                }
                else
                {
                    promptText = "E - Open/Close Door";
                }
            }
        }
        else if (currentInteractableObject.GetComponent<HidingSpot>() != null)
        {
            HidingSpot hidingSpot = currentInteractableObject.GetComponent<HidingSpot>();
            if (hidingSpot.IsOccupied())
            {
                promptText = "E - Exit Hiding Spot";
            }
            else
            {
                switch (hidingSpot.GetHidingType())
                {
                    case HidingSpot.HidingType.Closet:
                        promptText = "E - Hide in Closet";
                        break;
                    case HidingSpot.HidingType.UnderTable:
                        promptText = "E - Hide Under Table";
                        break;
                    case HidingSpot.HidingType.Corner:
                        promptText = "E - Hide in Corner";
                        break;
                    case HidingSpot.HidingType.Vent:
                        promptText = "E - Enter Vent";
                        break;
                    case HidingSpot.HidingType.Locker:
                        promptText = "E - Hide in Locker";
                        break;
                    default:
                        promptText = "E - Hide";
                        break;
                }
            }
        }
        else if (currentInteractableObject.GetComponent<PickupItem>() != null)
        {
            PickupItem pickup = currentInteractableObject.GetComponent<PickupItem>();
            
            switch (pickup.pickupType)
            {
                case PickupType.Key:
                    promptText = "E - Pick up Key";
                    break;
                case PickupType.Battery:
                    promptText = "E - Pick up Battery";
                    break;
                case PickupType.HealthPack:
                    promptText = "E - Pick up Health Pack";
                    break;
                default:
                    promptText = "E - Pick up";
                    break;
            }
            
            InventorySystem inventory = FindFirstObjectByType<InventorySystem>();
            if (inventory != null && inventory.IsInventoryFull() && pickup.addToInventory)
            {
                promptText += " (Inventory Full!)";
            }
        }
        else if (currentInteractableObject.GetComponent<DistractionItem>() != null)
        {
            promptText = "E - Throw";
        }
        
        interactionText.text = promptText;
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
            // Normal etkileşim mesafesi
            Gizmos.color = currentInteractable != null ? Color.green : Color.yellow;
            Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * interactionRange);
            
            // Saklanma mesafesi (daha kısa)
            HidingSpot currentHiding = currentInteractableObject != null ? currentInteractableObject.GetComponent<HidingSpot>() : null;
            if (currentHiding != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(cameraTransform.position, hidingSpotRange);
                Gizmos.DrawLine(cameraTransform.position, currentInteractableObject.transform.position);
            }
        }
    }
}

public interface IInteractable
{
    void Interact();
}