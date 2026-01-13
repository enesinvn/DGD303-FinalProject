using UnityEngine;

public class ObjectiveTrigger : MonoBehaviour
{
    [Header("Objective Settings")]
    [SerializeField] private string objectiveID;
    [SerializeField] private bool completeOnEnter = true;
    [SerializeField] private bool oneTimeOnly = true;
    
    [Header("References")]
    [SerializeField] private ObjectiveSystem objectiveSystem;
    
    private bool hasTriggered = false;
    
    void Start()
    {
        if (objectiveSystem == null)
        {
            objectiveSystem = FindFirstObjectByType<ObjectiveSystem>();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (oneTimeOnly && hasTriggered)
            {
                return;
            }
            
            if (objectiveSystem != null)
            {
                if (completeOnEnter)
                {
                    objectiveSystem.CompleteObjective(objectiveID);
                }
                else
                {
                    objectiveSystem.ActivateObjective(objectiveID);
                }
                
                hasTriggered = true;
            }
        }
    }
    
    public void TriggerObjective()
    {
        if (objectiveSystem != null)
        {
            objectiveSystem.CompleteObjective(objectiveID);
        }
    }
}

