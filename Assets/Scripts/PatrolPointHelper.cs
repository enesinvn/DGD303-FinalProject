using UnityEngine;

/// <summary>
/// Helper script to quickly create patrol points for Enemy.
/// Attach to Enemy GameObject, then click "Create Patrol Points" button.
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
        
        // Clear existing patrol points (optional)
        Transform existingParent = transform.Find("PatrolPoints");
        if (existingParent != null)
        {
            DestroyImmediate(existingParent.gameObject);
        }
        
        // Create parent GameObject
        GameObject parentObject = new GameObject("PatrolPoints");
        parentObject.transform.SetParent(transform);
        parentObject.transform.localPosition = Vector3.zero;
        
        Transform[] newPatrolPoints = new Transform[numberOfPoints];
        
        // Create patrol points
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
            
            // Add a small sphere for visualization (visible only in Scene view)
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(point);
            #endif
            
            newPatrolPoints[i] = point.transform;
        }
        
        // Auto-assign to EnemyAI
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
        // Show patrol point positions
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
