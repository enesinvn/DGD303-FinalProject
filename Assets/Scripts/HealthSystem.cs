using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isDead = false;
    
    [Header("Damage Settings")]
    [SerializeField] private float invincibilityDuration = 1f;
    private float invincibilityTimer = 0f;
    
    [Header("Audio Reference")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound;
    
    [Header("UI Reference")]
    [SerializeField] private UIManager uiManager;
    
    public delegate void HealthChangedDelegate(float currentHealth, float maxHealth);
    public event HealthChangedDelegate OnHealthChanged;
    
    public delegate void DeathDelegate();
    public event DeathDelegate OnDeath;
    
    void Start()
    {
        currentHealth = maxHealth;
        isDead = false;
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    void Update()
    {
        if (invincibilityTimer > 0f)
        {
            invincibilityTimer -= Time.deltaTime;
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead || invincibilityTimer > 0f) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0f, currentHealth);
        
        invincibilityTimer = invincibilityDuration;
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (audioSource != null && hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }
        
        if (currentHealth <= 0f && !isDead)
        {
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        if (isDead) return;
        
        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    void Die()
    {
        isDead = true;
        
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        OnDeath?.Invoke();
        
        Debug.Log("Player died!");
    }
    
    public void Respawn()
    {
        currentHealth = maxHealth;
        isDead = false;
        invincibilityTimer = 0f;
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public float GetHealthPercentage()
    {
        return (currentHealth / maxHealth) * 100f;
    }
    
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public float GetMaxHealth()
    {
        return maxHealth;
    }
    
    public bool IsDead()
    {
        return isDead;
    }
    
    public bool IsInvincible()
    {
        return invincibilityTimer > 0f;
    }
}

