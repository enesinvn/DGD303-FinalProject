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
    
    // İleride eklenebilecek diğer veriler
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
        
        // Oyuncu pozisyonu
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 playerPos = player.transform.position;
            saveData.playerPositionX = playerPos.x;
            saveData.playerPositionY = playerPos.y;
            saveData.playerPositionZ = playerPos.z;
        }
        
        // Sağlık
        HealthSystem healthSystem = FindFirstObjectByType<HealthSystem>();
        if (healthSystem != null)
        {
            saveData.currentHealth = healthSystem.GetCurrentHealth();
        }
        
        // Stamina
        StaminaSystem staminaSystem = FindFirstObjectByType<StaminaSystem>();
        if (staminaSystem != null)
        {
            saveData.currentStamina = staminaSystem.GetCurrentStamina();
        }
        
        // Batarya
        Flashlight flashlight = FindFirstObjectByType<Flashlight>();
        if (flashlight != null)
        {
            saveData.batteryLife = flashlight.GetBatteryPercentage();
        }
        
        // Sahne
        saveData.sceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        
        // Dosyaya kaydet
        string filePath = GetSaveFilePath();
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(filePath, FileMode.Create);
        
        formatter.Serialize(stream, saveData);
        stream.Close();
        
        Debug.Log($"Oyun kaydedildi: {filePath}");
    }
    
    public bool LoadGame()
    {
        string filePath = GetSaveFilePath();
        
        if (!File.Exists(filePath))
        {
            Debug.LogWarning("Kayıt dosyası bulunamadı!");
            return false;
        }
        
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(filePath, FileMode.Open);
        
        SaveData saveData = formatter.Deserialize(stream) as SaveData;
        stream.Close();
        
        // Sahneyi yükle
        if (saveData.sceneIndex != UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(saveData.sceneIndex);
            // Sahne yüklendikten sonra verileri yükle
            Invoke(nameof(LoadGameData), 0.1f);
        }
        else
        {
            LoadGameData();
        }
        
        // Verileri yükle
        void LoadGameData()
        {
            // Oyuncu pozisyonu
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 playerPos = new Vector3(saveData.playerPositionX, saveData.playerPositionY, saveData.playerPositionZ);
                player.transform.position = playerPos;
            }
            
            // Sağlık
            HealthSystem healthSystem = FindFirstObjectByType<HealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.Heal(saveData.currentHealth - healthSystem.GetCurrentHealth());
            }
            
            // Stamina - StaminaSystem'de setter metodu yoksa eklenebilir
            // StaminaSystem staminaSystem = FindFirstObjectByType<StaminaSystem>();
            // if (staminaSystem != null) { ... }
            
            // Batarya
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
        
        Debug.Log("Oyun yüklendi!");
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
            Debug.Log("Kayıt dosyası silindi!");
        }
    }
    
    string GetSaveFilePath()
    {
        return Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
    }
}

