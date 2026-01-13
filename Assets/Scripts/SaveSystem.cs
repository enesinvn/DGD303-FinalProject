using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class SaveData
{
    public float playerPositionX;
    public float playerPositionY;
    public float playerPositionZ;
    
    public float currentHealth;
    public float currentStamina;
    public float batteryLife;
    
    public int sceneIndex;
    
    // public int[] inventoryItems;
    // public bool[] doorsUnlocked;
}

public class SaveSystem : MonoBehaviour
{
    private const string SAVE_FILE_NAME = "savegame.dat";
    
    public static SaveSystem Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SaveGame()
    {
        SaveData saveData = new SaveData();
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 playerPos = player.transform.position;
            saveData.playerPositionX = playerPos.x;
            saveData.playerPositionY = playerPos.y;
            saveData.playerPositionZ = playerPos.z;
        }
        
        HealthSystem healthSystem = FindFirstObjectByType<HealthSystem>();
        if (healthSystem != null)
        {
            saveData.currentHealth = healthSystem.GetCurrentHealth();
        }
        
        StaminaSystem staminaSystem = FindFirstObjectByType<StaminaSystem>();
        if (staminaSystem != null)
        {
            saveData.currentStamina = staminaSystem.GetCurrentStamina();
        }
        
        Flashlight flashlight = FindFirstObjectByType<Flashlight>();
        if (flashlight != null)
        {
            saveData.batteryLife = flashlight.GetBatteryPercentage();
        }
        
        saveData.sceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        
        string filePath = GetSaveFilePath();
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(filePath, FileMode.Create);
        
        formatter.Serialize(stream, saveData);
        stream.Close();
        
        Debug.Log($"Game saved: {filePath}");
    }
    
    public bool LoadGame()
    {
        string filePath = GetSaveFilePath();
        
        if (!File.Exists(filePath))
        {
            Debug.LogWarning("Save file not found!");
            return false;
        }
        
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(filePath, FileMode.Open);
        
        SaveData saveData = formatter.Deserialize(stream) as SaveData;
        stream.Close();
        
        if (saveData.sceneIndex != UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(saveData.sceneIndex);
            Invoke(nameof(LoadGameData), 0.1f);
        }
        else
        {
            LoadGameData();
        }
        
        void LoadGameData()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 playerPos = new Vector3(saveData.playerPositionX, saveData.playerPositionY, saveData.playerPositionZ);
                player.transform.position = playerPos;
            }
            
            HealthSystem healthSystem = FindFirstObjectByType<HealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.Heal(saveData.currentHealth - healthSystem.GetCurrentHealth());
            }
            
            Flashlight flashlight = FindFirstObjectByType<Flashlight>();
            if (flashlight != null)
            {
                float currentBattery = flashlight.GetBatteryPercentage();
                float difference = saveData.batteryLife - currentBattery;
                if (difference > 0)
                {
                    flashlight.RechargeBattery(difference);
                }
            }
        }
        
        Debug.Log("Game loaded!");
        return true;
    }
    
    public bool HasSaveFile()
    {
        return File.Exists(GetSaveFilePath());
    }
    
    public void DeleteSaveFile()
    {
        string filePath = GetSaveFilePath();
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("Save file deleted!");
        }
    }
    
    string GetSaveFilePath()
    {
        return Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
    }
}

