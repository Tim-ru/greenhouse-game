using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Система спавна плесени в определенных точках по условиям.
/// Позволяет создавать плесень в заранее заданных позициях при выполнении условий.
/// </summary>
public class MoldSpawner : MonoBehaviour
{
    [System.Serializable]
    public class MoldSpawnPoint
    {
        [Header("Position")]
        public Vector3 position;
        public string pointName = "Mold Point";
        
        [Header("Mold Settings")]
        public GameObject moldPrefab; // Префаб плесени
        public Sprite moldSprite; // Альтернатива: спрайт для создания объекта
        public bool inheritFromPrefab = true; // Наследовать настройки из префаба
        public int moldLayer = 6; // Слой плесени (используется только если inheritFromPrefab = false)
        public Vector2 colliderSize = new Vector2(1f, 1f); // Размер коллайдера (используется только если inheritFromPrefab = false)
        
        [Header("Spawn Conditions")]
        public bool spawnOnStart = false;
        public bool spawnOnTrigger = false;
        public string triggerTag = "Player";
        public float spawnDelay = 0f;
        
        [Header("State")]
        public bool isSpawned = false;
        public bool isCleaned = false;
        public GameObject spawnedMold;
    }
    
    [Header("Spawn Settings")]
    [SerializeField] private List<MoldSpawnPoint> spawnPoints = new List<MoldSpawnPoint>();
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = Color.red;
    
    [Header("Global Conditions")]
    [SerializeField] private bool requireWateringCan = true;
    [SerializeField] private bool checkTimeOfDay = false;
    [SerializeField] private float minTimeToSpawn = 0f; // Минимальное время игры для спавна
    [SerializeField] private float maxTimeToSpawn = 300f; // Максимальное время игры для спавна
    
