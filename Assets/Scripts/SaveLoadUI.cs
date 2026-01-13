using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveLoadUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button deleteSaveButton;
    
    [Header("UI Feedback")]
    [SerializeField] private GameObject saveNotification;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float notificationDuration = 2f;
    
    private void Start()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveGame);
            
        if (loadButton != null)
            loadButton.onClick.AddListener(LoadGame);
            
        if (deleteSaveButton != null)
            deleteSaveButton.onClick.AddListener(DeleteSave);
            
        UpdateLoadButtonState();
        
        if (saveNotification != null)
            saveNotification.SetActive(false);
    }
    
    public void SaveGame()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveGame();
            ShowNotification("Game saved!");
        }
        else
        {
            Debug.LogWarning("SaveSystem not found!");
        }
    }
    
    public void LoadGame()
    {
        if (SaveSystem.Instance != null)
        {
            bool success = SaveSystem.Instance.LoadGame();
            if (success)
            {
                ShowNotification("Game loaded!");
                
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ResumeGame();
                }
            }
            else
            {
                ShowNotification("Save file not found!");
            }
        }
        else
        {
            Debug.LogWarning("SaveSystem not found!");
        }
    }
    
    public void DeleteSave()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.DeleteSaveFile();
            UpdateLoadButtonState();
            ShowNotification("Save deleted!");
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
    
    public void RefreshUI()
    {
        UpdateLoadButtonState();
    }
}
