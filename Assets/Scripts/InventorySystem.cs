using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public string itemName;
    public string itemDescription;
    public Sprite itemIcon;
    public ItemType itemType;
    public int quantity;
    public bool isUsable;
    
    public InventoryItem(string name, string desc, Sprite icon, ItemType type, int qty = 1, bool usable = false)
    {
        itemName = name;
        itemDescription = desc;
        itemIcon = icon;
        itemType = type;
        quantity = qty;
        isUsable = usable;
    }
}

public enum ItemType
{
    Key,
    Battery,
    HealthPack,
    Distraction,
    Tool,
    QuestItem,
    Misc
}

public class InventorySystem : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int maxSlots = 12;
    [SerializeField] private List<InventoryItem> inventory = new List<InventoryItem>();
    
    [Header("UI")]
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private bool isInventoryOpen = false;
    
    [Header("References")]
    [SerializeField] private Flashlight flashlight;
    [SerializeField] private HealthSystem healthSystem;
    
    public delegate void InventoryChangedDelegate();
    public event InventoryChangedDelegate OnInventoryChanged;
    
    void Start()
    {
        if (flashlight == null)
        {
            flashlight = FindFirstObjectByType<Flashlight>();
        }
        
        if (healthSystem == null)
        {
            healthSystem = FindFirstObjectByType<HealthSystem>();
        }
        
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }
    
    public bool AddItem(string itemName, string description, Sprite icon, ItemType type, int quantity = 1, bool usable = false)
    {
        InventoryItem existingItem = inventory.Find(item => item.itemName == itemName && item.itemType == type);
        
        if (existingItem != null)
        {
            existingItem.quantity += quantity;
            OnInventoryChanged?.Invoke();
            Debug.Log($"{itemName} added (Total: {existingItem.quantity})");
            return true;
        }
        
        if (inventory.Count >= maxSlots)
        {
            Debug.LogWarning("Inventory full!");
            return false;
        }
        
        InventoryItem newItem = new InventoryItem(itemName, description, icon, type, quantity, usable);
        inventory.Add(newItem);
        
        OnInventoryChanged?.Invoke();
        Debug.Log($"{itemName} added to inventory!");
        
        return true;
    }
    
    public bool RemoveItem(string itemName, int quantity = 1)
    {
        InventoryItem item = inventory.Find(i => i.itemName == itemName);
        
        if (item != null)
        {
            item.quantity -= quantity;
            
            if (item.quantity <= 0)
            {
                inventory.Remove(item);
            }
            
            OnInventoryChanged?.Invoke();
            return true;
        }
        
        return false;
    }
    
    public bool HasItem(string itemName, int requiredQuantity = 1)
    {
        InventoryItem item = inventory.Find(i => i.itemName == itemName);
        return item != null && item.quantity >= requiredQuantity;
    }
    
    public int GetItemQuantity(string itemName)
    {
        InventoryItem item = inventory.Find(i => i.itemName == itemName);
        return item != null ? item.quantity : 0;
    }
    
    public bool UseItem(string itemName)
    {
        InventoryItem item = inventory.Find(i => i.itemName == itemName);
        
        if (item == null || !item.isUsable)
        {
            return false;
        }
        
        bool used = false;
        
        switch (item.itemType)
        {
            case ItemType.Battery:
                if (flashlight != null)
                {
                    flashlight.RechargeBattery(50f);
                    used = true;
                }
                break;
                
            case ItemType.HealthPack:
                if (healthSystem != null)
                {
                    healthSystem.Heal(30f);
                    used = true;
                }
                break;
                
            case ItemType.Distraction:
                used = UseDistractionItem(itemName);
                break;
        }
        
        if (used)
        {
            RemoveItem(itemName, 1);
            Debug.Log($"{itemName} used!");
        }
        
        return used;
    }
    
    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(isInventoryOpen);
        }
        
        if (isInventoryOpen)
        {
            OnInventoryChanged?.Invoke();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        Debug.Log($"Inventory {(isInventoryOpen ? "opened" : "closed")}");
    }
    
    public List<InventoryItem> GetInventory()
    {
        return new List<InventoryItem>(inventory);
    }
    
    public int GetItemCount()
    {
        return inventory.Count;
    }
    
    public bool IsInventoryFull()
    {
        return inventory.Count >= maxSlots;
    }
    
    public bool IsInventoryOpen()
    {
        return isInventoryOpen;
    }
    
    bool UseDistractionItem(string itemName)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;
        
        Transform playerCamera = player.GetComponentInChildren<Camera>()?.transform;
        if (playerCamera == null) return false;
        
        GameObject distractionObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        distractionObj.name = itemName;
        distractionObj.transform.position = playerCamera.position + playerCamera.forward * 1f;
        distractionObj.transform.localScale = Vector3.one * 0.3f;
        
        DistractionItem distractionItem = distractionObj.AddComponent<DistractionItem>();
        if (distractionItem != null)
        {
            Rigidbody rb = distractionObj.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = distractionObj.AddComponent<Rigidbody>();
            }
            rb.mass = 0.5f;
            rb.isKinematic = false;
            
            Vector3 throwDirection = playerCamera.forward;
            rb.AddForce(throwDirection * 10f + Vector3.up * 2f, ForceMode.Impulse);
            
            Collider col = distractionObj.GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = false;
            }
            
            distractionItem.isThrown = true;
            
            PlayerSoundController soundController = player.GetComponent<PlayerSoundController>();
            if (soundController != null)
            {
                soundController.EmitThrowSound(0.8f);
            }
            
            Debug.Log($"{itemName} thrown!");
            return true;
        }
        
        Destroy(distractionObj);
        return false;
    }
}

