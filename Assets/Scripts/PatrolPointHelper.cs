using UnityEngine;

/// <summary>
/// Enemy için patrol point'leri hızlıca oluşturmak için yardımcı script.
/// Enemy GameObject'ine ekleyin, "Create Patrol Points" butonuna tıklayın.
/// </summary>
[System.Serializable]
public class PatrolPointHelper : MonoBehaviour
{
    [Header("Patrol Point Settings")]
    [SerializeField] private int numberOfPoints = 4;
    [SerializeField] private float radius = 10f;
    [SerializeField] private float heightOffset = 0f;
    [SerializeField] private string pointNamePrefix = "PatrolPoint";
    
    [Header("Auto Setup")]
    [SerializeField] private bool autoAssignToEnemy = true;
    [SerializeField] private EnemyAI targetEnemy;
    
    [ContextMenu("Create Patrol Points Around Enemy")]
    public void CreatePatrolPoints()
    {
        if (numberOfPoints <= 0)
        {
            Debug.LogWarning("Number of points must be greater than 0!");
            return;
        }
        
        // Mevcut patrol point'leri temizle (opsiyonel)
        Transform existingParent = transform.Find("PatrolPoints");
        if (existingParent != null)
        {
            DestroyImmediate(existingParent.gameObject);
        }
        
        // Parent GameObject oluştur
        GameObject parentObject = new GameObject("PatrolPoints");
        parentObject.transform.SetParent(transform);
        parentObject.transform.localPosition = Vector3.zero;
        
        Transform[] newPatrolPoints = new Transform[numberOfPoints];
        
        // Patrol point'leri oluştur
        for (int i = 0; i < numberOfPoints; i++)
        {
            float angle = (360f / numberOfPoints) * i;
            float radians = angle * Mathf.Deg2Rad;
            
            Vector3 position = transform.position + new Vector3(
                Mathf.Cos(radians) * radius,
                heightOffset,
                Mathf.Sin(radians) * radius
            );
            
            GameObject point = new GameObject($"{pointNamePrefix}_{i + 1}");
            point.transform.position = position;
            point.transform.SetParent(parentObject.transform);
            
            // Görselleştirme için küçük bir sphere ekle (sadece Scene view'da görünür)
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(point);
            #endif
            
            newPatrolPoints[i] = point.transform;
        }
        
        // EnemyAI'ya otomatik atama
        if (autoAssignToEnemy)
        {
            if (targetEnemy == null)
            {
                targetEnemy = GetComponent<EnemyAI>();
            }
            
            if (targetEnemy == null)
            {
                targetEnemy = FindFirstObjectByType<EnemyAI>();
            }
            
            if (targetEnemy != null)
            {
                SetPatrolPoints(targetEnemy, newPatrolPoints);
            }
            else
            {
                Debug.LogWarning("EnemyAI not found! Please assign patrol points manually in Inspector.");
            }
        }
        
        Debug.Log($"Created {numberOfPoints} patrol points around {gameObject.name}!");
    }
    
    void SetPatrolPoints(EnemyAI enemy, Transform[] points)
    {
        if (enemy != null)
        {
            enemy.SetPatrolPoints(points);
            Debug.Log($"Patrol points automatically assigned to {enemy.name}!");
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Patrol point'lerin konumlarını göster
        Gizmos.color = Color.yellow;
        for (int i = 0; i < numberOfPoints; i++)
        {
            float angle = (360f / numberOfPoints) * i;
            float radians = angle * Mathf.Deg2Rad;
            
            Vector3 position = transform.position + new Vector3(
                Mathf.Cos(radians) * radius,
                heightOffset,
                Mathf.Sin(radians) * radius
            );
            
            Gizmos.DrawWireSphere(position, 0.5f);
            Gizmos.DrawLine(transform.position, position);
        }
    }
}
