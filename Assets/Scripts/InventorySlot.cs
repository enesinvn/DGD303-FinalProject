using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image backgroundImage;
    
    [Header("Colors")]
    [SerializeField] private Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.85f);
    [SerializeField] private Color filledColor = new Color(0.3f, 0.3f, 0.3f, 0.95f);
    [SerializeField] private Color selectedColor = new Color(0.5f, 0.5f, 0.8f, 1f);
    
    private InventoryItem item;
    private InventoryUI inventoryUI;
    private bool isSelected = false;
    
    void Awake()
    {
        if (iconImage == null)
        {
            iconImage = GetComponentInChildren<Image>();
        }
        
        if (quantityText == null)
        {
            quantityText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }
    }
    
    public void SetItem(InventoryItem newItem, InventoryUI ui)
    {
        item = newItem;
        inventoryUI = ui;
        
        if (iconImage != null)
        {
            if (item.itemIcon != null)
            {
                iconImage.sprite = item.itemIcon;
                iconImage.color = Color.white;
            }
            else
            {
                iconImage.color = Color.clear;
            }
        }
        
        if (quantityText != null)
        {
            if (item.quantity > 1)
            {
                quantityText.text = item.quantity.ToString();
                quantityText.gameObject.SetActive(true);
            }
            else
            {
                quantityText.gameObject.SetActive(false);
            }
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = filledColor;
        }
    }
    
    public void SetEmpty()
    {
        item = null;
        
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.color = Color.clear;
        }
        
        if (quantityText != null)
        {
            quantityText.text = "";
            quantityText.gameObject.SetActive(false);
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = emptyColor;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (item != null && inventoryUI != null)
        {
            inventoryUI.SelectItem(item);
            SetSelected(true);
        }
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (backgroundImage != null)
        {
            backgroundImage.color = selected ? selectedColor : (item != null ? filledColor : emptyColor);
        }
    }
    
    public InventoryItem GetItem()
    {
        return item;
    }
}

