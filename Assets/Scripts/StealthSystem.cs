using UnityEngine;

public class StealthSystem : MonoBehaviour
{
    [Header("Gizlilik Ayarları")]
    [SerializeField] private float stealthLevel = 1f; // 0-1 arası (1 = tamamen görünür, 0 = tamamen gizli)
    [SerializeField] private float detectionRisk = 0f; // 0-1 arası (1 = kesinlikle görülecek)
    
    [Header("Işık/Gölge")]
    [SerializeField] private bool useLightSystem = true;
    [SerializeField] private float lightCheckRadius = 1f;
    [SerializeField] private LayerMask lightLayer;
    
    [Header("Hareket Modifier")]
    [SerializeField] private float crouchStealthBonus = 0.3f; // Eğilince gizlilik artar
    [SerializeField] private float sprintStealthPenalty = 0.5f; // Koşunca gizlilik azalır
    
    [Header("Referanslar")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerHiding playerHiding;
    [SerializeField] private Flashlight flashlight;
    
    [Header("UI")]
    [SerializeField] private GameObject stealthIndicator;
    
    void Start()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
        
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }
        
        if (playerHiding == null)
        {
            playerHiding = GetComponent<PlayerHiding>();
        }
        
        if (flashlight == null)
        {
            flashlight = FindFirstObjectByType<Flashlight>();
        }
    }
    
    void Update()
    {
        CalculateStealthLevel();
        UpdateUI();
    }
    
    void CalculateStealthLevel()
    {
        // Başlangıç değeri
        float baseStealth = 1f;
        
        // Saklanıyorsa tamamen gizli
        if (playerHiding != null && playerHiding.IsHiding())
        {
            stealthLevel = 0f;
            detectionRisk = 0f;
            return;
        }
        
        // Işık kontrolü
        if (useLightSystem)
        {
            bool isInLight = CheckIfInLight();
            if (isInLight)
            {
                baseStealth = 1f; // Işıkta tamamen görünür
            }
            else
            {
                baseStealth = 0.3f; // Karanlıkta daha gizli
            }
        }
        
        // Fener kontrolü
        if (flashlight != null && flashlight.IsOn())
        {
            baseStealth = 1f; // Fener açıksa tamamen görünür
        }
        
        // Hareket modifikasyonu
        if (characterController != null)
        {
            float speed = characterController.velocity.magnitude;
            float maxSpeed = 6f; // Sprint hızı
            
            if (speed > 0.1f)
            {
                bool isCrouching = Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl);
                bool isSprinting = Input.GetKey(KeyCode.LeftShift);
                
                if (isCrouching)
                {
                    baseStealth -= crouchStealthBonus; // Eğilince daha gizli
                }
                else if (isSprinting)
                {
                    baseStealth += sprintStealthPenalty; // Koşunca daha görünür
                }
                
                // Hız bazlı modifier
                float speedRatio = speed / maxSpeed;
                baseStealth += speedRatio * 0.2f; // Hızlı hareket görünürlüğü artırır
            }
        }
        
        // Değerleri sınırla
        stealthLevel = Mathf.Clamp01(baseStealth);
        
        // Detection risk hesapla (stealth level'in tersi)
        detectionRisk = stealthLevel;
    }
    
    bool CheckIfInLight()
    {
        // Basit bir ışık kontrolü (ileride daha gelişmiş yapılabilir)
        Collider[] lights = Physics.OverlapSphere(transform.position, lightCheckRadius, lightLayer);
        return lights.Length > 0;
    }
    
    void UpdateUI()
    {
        if (stealthIndicator != null)
        {
            // UI güncellemesi (örnek: gizlilik çubuğu)
            // Burada UI güncellemesi yapılabilir
        }
    }
    
    public float GetStealthLevel()
    {
        return stealthLevel;
    }
    
    public float GetDetectionRisk()
    {
        return detectionRisk;
    }
    
    public bool IsHidden()
    {
        return stealthLevel < 0.3f;
    }
    
    public bool IsVisible()
    {
        return stealthLevel > 0.7f;
    }
}

