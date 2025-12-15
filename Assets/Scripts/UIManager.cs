using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Batarya UI")]
    [SerializeField] private RectTransform batteryFillTransform;
    [SerializeField] private TextMeshProUGUI batteryText;
    
    [Header("Stamina UI")]
    [SerializeField] private RectTransform staminaFillTransform;
    
    [Header("Referanslar")]
    [SerializeField] private Flashlight flashlight;
    [SerializeField] private StaminaSystem staminaSystem;
    
    private Image batteryFillImage;
    private Image staminaFillImage;
    
    void Start()
    {
        if (batteryFillTransform != null)
        {
            batteryFillImage = batteryFillTransform.GetComponent<Image>();
        }
        
        if (staminaFillTransform != null)
        {
            staminaFillImage = staminaFillTransform.GetComponent<Image>();
        }
    }
    
    void Update()
    {
        UpdateBatteryUI();
        UpdateStaminaUI();
    }
    
    void UpdateBatteryUI()
    {
        if (flashlight == null || batteryFillTransform == null) return;
        
        float batteryPercent = flashlight.GetBatteryPercentage();
        
        Vector3 scale = batteryFillTransform.localScale;
        scale.x = batteryPercent / 100f;
        batteryFillTransform.localScale = scale;
        
        if (batteryText != null)
        {
            batteryText.text = $"{Mathf.RoundToInt(batteryPercent)}%";
        }
        
        if (batteryFillImage != null)
        {
            if (batteryPercent < 20f)
            {
                batteryFillImage.color = Color.red;
            }
            else if (batteryPercent < 50f)
            {
                batteryFillImage.color = Color.yellow;
            }
            else
            {
                batteryFillImage.color = new Color(0.3f, 1f, 0.3f);
            }
        }
    }
    
    void UpdateStaminaUI()
    {
        if (staminaSystem == null || staminaFillTransform == null) return;
        
        float staminaPercent = staminaSystem.GetStaminaPercentage();
        
        Vector3 scale = staminaFillTransform.localScale;
        scale.x = staminaPercent / 100f;
        staminaFillTransform.localScale = scale;
        
        if (staminaFillImage != null)
        {
            if (staminaPercent < 20f)
            {
                staminaFillImage.color = Color.red;
            }
            else if (staminaPercent < 50f)
            {
                staminaFillImage.color = Color.yellow;
            }
            else
            {
                staminaFillImage.color = new Color(0f, 0.9f, 1f);
            }
        }
    }
}
