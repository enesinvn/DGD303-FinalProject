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
    Key,           // Anahtar
    Battery,       // Batarya
    HealthPack,    // Sağlık paketi
    Distraction,   // Dikkat dağıtıcı
    Tool,          // Alet
    QuestItem,     // Görev eşyası
    Misc           // Diğer
}

public class InventorySystem : MonoBehaviour
{
    [Header("Envanter Ayarları")]
    [SerializeField] private int maxSlots = 12;
    [SerializeField] private List<InventoryItem> inventory = new List<InventoryItem>();
    
    [Header("UI")]
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private bool isInventoryOpen = false;
    
    [Header("Referanslar")]
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
        // Tab tuşu ile envanter aç/kapa
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }
    
    public bool AddItem(string itemName, string description, Sprite icon, ItemType type, int quantity = 1, bool usable = false)
    {
        // Aynı eşya varsa miktarını artır
        InventoryItem existingItem = inventory.Find(item => item.itemName == itemName && item.itemType == type);
        
        if (existingItem != null)
        {
            existingItem.quantity += quantity;
            OnInventoryChanged?.Invoke();
            Debug.Log($"{itemName} eklendi (Toplam: {existingItem.quantity})");
            return true;
        }
        
        // Envanter dolu mu kontrol et
        if (inventory.Count >= maxSlots)
        {
            Debug.LogWarning("Envanter dolu!");
            return false;
        }
        
        // Yeni eşya ekle
        InventoryItem newItem = new InventoryItem(itemName, description, icon, type, quantity, usable);
        inventory.Add(newItem);
        
        OnInventoryChanged?.Invoke();
        Debug.Log($"{itemName} envantere eklendi!");
        
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
                    flashlight.RechargeBattery(50f); // %50 şarj
                    used = true;
                }
                break;
                
            case ItemType.HealthPack:
                if (healthSystem != null)
                {
                    healthSystem.Heal(30f); // 30 HP
                    used = true;
                }
                break;
                
            case ItemType.Distraction:
                // Distraction sistemi eklendiğinde kullanılacak
                used = true;
                break;
        }
        
        if (used)
        {
            RemoveItem(itemName, 1);
            Debug.Log($"{itemName} kullanıldı!");
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
        
        // Mouse kontrolü
        if (isInventoryOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
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
}

