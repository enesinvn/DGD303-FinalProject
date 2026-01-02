using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveLoadUI : MonoBehaviour
{
    [Header("Butonlar")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button deleteSaveButton;
    
    [Header("UI Feedback")]
    [SerializeField] private GameObject saveNotification;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float notificationDuration = 2f;
    
    private void Start()
    {
        // Buton event'lerini bağla
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveGame);
            
        if (loadButton != null)
            loadButton.onClick.AddListener(LoadGame);
            
        if (deleteSaveButton != null)
            deleteSaveButton.onClick.AddListener(DeleteSave);
            
        // Kayıt varsa Load butonunu aktif et
        UpdateLoadButtonState();
        
        // Bildirim gizle
        if (saveNotification != null)
            saveNotification.SetActive(false);
    }
    
    public void SaveGame()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveGame();
            ShowNotification("Oyun kaydedildi!");
        }
        else
        {
            Debug.LogWarning("SaveSystem bulunamadı!");
        }
    }
    
    public void LoadGame()
    {
        if (SaveSystem.Instance != null)
        {
            bool success = SaveSystem.Instance.LoadGame();
            if (success)
            {
                ShowNotification("Oyun yüklendi!");
                
                // GameManager varsa resume yap
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ResumeGame();
                }
            }
            else
            {
                ShowNotification("Kayıt dosyası bulunamadı!");
            }
        }
        else
        {
            Debug.LogWarning("SaveSystem bulunamadı!");
        }
    }
    
    public void DeleteSave()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.DeleteSaveFile();
            UpdateLoadButtonState();
            ShowNotification("Kayıt silindi!");
        }
    }
    
    void UpdateLoadButtonState()
    {
        if (loadButton != null && SaveSystem.Instance != null)
        {
            loadButton.interactable = SaveSystem.Instance.HasSaveFile();
        }
    }
    
    public void ShowNotification(string message)
    {
        if (saveNotification != null)
        {
            if (notificationText != null)
            {
                notificationText.text = message;
            }
            
            saveNotification.SetActive(true);
            
            // Belirli süre sonra gizle
            CancelInvoke(nameof(HideNotification));
            Invoke(nameof(HideNotification), notificationDuration);
        }
        Debug.Log(message);
    }
    
    void HideNotification()
    {
        if (saveNotification != null)
            saveNotification.SetActive(false);
    }
    
    // Dışarıdan çağrılabilir - UI güncellemesi için
    public void RefreshUI()
    {
        UpdateLoadButtonState();
    }
}
