using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("AI Ayarları")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    
    [Header("Patrol Ayarları")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waitTimeAtPoint = 2f;
    
    [Header("Görme Ayarları")]
    [SerializeField] private float fieldOfView = 90f;
    [SerializeField] private LayerMask obstacleLayer;
    
    [Header("Ses Ayarları")]
    [SerializeField] private float hearingRange = 10f;
    
    [Header("Referanslar")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private HealthSystem playerHealth;
    
    [Header("Sesler")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] idleSounds;
    [SerializeField] private AudioClip[] chaseSounds;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private float soundInterval = 5f;
    
    private NavMeshAgent agent;
    private EnemyState currentState = EnemyState.Patrol;
    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;
    private float attackTimer = 0f;
    private float lastSoundTime = 0f;
    private bool hasSeenPlayer = false;
    
    private enum EnemyState
    {
        Patrol,
        Chase,
        Attack,
        Search
    }
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }
        
        agent.speed = patrolSpeed;
        
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerHealth = player.GetComponent<HealthSystem>();
            }
        }
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.maxDistance = 20f;
        }
        
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[0].position);
        }
    }
    
    void Update()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Durum güncellemesi
        UpdateState(distanceToPlayer);
        
        // Duruma göre davranış
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                Chase();
                break;
            case EnemyState.Attack:
                Attack();
                break;
            case EnemyState.Search:
                Search();
                break;
        }
        
        attackTimer -= Time.deltaTime;
    }
    
    void UpdateState(float distanceToPlayer)
    {
        bool canSeePlayer = CanSeePlayer();
        bool canHearPlayer = CanHearPlayer(distanceToPlayer);
        
        if (distanceToPlayer <= attackRange && canSeePlayer)
        {
            currentState = EnemyState.Attack;
        }
        else if ((canSeePlayer || canHearPlayer) && distanceToPlayer <= detectionRange)
        {
            currentState = EnemyState.Chase;
            hasSeenPlayer = true;
        }
        else if (hasSeenPlayer && distanceToPlayer > detectionRange)
        {
            currentState = EnemyState.Search;
        }
        else
        {
            currentState = EnemyState.Patrol;
            hasSeenPlayer = false;
        }
    }
    
    bool CanSeePlayer()
    {
        if (playerTransform == null) return false;
        
        // Oyuncu saklanıyorsa görülemez
        PlayerHiding playerHiding = playerTransform.GetComponent<PlayerHiding>();
        if (playerHiding != null && playerHiding.IsHiding())
        {
            // Saklanma yerini kontrol et
            HidingSpot hidingSpot = playerHiding.GetCurrentHidingSpot();
            if (hidingSpot != null)
            {
                // NPC çok yakınsa ve saklanma yeri tespit edilebilirse görülebilir
                float distanceToHidingSpot = Vector3.Distance(transform.position, hidingSpot.transform.position);
                if (distanceToHidingSpot > 1.5f) // Çok yakın değilse görülemez
                {
                    return false;
                }
            }
            else
            {
                return false; // Saklanıyor ama hiding spot yok (güvenlik)
            }
        }
        
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        
        if (angleToPlayer > fieldOfView / 2f) return false;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        RaycastHit hit;
        
        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, distanceToPlayer, obstacleLayer))
        {
            return hit.collider.transform == playerTransform;
        }
        
        return true;
    }
    
    bool CanHearPlayer(float distance)
    {
        // Oyuncu koşuyorsa veya ses çıkarıyorsa duyabilir
        if (distance <= hearingRange)
        {
            // Basit bir kontrol - gerçekte oyuncunun hareket hızına bakılabilir
            return true;
        }
        return false;
    }
    
    public void OnSoundDetected(Vector3 soundPosition, float soundStrength, SoundEmitter.SoundType soundType)
    {
        // Ses algılandı
        if (soundStrength > 0.3f) // Minimum eşik değeri
        {
            float distanceToSound = Vector3.Distance(transform.position, soundPosition);
            
            // Ses yeterince güçlüyse ve yakınsa
            if (distanceToSound <= hearingRange * soundStrength)
            {
                // Eğer oyuncuyu görmüyorsa ses kaynağına git
                if (currentState != EnemyState.Chase && currentState != EnemyState.Attack)
                {
                    // Ses kaynağına git (Search moduna geç)
                    if (currentState == EnemyState.Patrol)
                    {
                        currentState = EnemyState.Search;
                        agent.SetDestination(soundPosition);
                        hasSeenPlayer = true;
                        
                        Debug.Log($"NPC ses algıladı: {soundType} - Şiddet: {soundStrength}");
                    }
                }
                else if (currentState == EnemyState.Search)
                {
                    // Yeni ses kaynağına git
                    agent.SetDestination(soundPosition);
                }
            }
        }
    }
    
    void Patrol()
    {
        agent.speed = patrolSpeed;
        
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        
        if (agent.remainingDistance < 0.5f)
        {
            waitTimer += Time.deltaTime;
            
            if (waitTimer >= waitTimeAtPoint)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                waitTimer = 0f;
            }
        }
        
        PlayIdleSound();
    }
    
    void Chase()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(playerTransform.position);
        
        PlayChaseSound();
    }
    
    void Attack()
    {
        agent.SetDestination(transform.position); // Dur
        
        if (attackTimer <= 0f)
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
            
            if (audioSource != null && attackSound != null)
            {
                audioSource.PlayOneShot(attackSound);
            }
            
            attackTimer = attackCooldown;
        }
    }
    
    void Search()
    {
        agent.speed = patrolSpeed;
        
        // Son görülen konuma git
        if (playerTransform != null)
        {
            agent.SetDestination(playerTransform.position);
        }
        
        // Belirli bir süre sonra patrol'a dön
        if (agent.remainingDistance < 1f)
        {
            currentState = EnemyState.Patrol;
        }
    }
    
    void PlayIdleSound()
    {
        if (Time.time - lastSoundTime >= soundInterval && idleSounds != null && idleSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, idleSounds.Length);
            if (audioSource != null && idleSounds[randomIndex] != null)
            {
                audioSource.PlayOneShot(idleSounds[randomIndex]);
            }
            lastSoundTime = Time.time;
        }
    }
    
    void PlayChaseSound()
    {
        if (Time.time - lastSoundTime >= soundInterval * 0.5f && chaseSounds != null && chaseSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, chaseSounds.Length);
            if (audioSource != null && chaseSounds[randomIndex] != null)
            {
                audioSource.PlayOneShot(chaseSounds[randomIndex]);
            }
            lastSoundTime = Time.time;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Hearing range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
        
        // Field of view
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView / 2, 0) * transform.forward;
        
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, leftBoundary * detectionRange);
        Gizmos.DrawRay(transform.position, rightBoundary * detectionRange);
    }
}

