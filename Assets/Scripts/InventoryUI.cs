using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform itemSlotContainer;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private Image itemIconImage;
    [SerializeField] private Button useButton;
    [SerializeField] private Button dropButton;
    
    [Header("References")]
    [SerializeField] private InventorySystem inventorySystem;
    
    private List<GameObject> slotObjects = new List<GameObject>();
    private InventoryItem selectedItem;
    
    void Start()
    {
        if (inventorySystem == null)
        {
            inventorySystem = FindFirstObjectByType<InventorySystem>();
        }
        
        if (inventorySystem != null)
        {
            inventorySystem.OnInventoryChanged += UpdateInventoryUI;
        }
        
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            FixInventoryUIAppearance();
        }
        
        if (useButton != null)
        {
            useButton.onClick.AddListener(UseSelectedItem);
        }
        
        if (dropButton != null)
        {
            dropButton.onClick.AddListener(DropSelectedItem);
        }
        
        ClearItemInfo();
        
        UpdateInventoryUI();
    }
    
    void FixInventoryUIAppearance()
    {
        if (inventoryPanel == null) return;
        
        RectTransform panelRect = inventoryPanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.sizeDelta = new Vector2(900, 600);
        }
        
        Image backgroundImage = inventoryPanel.GetComponentInChildren<Image>();
        if (backgroundImage != null && backgroundImage.gameObject.name == "Background")
        {
            Color bgColor = backgroundImage.color;
            bgColor.a = 0.9f;
            backgroundImage.color = bgColor;
            
            RectTransform bgRect = backgroundImage.GetComponent<RectTransform>();
            if (bgRect != null)
            {
                bgRect.sizeDelta = new Vector2(900, 600);
            }
        }
        
        if (itemSlotContainer != null)
        {
            RectTransform containerRect = itemSlotContainer.GetComponent<RectTransform>();
            if (containerRect != null)
            {
                containerRect.sizeDelta = new Vector2(350, 350);
            }
            
            UnityEngine.UI.GridLayoutGroup gridLayout = itemSlotContainer.GetComponent<UnityEngine.UI.GridLayoutGroup>();
            if (gridLayout != null)
            {
                gridLayout.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = 3;
                gridLayout.cellSize = new Vector2(100, 100);
                gridLayout.spacing = new Vector2(10, 10);
            }
        }
    }
    
    void Update()
    {
        if (inventorySystem != null)
        {
            bool shouldBeOpen = inventorySystem.IsInventoryOpen();
            
            if (inventoryPanel != null)
            {
                if (shouldBeOpen && !inventoryPanel.activeSelf)
                {
                    inventoryPanel.SetActive(true);
                    UpdateInventoryUI();
                }
                else if (!shouldBeOpen && inventoryPanel.activeSelf)
                {
                    inventoryPanel.SetActive(false);
                }
            }
        }
    }
    
    void UpdateInventoryUI()
    {
        if (inventorySystem == null || itemSlotContainer == null) 
        {
            Debug.LogWarning("InventorySystem or ItemSlotContainer not found!");
            return;
        }
        
        foreach (GameObject slot in slotObjects)
        {
            if (slot != null)
            {
                Destroy(slot);
            }
        }
        slotObjects.Clear();
        
        List<InventoryItem> items = inventorySystem.GetInventory();
        
        for (int i = 0; i < inventorySystem.maxSlots; i++)
        {
            GameObject slotObj;
            
            if (itemSlotPrefab != null)
            {
                slotObj = Instantiate(itemSlotPrefab, itemSlotContainer);
                slotObj.name = $"Slot_{i}";
            }
            else
            {
                slotObj = new GameObject($"Slot_{i}");
                slotObj.transform.SetParent(itemSlotContainer);
                
                RectTransform rectTransform = slotObj.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(80, 80);
                
                Image bgImage = slotObj.AddComponent<Image>();
                bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            }
            
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            if (slot == null)
            {
                slot = slotObj.AddComponent<InventorySlot>();
            }
            
            if (i < items.Count)
            {
                slot.SetItem(items[i], this);
            }
            else
            {
                slot.SetEmpty();
            }
            
            slotObjects.Add(slotObj);
        }
        
        Debug.Log($"Inventory UI updated: {items.Count} items, {inventorySystem.maxSlots} slots");
        
        if (selectedItem != null)
        {
            ShowItemInfo(selectedItem);
        }
        else
        {
            ClearItemInfo();
        }
    }
    
    public void SelectItem(InventoryItem item)
    {
        selectedItem = item;
        ShowItemInfo(item);
    }
    
    void ShowItemInfo(InventoryItem item)
    {
        if (item == null) return;
        
        if (itemNameText != null)
        {
            itemNameText.text = item.itemName;
        }
        
        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = item.itemDescription;
        }
        
        if (itemIconImage != null)
        {
            if (item.itemIcon != null)
            {
                itemIconImage.sprite = item.itemIcon;
                itemIconImage.color = Color.white;
            }
            else
            {
                itemIconImage.color = Color.clear;
            }
        }
        
        if (useButton != null)
        {
            useButton.interactable = item.isUsable;
        }
        
        if (dropButton != null)
        {
            dropButton.interactable = true;
        }
    }
    
    void ClearItemInfo()
    {
        if (itemNameText != null)
        {
            itemNameText.text = "";
        }
        
        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = "";
        }
        
        if (itemIconImage != null)
        {
            itemIconImage.sprite = null;
            itemIconImage.color = Color.clear;
        }
        
        if (useButton != null)
        {
            useButton.interactable = false;
        }
        
        if (dropButton != null)
        {
            dropButton.interactable = false;
        }
    }
    
    void UseSelectedItem()
    {
        if (selectedItem != null && inventorySystem != null)
        {
            bool used = inventorySystem.UseItem(selectedItem.itemName);
            if (used)
            {
                selectedItem = null;
                ClearItemInfo();
                UpdateInventoryUI();
            }
        }
    }
    
    void DropSelectedItem()
    {
        if (selectedItem != null && inventorySystem != null)
        {
            inventorySystem.RemoveItem(selectedItem.itemName, 1);
            selectedItem = null;
            ClearItemInfo();
            UpdateInventoryUI();
        }
    }
}

