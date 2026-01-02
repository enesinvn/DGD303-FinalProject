using UnityEngine;

public class HidingSpot : MonoBehaviour, IInteractable
{
    [Header("Saklanma Ayarları")]
    [SerializeField] private HidingType hidingType = HidingType.Closet;
    [SerializeField] private Transform hidePosition;
    [SerializeField] private float hideDuration = 0.5f; // Saklanma animasyon süresi
    
    [Header("Nefes Ayarları")]
    [SerializeField] private bool requiresBreathHolding = true;
    [SerializeField] private float breathDrainRate = 10f; // Nefes tutarken stamina azalma hızı
    
    [Header("Görsel Efektler")]
    [SerializeField] private GameObject hideIndicator;
    [SerializeField] private Light hideLight;
    
    [Header("Ses")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip enterSound;
    [SerializeField] private AudioClip exitSound;
    
    [Header("NPC Algılama")]
    [SerializeField] private float detectionRadius = 2f; // NPC yakınsa tespit edilebilir
    [SerializeField] private LayerMask enemyLayer;
    
    private bool isOccupied = false;
    private GameObject hiddenPlayer;
    private Vector3 originalPlayerPosition;
    private Quaternion originalPlayerRotation;
    
    public enum HidingType
    {
        Closet,      // Dolap
        UnderTable,  // Masa altı
        Corner,      // Köşe
        Vent,        // Hava bacası
        Locker       // Dolap
    }
    
    void Start()
    {
        if (hidePosition == null)
        {
            hidePosition = transform;
        }
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.maxDistance = 10f;
        }
        
        if (hideIndicator != null)
        {
            hideIndicator.SetActive(false);
        }
    }
    
    void Update()
    {
        if (isOccupied && hiddenPlayer != null)
        {
            CheckEnemyProximity();
        }
    }
    
    public void Interact()
    {
        if (isOccupied)
        {
            ExitHiding();
        }
        else
        {
            EnterHiding();
        }
    }
    
    void EnterHiding()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        PlayerHiding playerHiding = player.GetComponent<PlayerHiding>();
        if (playerHiding == null) return;
        
        if (playerHiding.CanHide())
        {
            isOccupied = true;
            hiddenPlayer = player;
            
            // Oyuncu pozisyonunu kaydet
            originalPlayerPosition = player.transform.position;
            originalPlayerRotation = player.transform.rotation;
            
            // Oyuncuyu saklanma pozisyonuna taşı
            StartCoroutine(MoveToHidePosition(player));
            
            // PlayerHiding'e bildir
            playerHiding.StartHiding(this, requiresBreathHolding, breathDrainRate);
            
            // Görsel feedback
            if (hideIndicator != null)
            {
                hideIndicator.SetActive(true);
            }
            
            if (hideLight != null)
            {
                hideLight.enabled = true;
            }
            
            // Ses efekti
            if (audioSource != null && enterSound != null)
            {
                audioSource.PlayOneShot(enterSound);
            }
            
            Debug.Log($"{hidingType} içine saklandınız! (E tuşu ile çıkın)");
        }
    }
    
    void ExitHiding()
    {
        if (hiddenPlayer == null) return;
        
        PlayerHiding playerHiding = hiddenPlayer.GetComponent<PlayerHiding>();
        if (playerHiding != null)
        {
            playerHiding.StopHiding();
        }
        
        // Oyuncuyu geri taşı
        if (hiddenPlayer != null)
        {
            hiddenPlayer.transform.position = originalPlayerPosition;
            hiddenPlayer.transform.rotation = originalPlayerRotation;
        }
        
        // Ses efekti
        if (audioSource != null && exitSound != null)
        {
            audioSource.PlayOneShot(exitSound);
        }
        
        isOccupied = false;
        hiddenPlayer = null;
        
        // Görsel feedback
        if (hideIndicator != null)
        {
            hideIndicator.SetActive(false);
        }
        
        if (hideLight != null)
        {
            hideLight.enabled = false;
        }
        
        Debug.Log("Saklanma yerinden çıktınız!");
    }
    
    System.Collections.IEnumerator MoveToHidePosition(GameObject player)
    {
        Vector3 startPos = player.transform.position;
        Vector3 targetPos = hidePosition.position;
        Quaternion startRot = player.transform.rotation;
        Quaternion targetRot = hidePosition.rotation;
        
        float elapsed = 0f;
        
        while (elapsed < hideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / hideDuration;
            
            player.transform.position = Vector3.Lerp(startPos, targetPos, t);
            player.transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
            
            yield return null;
        }
        
        player.transform.position = targetPos;
        player.transform.rotation = targetRot;
    }
    
    void CheckEnemyProximity()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        
        if (enemies.Length > 0)
        {
            // NPC çok yakınsa tespit edilebilir
            foreach (Collider enemy in enemies)
            {
                EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    float distance = Vector3.Distance(enemy.transform.position, transform.position);
                    if (distance < detectionRadius * 0.5f) // Çok yakınsa
                    {
                        // %50 şansla tespit edilir
                        if (Random.value < 0.5f)
                        {
                            ExitHiding();
                            Debug.LogWarning("NPC sizi buldu!");
                        }
                    }
                }
            }
        }
    }
    
    public bool IsOccupied()
    {
        return isOccupied;
    }
    
    public HidingType GetHidingType()
    {
        return hidingType;
    }
    
    void OnDrawGizmosSelected()
    {
        // Detection radius göster
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Hide position göster
        if (hidePosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(hidePosition.position, 0.3f);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
}

