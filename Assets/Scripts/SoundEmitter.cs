using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    [Header("Ses Ayarları")]
    public float soundRadius = 10f;
    public float soundIntensity = 1f; // 0-1 arası
    public float soundDuration = 0.5f; // Ses ne kadar süre duyulur
    public bool isContinuous = false; // Sürekli ses (adım sesleri için)
    
    [Header("Ses Tipi")]
    public SoundType soundType = SoundType.Footstep;
    
    [Header("Layer")]
    [SerializeField] private LayerMask enemyLayer;
    
    private float soundTimer = 0f;
    private bool isActive = false;
    
    public enum SoundType
    {
        Footstep,      // Adım sesi
        ItemPickup,    // Eşya alma
        DoorOpen,      // Kapı açma
        DoorClose,     // Kapı kapatma
        Throw,         // Fırlatma
        Interaction,   // Etkileşim
        Custom         // Özel ses
    }
    
    void Update()
    {
        if (isActive && !isContinuous)
        {
            soundTimer -= Time.deltaTime;
            if (soundTimer <= 0f)
            {
                isActive = false;
            }
        }
    }
    
    public void EmitSound(float intensity = -1f)
    {
        if (intensity < 0f)
        {
            intensity = soundIntensity;
        }
        
        isActive = true;
        soundTimer = soundDuration;
        
        // NPC'lere ses sinyali gönder
        Collider[] enemies = Physics.OverlapSphere(transform.position, soundRadius * intensity, enemyLayer);
        
        foreach (Collider enemy in enemies)
        {
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                float normalizedDistance = distance / soundRadius;
                
                // Mesafeye göre ses şiddeti
                float soundStrength = (1f - normalizedDistance) * intensity;
                
                enemyAI.OnSoundDetected(transform.position, soundStrength, soundType);
            }
        }
        
        Debug.Log($"Ses yayıldı: {soundType} - Şiddet: {intensity}");
    }
    
    public void StartContinuousSound()
    {
        isContinuous = true;
        isActive = true;
    }
    
    public void StopContinuousSound()
    {
        isContinuous = false;
        isActive = false;
    }
    
    public bool IsActive()
    {
        return isActive;
    }
    
    public float GetSoundRadius()
    {
        return soundRadius;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isActive ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, soundRadius);
    }
}

