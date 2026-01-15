using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [Header("Flashlight Settings")]
    [SerializeField] private Light flashlight;
    [SerializeField] private float maxBatteryLife = 100f;
    [SerializeField] private float batteryDrainRate = 5f; 
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 3f;
    
    [Header("Flicker Effect")]
    [SerializeField] private bool enableFlicker = true;
    [SerializeField] private float flickerThreshold = 20f; 
    [SerializeField] private float flickerSpeed = 0.1f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip flashlightOnSound;
    [SerializeField] private AudioClip flashlightOffSound;
    [SerializeField] private AudioClip lowBatterySound;
    
    private float currentBatteryLife;
    private bool isOn;
    private float nextFlickerTime;
    private bool hasPlayedLowBatteryWarning;
    
    void Start()
    {
        currentBatteryLife = maxBatteryLife;
        
        if (flashlight != null)
        {
            flashlight.enabled = false;
            isOn = false;
        }
    }
    
    void Update()
    {
        HandleInput();
        
        if (isOn && flashlight != null)
        {
            DrainBattery();
            HandleFlicker();
        }
    }
    
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlashlight();
        }
    }
    
    void ToggleFlashlight()
    {
        if (currentBatteryLife <= 0)
        {
            
            if (audioSource != null && lowBatterySound != null)
            {
                audioSource.PlayOneShot(lowBatterySound);
            }
            return;
        }
        
        isOn = !isOn;
        
        if (flashlight != null)
        {
            flashlight.enabled = isOn;
        }
        
        // Açma/kapama sesini çal
        if (audioSource != null)
        {
            if (isOn && flashlightOnSound != null)
            {
                audioSource.PlayOneShot(flashlightOnSound);
            }
            else if (!isOn && flashlightOffSound != null)
            {
                audioSource.PlayOneShot(flashlightOffSound);
            }
        }
    }
    
    void DrainBattery()
    {
        currentBatteryLife -= batteryDrainRate * Time.deltaTime;
        currentBatteryLife = Mathf.Max(0, currentBatteryLife);
        
        float batteryPercentage = currentBatteryLife / maxBatteryLife;
        flashlight.intensity = Mathf.Lerp(minIntensity, maxIntensity, batteryPercentage);
        
        
        if (currentBatteryLife < flickerThreshold && !hasPlayedLowBatteryWarning)
        {
            if (audioSource != null && lowBatterySound != null)
            {
                audioSource.PlayOneShot(lowBatterySound);
            }
            hasPlayedLowBatteryWarning = true;
        }
        
        if (currentBatteryLife <= 0)
        {
            flashlight.enabled = false;
            isOn = false;
        }
    }
    
    void HandleFlicker()
    {
        if (!enableFlicker || currentBatteryLife > flickerThreshold) return;
        
        if (Time.time >= nextFlickerTime)
        {
            flashlight.enabled = !flashlight.enabled;
            nextFlickerTime = Time.time + Random.Range(flickerSpeed * 0.5f, flickerSpeed * 2f);
        }
    }
    
    public void RechargeBattery(float amount)
    {
        currentBatteryLife = Mathf.Min(maxBatteryLife, currentBatteryLife + amount);
        hasPlayedLowBatteryWarning = false;
    }
    
    public float GetBatteryPercentage()
    {
        return (currentBatteryLife / maxBatteryLife) * 100f;
    }
    
    public bool IsOn()
    {
        return isOn;
    }
}