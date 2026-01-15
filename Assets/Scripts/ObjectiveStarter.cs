using UnityEngine;

/// <summary>
/// Starts objectives at game start or under specific conditions.
/// You can attach this script to a GameObject and configure objectives in the Inspector.
/// </summary>
public class ObjectiveStarter : MonoBehaviour
{
    [Header("Objective Settings")]
    [SerializeField] private bool startOnAwake = true;
    [SerializeField] private bool startOnTrigger = false;
    [SerializeField] private bool requirePlayerTag = true;
    
    [Header("Initial Objectives")]
    [SerializeField] private ObjectiveData[] initialObjectives;
    
    [Header("References")]
    [SerializeField] private ObjectiveSystem objectiveSystem;
    
    [System.Serializable]
    public class ObjectiveData
    {
        public string objectiveID;
        public string title;
        public string description;
        public ObjectiveType type;
        public bool startActive = true;
    }
    
    void Awake()
    {
        if (objectiveSystem == null)
        {
            objectiveSystem = ObjectiveSystem.Instance;
        }
        
        if (startOnAwake)
        {
            StartObjectives();
        }
    }
    
    void StartObjectives()
    {
        if (objectiveSystem == null)
        {
            Debug.LogWarning("ObjectiveSystem not found! Cannot start objectives.");
            return;
        }
        
        if (initialObjectives == null || initialObjectives.Length == 0)
        {
            Debug.LogWarning("No objectives defined in ObjectiveStarter!");
            return;
        }
        
        foreach (ObjectiveData objData in initialObjectives)
        {
            if (objData.startActive)
            {
                objectiveSystem.AddObjective(
                    objData.objectiveID,
                    objData.title,
                    objData.description,
                    objData.type
                );
            }
        }
        
        Debug.Log($"Started {initialObjectives.Length} objective(s)");
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (startOnTrigger)
        {
            if (requirePlayerTag && !other.CompareTag("Player"))
            {
                return;
            }
            
            StartObjectives();
            // Disable after first trigger
            startOnTrigger = false;
        }
    }
    
    /// <summary>
    /// Manually start objectives from script
    /// </summary>
    public void TriggerStartObjectives()
    {
        StartObjectives();
    }
    
    /// <summary>
    /// Start a specific objective
    /// </summary>
    public void StartSpecificObjective(string objectiveID)
    {
        if (objectiveSystem == null)
        {
            objectiveSystem = ObjectiveSystem.Instance;
        }
        
        ObjectiveData objData = System.Array.Find(initialObjectives, o => o.objectiveID == objectiveID);
        if (objData != null)
        {
            objectiveSystem.AddObjective(
                objData.objectiveID,
                objData.title,
                objData.description,
                objData.type
            );
        }
    }
}