    private void Start()
    {
        // Спавним плесень по условию spawnOnStart
        foreach (var point in spawnPoints)
        {
            if (point.spawnOnStart && CheckGlobalConditions())
            {
                SpawnMoldAtPoint(point);
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[MoldSpawner] Игрок вошел в триггер");
            
            // Спавним плесень по условию spawnOnTrigger
            foreach (var point in spawnPoints)
            {
                if (point.spawnOnTrigger && !point.isSpawned && !point.isCleaned && CheckGlobalConditions())
                {
                    Debug.Log($"[MoldSpawner] Спавним плесень в точке '{point.pointName}'");
                    
                    if (point.spawnDelay > 0)
                    {
                        StartCoroutine(SpawnWithDelay(point, point.spawnDelay));
                    }
                    else
                    {
                        SpawnMoldAtPoint(point);
                    }
                }
                else
                {
                    Debug.Log($"[MoldSpawner] Пропускаем точку '{point.pointName}': spawnOnTrigger={point.spawnOnTrigger}, isSpawned={point.isSpawned}, isCleaned={point.isCleaned}");
                }
            }
        }
    }
    
    /// <summary>
    /// Спавнит плесень в указанной точке
    /// </summary>
    public void SpawnMoldAtPoint(MoldSpawnPoint spawnPoint)
    {
        if (spawnPoint.isSpawned || spawnPoint.isCleaned) return;
        
        GameObject moldObject;
        
        // Если есть префаб - используем его
        if (spawnPoint.moldPrefab != null)
        {
            moldObject = Instantiate(spawnPoint.moldPrefab, spawnPoint.position, Quaternion.identity);
            moldObject.name = $"Mold_{spawnPoint.pointName}";
            
            // Настраиваем слой и коллайдер (только если не наследуем из префаба)
            if (!spawnPoint.inheritFromPrefab)
            {
                moldObject.layer = spawnPoint.moldLayer;
                
                BoxCollider2D collider = moldObject.GetComponent<BoxCollider2D>();
                if (collider != null)
                {
                    collider.size = spawnPoint.colliderSize;
                    Debug.Log($"[MoldSpawner] Переопределены настройки для '{spawnPoint.pointName}': layer={spawnPoint.moldLayer}, size={spawnPoint.colliderSize}");
                }
            }
            else
            {
                Debug.Log($"[MoldSpawner] Настройки наследуются из префаба для '{spawnPoint.pointName}': layer={moldObject.layer}");
            }
            
            // Инициализируем MoldSurface для префаба
            MoldSurface moldSurface = moldObject.GetComponent<MoldSurface>();
            if (moldSurface != null)
            {
                // Если Source Mold Sprite не назначен, назначаем из SpriteRenderer
                var sourceMoldSpriteField = typeof(MoldSurface).GetField("sourceMoldSprite", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (sourceMoldSpriteField.GetValue(moldSurface) == null)
                {
                    SpriteRenderer spriteRenderer = moldObject.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null && spriteRenderer.sprite != null)
                    {
                        sourceMoldSpriteField.SetValue(moldSurface, spriteRenderer.sprite);
                        Debug.Log($"[MoldSpawner] Назначен Source Mold Sprite для префаба '{spawnPoint.pointName}'");
                    }
                }
                
                // Убеждаемся, что MoldSurface инициализирован
                moldSurface.SendMessage("Awake");
                Debug.Log($"[MoldSpawner] MoldSurface инициализирован для префаба '{spawnPoint.pointName}'");
            }
            
            Debug.Log($"[MoldSpawner] Создана плесень из префаба '{spawnPoint.pointName}' в позиции {spawnPoint.position}");
        }
        else
        {
            // Если нет префаба - создаем объект из спрайта
            moldObject = CreateMoldFromSprite(spawnPoint);
            Debug.Log($"[MoldSpawner] Создана плесень из спрайта '{spawnPoint.pointName}' в позиции {spawnPoint.position}");
        }
        
        // Сохраняем ссылку на созданную плесень
        spawnPoint.spawnedMold = moldObject;
        spawnPoint.isSpawned = true;
    }
    
    /// <summary>
    /// Создает объект плесени из спрайта (fallback метод)
    /// </summary>
    private GameObject CreateMoldFromSprite(MoldSpawnPoint spawnPoint)
    {
        if (spawnPoint.moldSprite == null)
        {
            Debug.LogError($"[MoldSpawner] Нет ни префаба, ни спрайта для точки '{spawnPoint.pointName}'!");
            return null;
        }
        
        // Создаем объект плесени
        GameObject moldObject = new GameObject($"Mold_{spawnPoint.pointName}");
        moldObject.transform.position = spawnPoint.position;
        moldObject.layer = spawnPoint.moldLayer;
        
        // Добавляем SpriteRenderer
        SpriteRenderer spriteRenderer = moldObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = spawnPoint.moldSprite;
        spriteRenderer.sortingOrder = 5;
        
        // Добавляем коллайдер
        BoxCollider2D collider = moldObject.AddComponent<BoxCollider2D>();
        collider.size = spawnPoint.colliderSize;
        
        // Добавляем MoldSurface
        MoldSurface moldSurface = moldObject.AddComponent<MoldSurface>();
        
        // Настраиваем MoldSurface через рефлексию
        var sourceMoldSpriteField = typeof(MoldSurface).GetField("sourceMoldSprite", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        sourceMoldSpriteField?.SetValue(moldSurface, spawnPoint.moldSprite);
        
        // Инициализируем MoldSurface
        moldSurface.SendMessage("Awake");
        
        // Добавляем MoldInteractable
        MoldInteractable moldInteractable = moldObject.AddComponent<MoldInteractable>();
        
        return moldObject;
    }
    
    /// <summary>
    /// Спавнит плесень в точке по имени
    /// </summary>
    public void SpawnMoldAtPoint(string pointName)
    {
        var spawnPoint = spawnPoints.Find(p => p.pointName == pointName);
        if (spawnPoint != null)
        {
            SpawnMoldAtPoint(spawnPoint);
        }
        else
        {
            Debug.LogWarning($"[MoldSpawner] Точка спавна '{pointName}' не найдена!");
        }
    }
    
    /// <summary>
    /// Спавнит плесень в точке по индексу
    /// </summary>
    public void SpawnMoldAtPoint(int index)
    {
        if (index >= 0 && index < spawnPoints.Count)
        {
            SpawnMoldAtPoint(spawnPoints[index]);
        }
        else
        {
            Debug.LogWarning($"[MoldSpawner] Индекс точки спавна {index} вне диапазона!");
        }
    }
    
    /// <summary>
    /// Спавнит все плесени
    /// </summary>
    public void SpawnAllMolds()
    {
        foreach (var point in spawnPoints)
        {
            if (!point.isSpawned && !point.isCleaned)
            {
                SpawnMoldAtPoint(point);
            }
        }
    }
    
    /// <summary>
    /// Удаляет плесень в указанной точке
    /// </summary>
    public void RemoveMoldAtPoint(string pointName)
    {
        var spawnPoint = spawnPoints.Find(p => p.pointName == pointName);
        if (spawnPoint != null && spawnPoint.spawnedMold != null)
        {
            Destroy(spawnPoint.spawnedMold);
            spawnPoint.isSpawned = false;
            spawnPoint.isCleaned = true;
            Debug.Log($"[MoldSpawner] Плесень '{pointName}' удалена");
        }
    }
    
    /// <summary>
    /// Проверяет глобальные условия для спавна
    /// </summary>
    private bool CheckGlobalConditions()
    {
        // Проверяем наличие лейки
        if (requireWateringCan)
        {
            if (InventorySystem.Instance == null || !InventorySystem.Instance.HasWateringCan)
            {
                return false;
            }
        }
        
        // Проверяем время игры
        if (checkTimeOfDay)
        {
            float currentTime = Time.time;
            if (currentTime < minTimeToSpawn || currentTime > maxTimeToSpawn)
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Спавн с задержкой
    /// </summary>
    private System.Collections.IEnumerator SpawnWithDelay(MoldSpawnPoint spawnPoint, float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnMoldAtPoint(spawnPoint);
    }
    
    /// <summary>
    /// Получает информацию о точке спавна
    /// </summary>
    public MoldSpawnPoint GetSpawnPoint(string pointName)
    {
        return spawnPoints.Find(p => p.pointName == pointName);
    }
    
    /// <summary>
    /// Получает все точки спавна
    /// </summary>
    public List<MoldSpawnPoint> GetAllSpawnPoints()
    {
        return spawnPoints;
    }
    
    /// <summary>
    /// Очищает все заспавненные плесени (для отладки)
    /// </summary>
    [ContextMenu("Clear All Spawned Molds")]
    public void ClearAllSpawnedMolds()
    {
        foreach (var point in spawnPoints)
        {
            if (point.spawnedMold != null)
            {
                DestroyImmediate(point.spawnedMold);
                point.spawnedMold = null;
                point.isSpawned = false;
                point.isCleaned = false;
            }
        }
        
        Debug.Log("[MoldSpawner] Все заспавненные плесени очищены");
    }
    
    /// <summary>
    /// Сбрасывает состояние всех точек спавна
    /// </summary>
    [ContextMenu("Reset All Spawn Points")]
    public void ResetAllSpawnPoints()
    {
        foreach (var point in spawnPoints)
        {
            point.isSpawned = false;
            point.isCleaned = false;
            if (point.spawnedMold != null)
            {
                DestroyImmediate(point.spawnedMold);
                point.spawnedMold = null;
            }
        }
        
        Debug.Log("[MoldSpawner] Все точки спавна сброшены");
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Gizmos.color = gizmoColor;
        
        foreach (var point in spawnPoints)
        {
            // Рисуем точку спавна
            Gizmos.DrawWireSphere(point.position, 0.3f);
            
            // Рисуем размер коллайдера
            Gizmos.DrawWireCube(point.position, point.colliderSize);
            
            // Подписываем точку
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(point.position + Vector3.up * 0.5f, point.pointName);
            #endif
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        Gizmos.color = Color.yellow;
        
        foreach (var point in spawnPoints)
        {
            // Выделяем выбранные точки
            Gizmos.DrawSphere(point.position, 0.2f);
        }
    }
}
