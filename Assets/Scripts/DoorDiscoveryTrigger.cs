using UnityEngine;

/// <summary>
/// Activates the key objective when player finds the locked door
/// </summary>
public class DoorDiscoveryTrigger : MonoBehaviour
{
    [Header("Objective to Activate")]
    [SerializeField] private string objectiveIDToActivate = "collect_hidden_room_key";
    
    [Header("References")]
    [SerializeField] private ObjectiveSystem objectiveSystem;
    
    [Header("Settings")]
    [SerializeField] private bool oneTimeOnly = true;
    
    private bool hasTriggered = false;
    
    void Start()
    {
        if (objectiveSystem == null)
        {
            objectiveSystem = ObjectiveSystem.Instance;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (oneTimeOnly && hasTriggered) return;
            
            if (objectiveSystem == null)
            {
                Debug.LogWarning("[DoorDiscovery] ObjectiveSystem not found!");
                return;
            }
            
            Debug.Log("[DoorDiscovery] Player found the locked door!");
            
            // Activate "Find Hidden Room Key" objective
            objectiveSystem.ActivateObjective(objectiveIDToActivate);
            Debug.Log($"[DoorDiscovery] Activated objective: {objectiveIDToActivate}");
            
            hasTriggered = true;
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = hasTriggered ? Color.green : Color.cyan;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
