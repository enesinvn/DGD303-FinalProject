using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    [Header("Sound Settings")]
    public float soundRadius = 10f;
    public float soundIntensity = 1f;
    public float soundDuration = 0.5f;
    public bool isContinuous = false;
    
    [Header("Sound Type")]
    public SoundType soundType = SoundType.Footstep;
    
    [Header("Layer")]
    [SerializeField] private LayerMask enemyLayer;
    
    [Header("Debug/Test")]
    public bool showDebugVisuals = false;
    [SerializeField] private GameObject debugSpherePrefab;
    [SerializeField] private LineRenderer debugLineRenderer;
    
    private float soundTimer = 0f;
    private bool isActive = false;
    private GameObject debugSphere;
    
    public enum SoundType
    {
        Footstep,
        ItemPickup,
        DoorOpen,
        DoorClose,
        Throw,
        Interaction,
        Custom
    }
    
    void Start()
    {
        if (debugSphere != null)
        {
            debugSphere.SetActive(false);
            Destroy(debugSphere);
            debugSphere = null;
        }
        
        showDebugVisuals = false;
    }
    
    void Update()
    {
        if (isActive && !isContinuous)
        {
            soundTimer -= Time.deltaTime;
            if (soundTimer <= 0f)
            {
                isActive = false;
                
                if (debugSphere != null)
                {
                    debugSphere.SetActive(false);
                }
                
                if (debugLineRenderer != null)
                {
                    debugLineRenderer.enabled = false;
                }
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
        
        Collider[] enemies = Physics.OverlapSphere(transform.position, soundRadius * intensity, enemyLayer);
        
        int detectedEnemies = 0;
        foreach (Collider enemy in enemies)
        {
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                float normalizedDistance = distance / soundRadius;
                
                float soundStrength = (1f - normalizedDistance) * intensity;
                
                enemyAI.OnSoundDetected(transform.position, soundStrength, soundType);
                detectedEnemies++;
            }
        }
        
        Debug.Log($"[SoundEmitter] Sound emitted: {soundType} - Intensity: {intensity:F2} - Detected NPCs: {detectedEnemies} - Radius: {soundRadius * intensity:F1}m");
    }
    
    void ShowSoundVisualization(float intensity)
    {
        return;
    }
    
    void DrawSoundLine(Vector3 targetPosition, float strength)
    {
        if (debugLineRenderer == null)
        {
            GameObject lineObj = new GameObject("SoundLine");
            lineObj.transform.SetParent(transform);
            debugLineRenderer = lineObj.AddComponent<LineRenderer>();
            debugLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            debugLineRenderer.startColor = Color.yellow;
            debugLineRenderer.endColor = Color.yellow;
            debugLineRenderer.startWidth = 0.1f;
            debugLineRenderer.endWidth = 0.05f;
            debugLineRenderer.positionCount = 2;
        }
        
        if (debugLineRenderer != null)
        {
            debugLineRenderer.enabled = true;
            debugLineRenderer.SetPosition(0, transform.position);
            debugLineRenderer.SetPosition(1, targetPosition);
            Color lineColor = Color.Lerp(Color.green, Color.red, 1f - strength);
            debugLineRenderer.startColor = lineColor;
            debugLineRenderer.endColor = lineColor;
        }
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
    
    public void SetEnemyLayer(LayerMask layer)
    {
        enemyLayer = layer;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isActive ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, soundRadius);
        
        if (isActive)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Gizmos.DrawSphere(transform.position, soundRadius);
        }
    }
    
    void OnDestroy()
    {
        if (debugSphere != null)
        {
            Destroy(debugSphere);
        }
        
        if (debugLineRenderer != null)
        {
            Destroy(debugLineRenderer.gameObject);
        }
    }
}

