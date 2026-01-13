using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private GameObject checkpointIndicator;
    [SerializeField] private Light checkpointLight;
    [SerializeField] private ParticleSystem checkpointParticles;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip checkpointSound;
    
    [Header("Settings")]
    [SerializeField] private bool autoSave = true;
    [SerializeField] private bool showNotification = true;
    
    private bool isActivated = false;
    private Color originalLightColor;
    private float originalLightIntensity;
    
    void Start()
    {
        if (checkpointLight != null)
        {
            originalLightColor = checkpointLight.color;
            originalLightIntensity = checkpointLight.intensity;
            checkpointLight.enabled = false;
        }
        
        if (checkpointIndicator != null)
        {
            checkpointIndicator.SetActive(false);
        }
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.maxDistance = 15f;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            ActivateCheckpoint();
        }
    }
    
    void ActivateCheckpoint()
    {
        isActivated = true;
        
        if (autoSave && SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveGame();
            Debug.Log("Checkpoint reached! Game saved.");
        }
        
        if (checkpointIndicator != null)
        {
            checkpointIndicator.SetActive(true);
        }
        
        if (checkpointLight != null)
        {
            checkpointLight.enabled = true;
            checkpointLight.color = Color.green;
            checkpointLight.intensity = originalLightIntensity * 1.5f;
        }
        
        if (checkpointParticles != null)
        {
            checkpointParticles.Play();
        }
        
        if (audioSource != null && checkpointSound != null)
        {
            audioSource.PlayOneShot(checkpointSound);
        }
        
        if (showNotification)
        {
            SaveLoadUI saveLoadUI = FindFirstObjectByType<SaveLoadUI>();
            if (saveLoadUI != null)
            {
                saveLoadUI.ShowNotification("Checkpoint reached!");
            }
        }
    }
    
    public bool IsActivated()
    {
        return isActivated;
    }
    
    public void ResetCheckpoint()
    {
        isActivated = false;
        
        if (checkpointIndicator != null)
        {
            checkpointIndicator.SetActive(false);
        }
        
        if (checkpointLight != null)
        {
            checkpointLight.enabled = false;
            checkpointLight.color = originalLightColor;
            checkpointLight.intensity = originalLightIntensity;
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = isActivated ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}

