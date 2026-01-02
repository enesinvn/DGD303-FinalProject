using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Kapı Ayarları")]
    [SerializeField] private bool isLocked = false;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private bool slideMode = false; 
    [SerializeField] private Vector3 slideDirection = new Vector3(0, 0, 1);
    [SerializeField] private float slideDistance = 2f;
    
    [Header("Sesler")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip lockedSound;
    
    private bool isOpen = false;
    private bool isMoving = false;
    
    private Quaternion closedRotation;
    private Quaternion openRotation;
    
    private Vector3 closedPosition;
    private Vector3 openPosition;
    
    void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; 
            audioSource.maxDistance = 10f;
        }
        
        if (slideMode)
        {
            closedPosition = transform.position;
            openPosition = closedPosition + slideDirection.normalized * slideDistance;
        }
        else
        {
            closedRotation = transform.rotation;
            openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
        }
    }
    
    void Update()
    {
        if (!isMoving) return;
        
        if (slideMode)
        {
            Vector3 targetPosition = isOpen ? openPosition : closedPosition;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * openSpeed);
            
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
        else
        {
            Quaternion targetRotation = isOpen ? openRotation : closedRotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
            
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.rotation = targetRotation;
                isMoving = false;
            }
        }
    }
    
    public void Interact()
    {
        if (isLocked)
        {
            PlaySound(lockedSound);
            Debug.Log($"{gameObject.name} kilitli!");
            return;
        }
        
        if (isMoving) return; 
        
        isOpen = !isOpen;
        isMoving = true;
        
        PlaySound(isOpen ? openSound : closeSound);
        
        // Ses çıkar (NPC'ler duyabilir)
        SoundEmitter soundEmitter = GetComponent<SoundEmitter>();
        if (soundEmitter == null)
        {
            soundEmitter = gameObject.AddComponent<SoundEmitter>();
        }
        
        if (soundEmitter != null)
        {
            soundEmitter.soundType = isOpen ? SoundEmitter.SoundType.DoorOpen : SoundEmitter.SoundType.DoorClose;
            soundEmitter.soundRadius = 10f;
            soundEmitter.EmitSound(0.6f);
        }
        
        // PlayerSoundController'a da bildir
        PlayerSoundController soundController = FindFirstObjectByType<PlayerSoundController>();
        if (soundController != null)
        {
            soundController.EmitInteractionSound(0.5f);
        }
        
        Debug.Log($"{gameObject.name} {(isOpen ? "açıldı" : "kapandı")}");
    }
    
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    public void Unlock()
    {
        isLocked = false;
        Debug.Log($"{gameObject.name} kilidi açıldı!");
    }
    
    public void Lock()
    {
        isLocked = true;
        Debug.Log($"{gameObject.name} kilitlendi!");
    }
}
