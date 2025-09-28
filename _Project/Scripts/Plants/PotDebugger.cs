using UnityEngine;

public class PotDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool enableDebugLogs = true;
    
    void Start()
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[PotDebugger] Pot {gameObject.name} started");
            CheckComponents();
            CheckReferences();
        }
    }
    
    void Update()
    {
        // Убираем Input.GetKeyDown чтобы избежать конфликта с новой Input System
        // if (enableDebugLogs && Input.GetKeyDown(KeyCode.D))
        // {
        //     Debug.Log($"[PotDebugger] Manual debug check for {gameObject.name}");
        //     CheckComponents();
        //     CheckReferences();
        // }
    }
    
    private void CheckComponents()
    {
        var potEntity = GetComponent<PotEntity>();
        var spriteRenderer = GetComponent<SpriteRenderer>();
        var collider = GetComponent<Collider2D>();
        
        Debug.Log($"[PotDebugger] Components check:");
        Debug.Log($"  - PotEntity: {(potEntity != null ? "✓" : "✗")}");
        Debug.Log($"  - SpriteRenderer: {(spriteRenderer != null ? "✓" : "✗")}");
        Debug.Log($"  - Collider2D: {(collider != null ? "✓" : "✗")}");
        
        if (potEntity != null)
        {
            Debug.Log($"  - Pot Data: {(potEntity.data != null ? "✓" : "✗")}");
            if (potEntity.data != null)
            {
                Debug.Log($"    - Data name: {potEntity.data.name}");
            }
        }
    }
    
    private void CheckReferences()
    {
        var potEntity = GetComponent<PotEntity>();
        if (potEntity != null)
        {
            Debug.Log($"[PotDebugger] References check:");
            Debug.Log($"  - Has Plant: {potEntity.HasPlant}");
            Debug.Log($"  - Water Level: {potEntity.WaterLevel}");
            Debug.Log($"  - Position: {transform.position}");
            Debug.Log($"  - Active: {gameObject.activeInHierarchy}");
            Debug.Log($"  - Enabled: {enabled}");
        }
    }
    
    void OnEnable()
    {
        if (enableDebugLogs)
            Debug.Log($"[PotDebugger] Pot {gameObject.name} enabled");
    }
    
    void OnDisable()
    {
        if (enableDebugLogs)
            Debug.Log($"[PotDebugger] Pot {gameObject.name} disabled");
    }
    
    void OnDestroy()
    {
        if (enableDebugLogs)
            Debug.Log($"[PotDebugger] Pot {gameObject.name} destroyed");
    }
}
