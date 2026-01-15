using UnityEngine;

public class HidingSpot : MonoBehaviour, IInteractable
{
    public enum HidingType
    {
        Closet,
        UnderTable,
        Corner,
        Vent,
        Locker
    }
    
    [Header("Hiding Spot Settings")]
    [SerializeField] private HidingType hidingType = HidingType.Closet;
    [SerializeField] private Transform playerPosition;
    [SerializeField] private Transform exitPosition;
    [SerializeField] private bool hasCustomCamera = false;
    [SerializeField] private Vector3 cameraOffset = Vector3.zero;
    [SerializeField] private Vector3 cameraRotation = Vector3.zero;
    
    [Header("Status")]
    [SerializeField] private bool isOccupied = false;
    
    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    
    private PlayerHiding playerHiding;
    
    void Start()
    {
        if (playerPosition == null)
        {
            GameObject posObj = new GameObject("PlayerPosition");
            posObj.transform.SetParent(transform);
            posObj.transform.position = transform.position;
            posObj.transform.rotation = transform.rotation;
            playerPosition = posObj.transform;
            // Auto-created successfully (no warning needed)
        }
        
        if (exitPosition == null)
        {
            GameObject exitObj = new GameObject("ExitPosition");
            exitObj.transform.SetParent(transform);
            exitObj.transform.position = transform.position + transform.forward * 1.5f;
            exitObj.transform.rotation = transform.rotation;
            exitPosition = exitObj.transform;
            // Auto-created successfully (no warning needed)
        }
    }
    
    public void Interact()
    {
        Debug.Log($"[HidingSpot] Interact called. IsOccupied: {isOccupied}");
        
        if (playerHiding == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHiding = player.GetComponent<PlayerHiding>();
            }
            else
            {
                Debug.LogError("[HidingSpot] Player not found!");
                return;
            }
        }
        
        if (isOccupied)
        {
            Debug.Log($"[HidingSpot] Exiting hiding spot: {name}");
            playerHiding.ExitHidingSpot();
        }
        else
        {
            Debug.Log($"[HidingSpot] Entering hiding spot: {name}");
            SetOccupied(true);
            playerHiding.EnterHidingSpot(this);
        }
    }
    
    public bool IsOccupied()
    {
        return isOccupied;
    }
    
    public void SetOccupied(bool occupied)
    {
        Debug.Log($"[HidingSpot] {name} SetOccupied: {occupied}");
        isOccupied = occupied;
    }
    
    public HidingType GetHidingType()
    {
        return hidingType;
    }
    
    public Vector3 GetPlayerPosition()
    {
        if (playerPosition != null)
        {
            Vector3 pos = playerPosition.position;
            if (pos.y < 0.1f)
            {
                pos.y = Mathf.Max(transform.position.y, 0.1f);
            }
            return pos;
        }
        
        Vector3 fallbackPos = transform.position;
        fallbackPos.y = Mathf.Max(transform.position.y, 0.1f);
        return fallbackPos;
    }
    
    public Quaternion GetPlayerRotation()
    {
        return playerPosition != null ? playerPosition.rotation : transform.rotation;
    }
    
    public Vector3 GetExitPosition()
    {
        if (exitPosition != null)
        {
            Vector3 pos = exitPosition.position;
            if (pos.y < 0.1f)
            {
                pos.y = Mathf.Max(transform.position.y, 0.1f);
            }
            return pos;
        }
        
        Vector3 fallbackPos = transform.position + transform.forward * 1.5f;
        fallbackPos.y = Mathf.Max(transform.position.y, 0.1f);
        return fallbackPos;
    }
    
    public bool HasCustomCameraPosition()
    {
        return hasCustomCamera;
    }
    
    public Vector3 GetCameraOffset()
    {
        return cameraOffset;
    }
    
    public Vector3 GetCameraRotation()
    {
        return cameraRotation;
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        if (playerPosition != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(playerPosition.position, 0.3f);
            Gizmos.DrawLine(transform.position, playerPosition.position);
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(playerPosition.position, playerPosition.forward * 0.5f);
        }
        
        if (exitPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(exitPosition.position, 0.3f);
            Gizmos.DrawLine(transform.position, exitPosition.position);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(exitPosition.position, exitPosition.forward * 0.5f);
        }
        
        Gizmos.color = isOccupied ? Color.red : Color.white;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        
        if (playerPosition != null && exitPosition != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(playerPosition.position, exitPosition.position);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position, Vector3.one);
    }
}