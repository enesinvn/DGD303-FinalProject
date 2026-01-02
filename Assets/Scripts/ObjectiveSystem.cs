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
    CollectItem,    // Eşya topla
    ReachLocation,  // Konuma ulaş
    UnlockDoor,     // Kapı aç
    UseItem,        // Eşya kullan
    Escape,         // Kaç
    Custom          // Özel
}

public class ObjectiveSystem : MonoBehaviour
{
    [Header("Görevler")]
    [SerializeField] private List<Objective> objectives = new List<Objective>();
    
    [Header("UI")]
    [SerializeField] private GameObject objectiveUI;
    [SerializeField] private TextMeshProUGUI objectiveTitleText;
    [SerializeField] private TextMeshProUGUI objectiveDescriptionText;
    
    [Header("Referanslar")]
    [SerializeField] private InventorySystem inventorySystem;
    
    public delegate void ObjectiveCompletedDelegate(string objectiveID);
    public event ObjectiveCompletedDelegate OnObjectiveCompleted;
    
    public delegate void ObjectiveAddedDelegate(Objective objective);
    public event ObjectiveAddedDelegate OnObjectiveAdded;
    
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
        // Aktif görevleri kontrol et
        CheckObjectives();
    }
    
    public void AddObjective(string id, string title, string description, ObjectiveType type)
    {
        Objective newObjective = new Objective(id, title, description, type);
        newObjective.isActive = true;
        objectives.Add(newObjective);
        
        OnObjectiveAdded?.Invoke(newObjective);
        UpdateUI();
        
        Debug.Log($"Yeni görev: {title}");
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
            
            Debug.Log($"Görev tamamlandı: {obj.title}");
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
                        // Eşya toplama kontrolü (örnek: "Key" eşyası)
                        if (inventorySystem != null)
                        {
                            // Objective ID'den eşya adını çıkar (örnek: "collect_key" -> "Key")
                            string itemName = ExtractItemNameFromID(obj.objectiveID);
                            if (inventorySystem.HasItem(itemName))
                            {
                                completed = true;
                            }
                        }
                        break;
                        
                    case ObjectiveType.ReachLocation:
                        // Konum kontrolü (ObjectiveTrigger ile yapılacak)
                        break;
                        
                    case ObjectiveType.UnlockDoor:
                        // Kapı açma kontrolü (LockSystem ile yapılacak)
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
        // Basit bir çıkarım (örnek: "collect_key" -> "Key")
        if (id.Contains("key"))
            return "Key";
        if (id.Contains("battery"))
            return "Battery";
        if (id.Contains("card"))
            return "Keycard";
        
        return "Item";
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

