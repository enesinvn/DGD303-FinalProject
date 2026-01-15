using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour
{
    public enum PatrolMode
    {
        FixedPoints,        // Follow designated patrol points
        RandomWaypoints,    // Go to random NavMesh points
        Mixed               // Mix both (use patrol points if available, otherwise random)
    }
    
    public enum EnemyState
    {
        Patrol,
        Chase,
        Attack,
        Search,
        Investigate
    }
    
    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 100f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 5f;
    
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waitTimeAtPoint = 2f;
    [SerializeField] private float patrolPointReachedDistance = 0.5f;
    [SerializeField] private PatrolMode patrolMode = PatrolMode.Mixed;
    [SerializeField] private float randomPatrolRadius = 20f;
    [SerializeField] private float minRandomWaitTime = 1f;
    [SerializeField] private float maxRandomWaitTime = 3f;
    
    [Header("Vision Settings")]
    [SerializeField] private float fieldOfView = 110f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float visionUpdateInterval = 0.1f;
    
    [Header("Audio Settings")]
    [SerializeField] private float hearingRange = 12f;
    [SerializeField] private float soundMemoryDuration = 5f;
    
    [Header("Search Settings")]
    [SerializeField] private float searchDuration = 10f;
    [SerializeField] private float searchSpeed = 3f;
    [SerializeField] private float searchRadius = 5f;
    
    [Header("Game Over")]
    [SerializeField] private bool restartOnCatch = true;
    [SerializeField] private string gameOverSceneName = "";
    [SerializeField] private float gameOverDelay = 1f;
    [SerializeField] private bool useHealthSystem = false;
    [SerializeField] private bool useGameManager = true; // Use GameManager for game over
    
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private HealthSystem playerHealth;
    
    [Header("Sounds")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] idleSounds;
    [SerializeField] private AudioClip[] alertSounds;
    [SerializeField] private AudioClip[] chaseSounds;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip killSound;
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float soundInterval = 5f;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private bool useAnimator = true;
    [SerializeField] private float patrolAnimationSpeed = 1.0f;
    [SerializeField] private float chaseAnimationSpeed = 1.2f;
    [SerializeField] private float searchAnimationSpeed = 1.1f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private bool enableDebugLogs = true;
    
    private NavMeshAgent agent;
    private EnemyState currentState = EnemyState.Patrol;
    private EnemyState previousState;
    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;
    private float attackTimer = 0f;
    private float lastSoundTime = 0f;
    private float searchTimer = 0f;
    private float lastVisionCheckTime = 0f;
    private bool hasSeenPlayer = false;
    private Vector3 lastKnownPlayerPosition;
    private Vector3 lastSoundPosition;
    private float lastSoundTime_Memory = 0f;
    private bool isGameOver = false;
    private AudioManager audioManager;
    private Vector3 currentRandomDestination;
    private bool hasRandomDestination = false;
    private float currentRandomWaitTime = 0f;
    
    // Animation parameter hashes
    private int animIDSpeed;
    private int animIDGrounded;
    private int animIDJump;
    private int animIDFreeFall;
    private int animIDMotionSpeed;
    
    void Start()
    {
        InitializeAgent();
        FindPlayer();
        SetupAudio();
        SetupAnimator();
        
        audioManager = AudioManager.Instance;
        
        lastSoundTime_Memory = -soundMemoryDuration;
        searchTimer = 0f;
        hasSeenPlayer = false;
        
        // Start based on patrol mode
        if (patrolMode == PatrolMode.FixedPoints || patrolMode == PatrolMode.Mixed)
        {
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                agent.SetDestination(patrolPoints[0].position);
                Log($"Patrol started - First target: {patrolPoints[0].name}");
            }
            else if (patrolMode == PatrolMode.Mixed)
            {
                Log("No patrol points - Starting random waypoint patrol");
                hasRandomDestination = false;
                currentRandomWaitTime = 0f;
            }
            else
            {
                LogWarning("Patrol points not assigned! Enemy won't be able to move.");
            }
        }
        else if (patrolMode == PatrolMode.RandomWaypoints)
        {
            Log("Random waypoint patrol mode activated");
            hasRandomDestination = false;
            currentRandomWaitTime = 0f;
        }
    }
    
    void SetupAnimator()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (animator != null && useAnimator)
        {
            // Hash animation parameter names for performance
            animIDSpeed = Animator.StringToHash("Speed");
            animIDGrounded = Animator.StringToHash("Grounded");
            animIDJump = Animator.StringToHash("Jump");
            animIDFreeFall = Animator.StringToHash("FreeFall");
            animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            
            Log("Animator initialized");
        }
        else if (useAnimator)
        {
            LogWarning("Animator not found! Add Animator component to enemy.");
        }
    }
    
    void InitializeAgent()
    {
        agent = GetComponent<NavMeshAgent>();
        
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
            LogWarning("NavMeshAgent was missing, added automatically!");
        }
        
        agent.speed = patrolSpeed;
        agent.angularSpeed = 120f;
        agent.acceleration = 8f;
        agent.stoppingDistance = 0.5f;
        agent.autoBraking = true;
        agent.autoRepath = true;
        
        Log($"NavMeshAgent initialized - Speed: {agent.speed}, Stopping Distance: {agent.stoppingDistance}");
    }
    
    void FindPlayer()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                
                if (useHealthSystem)
                {
                    playerHealth = player.GetComponent<HealthSystem>();
                    if (playerHealth != null)
                    {
                        Log("Player found - HealthSystem active");
                    }
                    else
                    {
                        LogWarning("HealthSystem not found! Set useHealthSystem = false.");
                    }
                }
                else
                {
                    Log("Player found - Direct game over mode");
                }
            }
            else
            {
                LogError("Player not found! Add 'Player' tag.");
            }
        }
    }
    
    void SetupAudio()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.maxDistance = 25f;
            audioSource.minDistance = 5f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
        }
    }
    
    void Update()
    {
        // Check if intro/tutorial is active - freeze enemy during intro
        if (TutorialManager.IsIntroActiveStatic)
        {
            // Stop audio during intro
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
            
            // Stop agent movement
            if (agent != null && agent.enabled)
            {
                agent.isStopped = true;
            }
            
            return; // Don't process any AI logic during intro
        }
        else if (agent != null && agent.enabled && agent.isStopped)
        {
            // Resume movement when intro ends
            agent.isStopped = false;
        }
        
        // Pause or Game Over - stop audio
        if (Time.timeScale == 0f || isGameOver)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
            }

            if (isGameOver) return;
            return;
        }

        // Resume audio if it was paused
        if (audioSource != null && !audioSource.isPlaying && Time.timeScale > 0f && !isGameOver)
        {
            // Don't auto-resume, let the sound play naturally
        }

        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        UpdateState(distanceToPlayer);
        
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
            case EnemyState.Investigate:
                Investigate();
                break;
        }
        
        attackTimer -= Time.deltaTime;
        searchTimer -= Time.deltaTime;
        
        UpdateAnimator();
    }
    
    void UpdateAnimator()
    {
        if (animator == null || !useAnimator) return;
        
        // Calculate speed based on agent velocity
        float speed = 0f;
        bool isGrounded = true;
        
        if (agent != null && agent.enabled)
        {
            Vector3 velocity = agent.velocity;
            speed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
            
            // Normalize speed based on agent's current max speed (not always chaseSpeed)
            // This ensures Speed = 1 when moving at target speed
            float normalizedSpeed = (agent.speed > 0.01f) ? (speed / agent.speed) : 0f;
            
            // Calculate motion speed based on current state
            float motionSpeed = GetAnimationSpeedForState();
            
            // Set animator parameters
            animator.SetFloat(animIDSpeed, normalizedSpeed);
            animator.SetFloat(animIDMotionSpeed, motionSpeed);
            
            // Grounded check (NavMeshAgent is always grounded when moving)
            isGrounded = agent.isOnNavMesh && !agent.isStopped;
            animator.SetBool(animIDGrounded, isGrounded);
            
            // FreeFall (not grounded)
            animator.SetBool(animIDFreeFall, !isGrounded);
        }
        else
        {
            // Agent disabled - set idle
            animator.SetFloat(animIDSpeed, 0f);
            animator.SetBool(animIDGrounded, true);
            animator.SetBool(animIDFreeFall, false);
        }
    }
    
    float GetAnimationSpeedForState()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                return patrolAnimationSpeed;
            case EnemyState.Chase:
                return chaseAnimationSpeed;
            case EnemyState.Search:
            case EnemyState.Investigate:
                return searchAnimationSpeed;
            case EnemyState.Attack:
                return chaseAnimationSpeed; // Use chase speed during Attack
            default:
                return 1f;
        }
    }
    
    void UpdateState(float distanceToPlayer)
    {
        PlayerHiding playerHiding = playerTransform?.GetComponent<PlayerHiding>();
        if (playerHiding != null && playerHiding.IsHiding())
        {
            HandleHiddenPlayer(playerHiding, distanceToPlayer);
            return;
        }
        
        bool canSeePlayer = false;
        if (Time.time - lastVisionCheckTime >= visionUpdateInterval)
        {
            canSeePlayer = CanSeePlayer();
            lastVisionCheckTime = Time.time;
        }
        
        bool canHearPlayer = CanHearPlayer(distanceToPlayer);
        
        if (distanceToPlayer <= attackRange && canSeePlayer)
        {
            ChangeState(EnemyState.Attack);
            lastKnownPlayerPosition = playerTransform.position;
            return;
        }
        
        if ((canSeePlayer || canHearPlayer) && distanceToPlayer <= detectionRange)
        {
            ChangeState(EnemyState.Chase);
            hasSeenPlayer = true;
            lastKnownPlayerPosition = playerTransform.position;
            return;
        }
        
        if (hasSeenPlayer && distanceToPlayer > detectionRange)
        {
            if (currentState != EnemyState.Search)
            {
                ChangeState(EnemyState.Search);
                searchTimer = searchDuration;
            }
            return;
        }
        
        if (Time.time - lastSoundTime_Memory < soundMemoryDuration && 
            currentState != EnemyState.Chase && 
            currentState != EnemyState.Attack &&
            hasSeenPlayer == false)
        {
            if (currentState != EnemyState.Investigate)
            {
                ChangeState(EnemyState.Investigate);
            }
            return;
        }
        
        if (currentState == EnemyState.Search && searchTimer <= 0f)
        {
            ChangeState(EnemyState.Patrol);
            hasSeenPlayer = false;
        }
        
        if (currentState != EnemyState.Patrol && currentState != EnemyState.Search && 
            currentState != EnemyState.Investigate && !hasSeenPlayer)
        {
            ChangeState(EnemyState.Patrol);
        }
    }
    
    void HandleHiddenPlayer(PlayerHiding playerHiding, float distanceToPlayer)
    {
        HidingSpot hidingSpot = playerHiding.GetCurrentHidingSpot();
        if (hidingSpot == null)
        {
            if (currentState != EnemyState.Patrol)
            {
                ChangeState(EnemyState.Patrol);
                hasSeenPlayer = false;
            }
            return;
        }
        
        float distanceToHidingSpot = Vector3.Distance(transform.position, hidingSpot.transform.position);
        HidingSpot.HidingType hidingType = hidingSpot.GetHidingType();
        
        if (playerHiding.IsHoldingBreath())
        {
            if (currentState != EnemyState.Patrol && currentState != EnemyState.Search)
            {
                ChangeState(EnemyState.Search);
                searchTimer = searchDuration * 0.5f;
                Log("Player holding breath - short search initiated");
            }
            else if (currentState == EnemyState.Search && distanceToHidingSpot < 3f)
            {
                Vector3 randomPoint = transform.position + Random.insideUnitSphere * searchRadius;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, searchRadius, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                    Log("Near hiding spot - moving to different direction");
                }
            }
            return;
        }
        
        bool foundPlayer = false;
        
        if (distanceToHidingSpot < 1.5f)
        {
            switch (hidingType)
            {
                case HidingSpot.HidingType.Closet:
                case HidingSpot.HidingType.Locker:
                case HidingSpot.HidingType.Vent:
                    if (distanceToHidingSpot < 0.8f && Random.value < 0.1f)
                    {
                        foundPlayer = true;
                        Log($"Player found in {hidingType}! (10% chance, distance: {distanceToHidingSpot:F2}m)");
                    }
                    break;
                    
                case HidingSpot.HidingType.UnderTable:
                    if (distanceToHidingSpot < 1.2f && Random.value < 0.15f)
                    {
                        foundPlayer = true;
                        Log($"Player found under table! (15% chance, distance: {distanceToHidingSpot:F2}m)");
                    }
                    break;
                    
                case HidingSpot.HidingType.Corner:
                    Vector3 directionToHiding = (hidingSpot.transform.position - transform.position).normalized;
                    float angleToHiding = Vector3.Angle(transform.forward, directionToHiding);
                    
                    if (angleToHiding < fieldOfView / 2f && distanceToHidingSpot < 1.5f && Random.value < 0.25f)
                    {
                        foundPlayer = true;
                        Log($"Player found in corner! (25% chance, distance: {distanceToHidingSpot:F2}m)");
                    }
                    break;
            }
        }
        
        if (foundPlayer)
        {
            ChangeState(EnemyState.Attack);
            lastKnownPlayerPosition = hidingSpot.transform.position;
            hasSeenPlayer = true;
        }
        else
        {
            if (distanceToHidingSpot > 5f)
            {
                // Uzakta - normal patrol
                if (currentState != EnemyState.Patrol)
                {
                    ChangeState(EnemyState.Patrol);
                    hasSeenPlayer = false;
                    Log("Player hid, far away - Returned to Patrol");
                }
            }
            else if (distanceToHidingSpot > 2f)
            {
                if (currentState == EnemyState.Chase || currentState == EnemyState.Attack)
                {
                    ChangeState(EnemyState.Search);
                    searchTimer = 3f;
                    Log("Player hid, medium distance - Short search");
                }
            }
            else
            {
                if (currentState == EnemyState.Chase || currentState == EnemyState.Attack)
                {
                    ChangeState(EnemyState.Search);
                    searchTimer = 5f;
                    Log("Player hid, nearby - Searching around");
                }
            }
        }
    }
    
    bool CanSeePlayer()
    {
        if (playerTransform == null) return false;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer > detectionRange) 
        {
            return false;
        }
        
        PlayerHiding playerHiding = playerTransform.GetComponent<PlayerHiding>();
        if (playerHiding != null && playerHiding.IsHiding())
        {
            return false;
        }
        
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        
        if (angleToPlayer > fieldOfView / 2f) return false;
        
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 1.5f;
        Vector3 playerCenter = playerTransform.position + Vector3.up;
        
        if (Physics.Raycast(rayOrigin, (playerCenter - rayOrigin).normalized, out hit, distanceToPlayer, obstacleLayer))
        {
            if (hit.collider.transform != playerTransform && !hit.collider.CompareTag("Player"))
            {
                return false; // Engel var
            }
        }
        
        float detectionChance = 1f;
        
        if (distanceToPlayer > detectionRange * 0.8f)
        {
            detectionChance = 0.5f;
        }
        else if (distanceToPlayer > detectionRange * 0.6f)
        {
            detectionChance = 0.7f;
        }
        else if (distanceToPlayer > detectionRange * 0.4f)
        {
            detectionChance = 0.9f;
        }
        
        return Random.value < detectionChance;
    }
    
    bool CanHearPlayer(float distance)
    {
        if (playerTransform == null) return false;
        
        PlayerHiding playerHiding = playerTransform.GetComponent<PlayerHiding>();
        if (playerHiding != null && playerHiding.IsHiding())
        {
            if (playerHiding.IsHoldingBreath())
            {
                return false;
            }
            return distance <= hearingRange * 0.2f;
        }
        
        if (distance > hearingRange) return false;
        
        PlayerController playerController = playerTransform.GetComponent<PlayerController>();
        if (playerController != null)
        {
            CharacterController charController = playerTransform.GetComponent<CharacterController>();
            if (charController != null)
            {
                float speed = charController.velocity.magnitude;
                
                if (speed > playerController.WalkSpeed * 1.5f)
                {
                    return true;
                }
                else if (speed > 0.5f && distance < hearingRange * 0.6f)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    public void OnSoundDetected(Vector3 soundPosition, float soundStrength, SoundEmitter.SoundType soundType)
    {
        float distanceToSound = Vector3.Distance(transform.position, soundPosition);
        float effectiveRange = hearingRange * soundStrength;
        
        if (soundStrength > 0.2f && distanceToSound <= effectiveRange)
        {
            lastSoundPosition = soundPosition;
            lastSoundTime_Memory = Time.time;
            
            if (soundType == SoundEmitter.SoundType.Throw)
            {
                ChangeState(EnemyState.Investigate);
                agent.SetDestination(soundPosition);
                hasSeenPlayer = false;
                
                Log($"Distraction detected! Going to sound source (distance: {distanceToSound:F1}m)");
                return;
            }
            
            if (currentState != EnemyState.Chase && currentState != EnemyState.Attack)
            {
                ChangeState(EnemyState.Investigate);
                agent.SetDestination(soundPosition);
                
                Log($"Sound detected: {soundType} - Strength: {soundStrength:F2} - Going to investigate");
            }
            else if (soundStrength > 0.7f && distanceToSound < hearingRange * 0.5f)
            {
                lastKnownPlayerPosition = soundPosition;
                Log($"Heard strong sound during chase - Position updated");
            }
        }
    }
    
    void Patrol()
    {
        agent.speed = patrolSpeed;
        
        // Determine behavior based on patrol mode
        bool useFixedPoints = ShouldUseFixedPatrolPoints();
        
        if (useFixedPoints)
        {
            PatrolFixedPoints();
        }
        else
        {
            PatrolRandomWaypoints();
        }
        
        PlayIdleSound();
    }
    
    bool ShouldUseFixedPatrolPoints()
    {
        switch (patrolMode)
        {
            case PatrolMode.FixedPoints:
                return patrolPoints != null && patrolPoints.Length > 0;
                
            case PatrolMode.RandomWaypoints:
                return false;
                
            case PatrolMode.Mixed:
                return patrolPoints != null && patrolPoints.Length > 0;
                
            default:
                return patrolPoints != null && patrolPoints.Length > 0;
        }
    }
    
    void PatrolFixedPoints()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            LogWarning("No patrol points - Switching to random waypoints!");
            PatrolRandomWaypoints();
            return;
        }
        
        if (!agent.pathPending && agent.remainingDistance <= patrolPointReachedDistance)
        {
            waitTimer += Time.deltaTime;
            
            if (waitTimer >= waitTimeAtPoint)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                
                if (patrolPoints[currentPatrolIndex] != null)
                {
                    agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                    Log($"Moving to new patrol point: {currentPatrolIndex} / {patrolPoints.Length}");
                }
                else
                {
                    LogWarning($"Patrol point {currentPatrolIndex} is null!");
                }
                
                waitTimer = 0f;
            }
        }
        else if (agent.pathPending)
        {
            Log("NavMesh path calculating...");
        }
        else if (!agent.hasPath)
        {
            if (patrolPoints[currentPatrolIndex] != null)
            {
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                Log($"Path lost, recalculating: Point {currentPatrolIndex}");
            }
        }
    }
    
    void PatrolRandomWaypoints()
    {
        // If we reached the target or there is no target
        if (!hasRandomDestination || (!agent.pathPending && agent.remainingDistance <= patrolPointReachedDistance))
        {
            waitTimer += Time.deltaTime;
            
            // Set new target when random wait time is up
            if (waitTimer >= currentRandomWaitTime)
            {
                Vector3 randomDestination = GetRandomNavMeshPoint(transform.position, randomPatrolRadius);
                
                if (randomDestination != Vector3.zero)
                {
                    agent.SetDestination(randomDestination);
                    currentRandomDestination = randomDestination;
                    hasRandomDestination = true;
                    currentRandomWaitTime = Random.Range(minRandomWaitTime, maxRandomWaitTime);
                    waitTimer = 0f;
                    
                    Log($"Random waypoint set: {randomDestination} (Wait time: {currentRandomWaitTime:F1}s)");
                }
                else
                {
                    LogWarning("Failed to find random NavMesh point!");
                    // Try again
                    waitTimer = 0f;
                    currentRandomWaitTime = 0.5f;
                }
            }
        }
        else if (!agent.hasPath && hasRandomDestination)
        {
            // Path lost, recalculate
            agent.SetDestination(currentRandomDestination);
            Log("Path lost, recalculating random waypoint");
        }
    }
    
    Vector3 GetRandomNavMeshPoint(Vector3 center, float radius)
    {
        // Select a random point
        for (int i = 0; i < 30; i++) // Try 30 times
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * radius;
            randomPoint.y = center.y; // Keep Y axis
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, radius * 0.5f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        
        return Vector3.zero; // Bulunamadı
    }
    
    void Chase()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(playerTransform.position);
        lastKnownPlayerPosition = playerTransform.position;
        
        PlayChaseSound();
    }
    
    void Attack()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer > attackRange * 0.5f)
        {
            agent.SetDestination(playerTransform.position);
        }
        else
        {
            agent.SetDestination(transform.position); // Dur
        }
        
        Vector3 lookDirection = (playerTransform.position - transform.position).normalized;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 5f);
        }
        
        if (attackTimer <= 0f)
        {
            bool canAttack = true;
            if (useHealthSystem && playerHealth != null)
            {
                if (playerHealth.IsInvincible())
                {
                    canAttack = false;
                    Log("Player invincible - waiting for attack");
                }
            }
            
            if (canAttack)
            {
                PerformAttack();
                attackTimer = attackCooldown;
            }
        }
    }
    
    void PerformAttack()
    {
        // Trigger attack animation if animator exists
        if (animator != null && useAnimator)
        {
            // You can add an "Attack" trigger parameter if your animator has attack animations
            // animator.SetTrigger("Attack");
        }
        
        bool playerKilled = false;
        
        // ✅ Deal damage if HealthSystem exists
        if (useHealthSystem && playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
            Log($"Dealt {attackDamage} damage to player");
            
            if (playerHealth.IsDead())
            {
                playerKilled = true;
                Log("Player died (HealthSystem)");
            }
        }
        else
        {
            playerKilled = true;
            Log("Player caught (direct kill mode)");
        }
        
        // ✅ Ses efekti
        if (audioSource != null)
        {
            if (attackSound != null)
            {
                audioSource.PlayOneShot(attackSound);
            }
            
            if (playerKilled && killSound != null)
            {
                audioSource.PlayOneShot(killSound);
            }
        }
        
        if (playerKilled)
        {
            TriggerGameOver();
        }
    }
    
    void Search()
    {
        agent.speed = searchSpeed;
        
        if (playerTransform != null)
        {
            PlayerHiding playerHiding = playerTransform.GetComponent<PlayerHiding>();
            if (playerHiding != null && playerHiding.IsHiding())
            {
                HidingSpot hidingSpot = playerHiding.GetCurrentHidingSpot();
                if (hidingSpot != null)
                {
                    float distanceToSpot = Vector3.Distance(transform.position, hidingSpot.transform.position);
                    
                    if (distanceToSpot < 3f)
                    {
                        // Hiding spot'tan uzak rastgele bir nokta bul
                        Vector3 awayDirection = (transform.position - hidingSpot.transform.position).normalized;
                        Vector3 targetPoint = transform.position + awayDirection * searchRadius;
                        
                        NavMeshHit hit;
                        if (NavMesh.SamplePosition(targetPoint, out hit, searchRadius * 2f, NavMesh.AllAreas))
                        {
                            agent.SetDestination(hit.position);
                            Log($"Moving away from hiding spot (distance: {distanceToSpot:F1}m)");
                            return;
                        }
                    }
                }
            }
        }
        
        if (Vector3.Distance(transform.position, lastKnownPlayerPosition) > 2f)
        {
            // Son bilinen pozisyona git
            agent.SetDestination(lastKnownPlayerPosition);
        }
        else
        {
            // Son pozisyonda - rastgele etrafta ara
            if (!agent.hasPath || agent.remainingDistance < 1f)
            {
                Vector3 randomPoint = lastKnownPlayerPosition + Random.insideUnitSphere * searchRadius;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, searchRadius, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                    Log($"Random search point: {hit.position}");
                }
            }
        }
        
        if (searchTimer <= 0f)
        {
            ChangeState(EnemyState.Patrol);
            hasSeenPlayer = false;
            Log("Search time expired - Returned to Patrol");
        }
    }
    
    void Investigate()
    {
        agent.speed = searchSpeed;
        
        // Ses pozisyonuna git
        if (Vector3.Distance(transform.position, lastSoundPosition) > 1f)
        {
            agent.SetDestination(lastSoundPosition);
        }
        else
        {
            if (!agent.hasPath || agent.remainingDistance < 1f)
            {
                Vector3 randomPoint = lastSoundPosition + Random.insideUnitSphere * 3f;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 3f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
                else
                {
                    ChangeState(EnemyState.Patrol);
                }
            }
        }
        
        if (Time.time - lastSoundTime_Memory > soundMemoryDuration)
        {
            ChangeState(EnemyState.Patrol);
            Log("Sound memory cleared - Returned to Patrol");
        }
    }
    
    void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;
        
        previousState = currentState;
        currentState = newState;
        
        Log($"State changed: {previousState} → {currentState}");
        
        // Update animator based on state
        if (animator != null && useAnimator)
        {
            // You can add state-specific animation triggers here if needed
            // For example: animator.SetTrigger("ChaseTrigger");
        }
        
        // Update music based on state
        UpdateMusicForState(newState);
        
        switch (newState)
        {
            case EnemyState.Chase:
                // Play alert sound
                if (audioSource != null && alertSounds != null && alertSounds.Length > 0 && previousState == EnemyState.Patrol)
                {
                    audioSource.PlayOneShot(alertSounds[Random.Range(0, alertSounds.Length)]);
                }
                break;
                
            case EnemyState.Search:
                searchTimer = searchDuration;
                break;
                
            case EnemyState.Patrol:
                // Return to normal music when returning to patrol
                break;
        }
    }
    
    void UpdateMusicForState(EnemyState state)
    {
        if (audioManager == null) return;
        
        switch (state)
        {
            case EnemyState.Patrol:
            case EnemyState.Investigate:
                audioManager.PlayMusic(AudioManager.MusicState.Normal);
                break;
                
            case EnemyState.Search:
                audioManager.PlayMusic(AudioManager.MusicState.Tension);
                break;
                
            case EnemyState.Chase:
            case EnemyState.Attack:
                audioManager.PlayMusic(AudioManager.MusicState.Chase);
                break;
        }
    }
    
    void TriggerGameOver()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        agent.isStopped = true;
        
        Log("GAME OVER - Player caught!");
        
        // Try to use GameManager first
        if (useGameManager)
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.TriggerDefeat("You were caught by the enemy!");
                return;
            }
            else
            {
                LogWarning("GameManager not found! Using fallback restart method.");
            }
        }
        
        // Fallback to old system
        if (restartOnCatch)
        {
            Invoke(nameof(RestartGame), gameOverDelay);
        }
        else
        {
            Log("Restart disabled - Manual restart required");
        }
    }
    
    void RestartGame()
    {
        if (!string.IsNullOrEmpty(gameOverSceneName))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
    void PlayIdleSound()
    {
        if (Time.time - lastSoundTime >= soundInterval && idleSounds != null && idleSounds.Length > 0)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(idleSounds[Random.Range(0, idleSounds.Length)]);
            }
            lastSoundTime = Time.time;
        }
    }
    
    void PlayChaseSound()
    {
        if (Time.time - lastSoundTime >= soundInterval * 0.5f && chaseSounds != null && chaseSounds.Length > 0)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(chaseSounds[Random.Range(0, chaseSounds.Length)]);
            }
            lastSoundTime = Time.time;
        }
    }
    
    // Called by Animation Event (OnFootstep)
    void OnFootstep()
    {
        if (footstepSounds != null && footstepSounds.Length > 0)
        {
            if (audioSource != null)
            {
                AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
                audioSource.PlayOneShot(clip, 0.3f); // Lower volume for footsteps
            }
        }
    }
    
    void Log(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[EnemyAI - {gameObject.name}] {message}");
        }
    }
    
    void LogWarning(string message)
    {
        if (enableDebugLogs)
        {
            Debug.LogWarning($"[EnemyAI - {gameObject.name}] {message}");
        }
    }
    
    void LogError(string message)
    {
        Debug.LogError($"[EnemyAI - {gameObject.name}] {message}");
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Hearing range (Mavi)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
        
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView / 2, 0) * transform.forward;
        
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position + Vector3.up, leftBoundary * detectionRange);
        Gizmos.DrawRay(transform.position + Vector3.up, rightBoundary * detectionRange);
        
        // Son bilinen pozisyon (Turuncu)
        if (hasSeenPlayer)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(lastKnownPlayerPosition, 1f);
            Gizmos.DrawLine(transform.position + Vector3.up, lastKnownPlayerPosition + Vector3.up);
        }
        
        // Last heard sound (Cyan)
        if (Time.time - lastSoundTime_Memory < soundMemoryDuration)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(lastSoundPosition, 0.5f);
        }
        
        // Rastgele hedef (Random waypoint - Yeşil)
        if (hasRandomDestination && patrolMode == PatrolMode.RandomWaypoints)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(currentRandomDestination, 0.7f);
            Gizmos.DrawLine(transform.position + Vector3.up, currentRandomDestination + Vector3.up);
        }
        
        // Rastgele patrol yarıçapı (Açık yeşil)
        if (patrolMode == PatrolMode.RandomWaypoints || (patrolMode == PatrolMode.Mixed && (patrolPoints == null || patrolPoints.Length == 0)))
        {
            Gizmos.color = new Color(0.5f, 1f, 0.5f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, randomPatrolRadius);
        }
        
        Gizmos.color = GetStateColor();
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2.5f, 0.3f);
    }
    
    // ✅ Public getter (for Door to check)
    public string GetCurrentStateName()
    {
        return currentState.ToString();
    }
    
    public EnemyState GetCurrentState()
    {
        return currentState;
    }
    
    /// <summary>
    /// Set patrol points programmatically
    /// </summary>
    public void SetPatrolPoints(Transform[] points)
    {
        patrolPoints = points;
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            currentPatrolIndex = 0;
            if (agent != null && agent.isActiveAndEnabled)
            {
                agent.SetDestination(patrolPoints[0].position);
                Log($"Patrol points updated - {patrolPoints.Length} points assigned");
            }
        }
    }
    
    /// <summary>
    /// Mevcut patrol point'leri almak için
    /// </summary>
    public Transform[] GetPatrolPoints()
    {
        return patrolPoints;
    }
    
    Color GetStateColor()
    {
        switch (currentState)
        {
            case EnemyState.Patrol: return Color.white;
            case EnemyState.Chase: return Color.red;
            case EnemyState.Attack: return new Color(1f, 0f, 0f, 1f);
            case EnemyState.Search: return Color.yellow;
            case EnemyState.Investigate: return Color.cyan;
            default: return Color.gray;
        }
    }
}