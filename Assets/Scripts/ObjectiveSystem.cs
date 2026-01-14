using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class Objective
{
    public string objectiveID;
    public string title;
    public string description;
    public bool isCompleted;
    public bool isActive;
    public ObjectiveType type;
    
    public Objective(string id, string t, string desc, ObjectiveType objType)
    {
        objectiveID = id;
        title = t;
        description = desc;
        type = objType;
        isCompleted = false;
        isActive = false;
    }
}

public enum ObjectiveType
{
    CollectItem,
    ReachLocation,
    UnlockDoor,
    UseItem,
    Escape,
    Custom
}

public class ObjectiveSystem : MonoBehaviour
{
    private static ObjectiveSystem instance;
    public static ObjectiveSystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<ObjectiveSystem>();
            }
            return instance;
        }
    }
    
    [Header("Objectives")]
    [SerializeField] private List<Objective> objectives = new List<Objective>();
    
    [Header("UI")]
    [SerializeField] private GameObject objectiveUI;
    [SerializeField] private TextMeshProUGUI objectiveTitleText;
    [SerializeField] private TextMeshProUGUI objectiveDescriptionText;
    
    [Header("References")]
    [SerializeField] private InventorySystem inventorySystem;
    
    public delegate void ObjectiveCompletedDelegate(string objectiveID);
    public event ObjectiveCompletedDelegate OnObjectiveCompleted;
    
    public delegate void AllObjectivesCompletedDelegate();
    public event AllObjectivesCompletedDelegate OnAllObjectivesCompleted;
    
    public delegate void ObjectiveAddedDelegate(Objective objective);
    public event ObjectiveAddedDelegate OnObjectiveAdded;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        if (inventorySystem == null)
        {
            inventorySystem = FindFirstObjectByType<InventorySystem>();
        }
        
        if (objectiveUI != null)
        {
            objectiveUI.SetActive(false);
        }
    }
    
    void Update()
    {
        CheckObjectives();
    }
    
    public void AddObjective(string id, string title, string description, ObjectiveType type)
    {
        Objective newObjective = new Objective(id, title, description, type);
        newObjective.isActive = true;
        objectives.Add(newObjective);
        
        OnObjectiveAdded?.Invoke(newObjective);
        UpdateUI();
        
        Debug.Log($"New objective: {title}");
    }
    
    public void CompleteObjective(string objectiveID)
    {
        Objective obj = objectives.Find(o => o.objectiveID == objectiveID);
        
        if (obj != null && !obj.isCompleted)
        {
            obj.isCompleted = true;
            obj.isActive = false;
            
            OnObjectiveCompleted?.Invoke(objectiveID);
            UpdateUI();
            
            // Check if all objectives are completed
            bool allCompleted = objectives.TrueForAll(o => o.isCompleted);
            if (allCompleted && objectives.Count > 0)
            {
                OnAllObjectivesCompleted?.Invoke();
            }
            
            Debug.Log($"Objective completed: {obj.title}");
        }
    }
    
    public void ActivateObjective(string objectiveID)
    {
        Objective obj = objectives.Find(o => o.objectiveID == objectiveID);
        
        if (obj != null)
        {
            obj.isActive = true;
            UpdateUI();
        }
    }
    
    void CheckObjectives()
    {
        foreach (Objective obj in objectives)
        {
            if (obj.isActive && !obj.isCompleted)
            {
                bool completed = false;
                
                switch (obj.type)
                {
                    case ObjectiveType.CollectItem:
                        if (inventorySystem != null)
                        {
                            string itemName = ExtractItemNameFromID(obj.objectiveID);
                            if (inventorySystem.HasItem(itemName))
                            {
                                completed = true;
                            }
                        }
                        break;
                        
                    case ObjectiveType.ReachLocation:
                        // ReachLocation is handled by ObjectiveTrigger
                        break;
                        
                    case ObjectiveType.UnlockDoor:
                        // UnlockDoor is handled by Door/LockSystem
                        break;
                        
                    case ObjectiveType.Escape:
                        // Escape is handled by ObjectiveTrigger (exit zone)
                        break;
                }
                
                if (completed)
                {
                    CompleteObjective(obj.objectiveID);
                }
            }
        }
    }
    
    string ExtractItemNameFromID(string id)
    {
        id = id.ToLower();
        
        if (id.Contains("key") && !id.Contains("keycard"))
            return "Key";
        if (id.Contains("master_key"))
            return "Master Key";
        if (id.Contains("keycard") || id.Contains("card"))
            return "Keycard";
        if (id.Contains("battery"))
            return "Battery";
        if (id.Contains("health"))
            return "Health Pack";
        
        if (id.Contains("collect_"))
        {
            string itemName = id.Replace("collect_", "");
            if (itemName.Length > 0)
            {
                itemName = char.ToUpper(itemName[0]) + itemName.Substring(1);
            }
            return itemName;
        }
        
        return "Item";
    }
    
    public void OnItemCollected(string itemName)
    {
        foreach (Objective obj in objectives)
        {
            if (obj.isActive && !obj.isCompleted && obj.type == ObjectiveType.CollectItem)
            {
                string requiredItem = ExtractItemNameFromID(obj.objectiveID);
                if (requiredItem.Equals(itemName, System.StringComparison.OrdinalIgnoreCase))
                {
                    CompleteObjective(obj.objectiveID);
                    Debug.Log($"[ObjectiveSystem] Objective completed: {obj.title} (Item: {itemName})");
                }
            }
        }
    }
    
    public void OnDoorUnlocked(string doorID = "")
    {
        foreach (Objective obj in objectives)
        {
            if (obj.isActive && !obj.isCompleted && obj.type == ObjectiveType.UnlockDoor)
            {
                if (string.IsNullOrEmpty(doorID) || obj.objectiveID.Contains(doorID))
                {
                    CompleteObjective(obj.objectiveID);
                    Debug.Log($"[ObjectiveSystem] Objective completed: {obj.title}");
                }
            }
        }
    }
    
    void UpdateUI()
    {
        Objective activeObjective = objectives.Find(o => o.isActive && !o.isCompleted);
        
        if (activeObjective != null)
        {
            if (objectiveUI != null)
            {
                objectiveUI.SetActive(true);
            }
            
            if (objectiveTitleText != null)
            {
                objectiveTitleText.text = activeObjective.title;
            }
            
            if (objectiveDescriptionText != null)
            {
                objectiveDescriptionText.text = activeObjective.description;
            }
        }
        else
        {
            if (objectiveUI != null)
            {
                objectiveUI.SetActive(false);
            }
        }
    }
    
    public Objective GetActiveObjective()
    {
        return objectives.Find(o => o.isActive && !o.isCompleted);
    }
    
    public List<Objective> GetAllObjectives()
    {
        return new List<Objective>(objectives);
    }
    
    public bool IsObjectiveCompleted(string objectiveID)
    {
        Objective obj = objectives.Find(o => o.objectiveID == objectiveID);
        return obj != null && obj.isCompleted;
    }
}

