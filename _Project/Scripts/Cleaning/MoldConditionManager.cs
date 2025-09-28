using UnityEngine;

/// <summary>
/// Менеджер условий для спавна плесени.
/// Управляет различными условиями, при которых может появляться плесень.
/// </summary>
public class MoldConditionManager : MonoBehaviour
{
    [Header("Time Conditions")]
    [SerializeField] private bool useTimeConditions = false;
    [SerializeField] private float gameStartTime = 0f;
    [SerializeField] private float moldSpawnTime = 30f; // Через сколько секунд после начала игры спавнить плесень
    
    [Header("Player Conditions")]
    [SerializeField] private bool requirePlayerInArea = false;
    [SerializeField] private Transform playerCheckArea;
    [SerializeField] private float checkRadius = 5f;
    
    [Header("Inventory Conditions")]
    [SerializeField] private bool requireWateringCan = true;
    [SerializeField] private bool requireWater = true;
    
    [Header("Random Conditions")]
    [SerializeField] private bool useRandomSpawn = false;
    [SerializeField] private float randomSpawnChance = 0.3f; // 30% шанс
    [SerializeField] private float randomCheckInterval = 10f; // Проверяем каждые 10 секунд
    
    [Header("Event Conditions")]
    [SerializeField] private bool spawnOnPlantWatered = false;
    [SerializeField] private int plantsToWater = 3; // Спавнить после полива 3 растений
    
    private MoldSpawner moldSpawner;
    private float lastRandomCheck = 0f;
    private int wateredPlantsCount = 0;
    
    private void Start()
    {
        moldSpawner = FindObjectOfType<MoldSpawner>();
        gameStartTime = Time.time;
        
        if (moldSpawner == null)
        {
            Debug.LogWarning("[MoldConditionManager] MoldSpawner не найден в сцене!");
        }
    }
    
    private void Update()
    {
        if (moldSpawner == null) return;
        
        // Проверяем временные условия
        if (useTimeConditions && Time.time - gameStartTime >= moldSpawnTime)
        {
            CheckAndSpawnMold("TimeCondition");
        }
        
        // Проверяем случайные условия
        if (useRandomSpawn && Time.time - lastRandomCheck >= randomCheckInterval)
        {
            CheckRandomSpawn();
            lastRandomCheck = Time.time;
        }
        
        // Проверяем условия игрока
        if (requirePlayerInArea && playerCheckArea != null)
        {
            CheckPlayerInArea();
        }
    }
    
    /// <summary>
    /// Проверяет все условия и спавнит плесень если они выполнены
    /// </summary>
    public bool CheckAndSpawnMold(string conditionName)
    {
        if (!CheckAllConditions()) return false;
        
        // Спавним случайную точку или все точки
        if (useRandomSpawn)
        {
            var spawnPoints = moldSpawner.GetAllSpawnPoints();
            var availablePoints = spawnPoints.FindAll(p => !p.isSpawned && !p.isCleaned);
            
            if (availablePoints.Count > 0)
            {
                var randomPoint = availablePoints[Random.Range(0, availablePoints.Count)];
                moldSpawner.SpawnMoldAtPoint(randomPoint);
                Debug.Log($"[MoldConditionManager] Плесень заспавнена по условию '{conditionName}' в точке '{randomPoint.pointName}'");
                return true;
            }
        }
        else
        {
            moldSpawner.SpawnAllMolds();
            Debug.Log($"[MoldConditionManager] Вся плесень заспавнена по условию '{conditionName}'");
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Проверяет все активные условия
    /// </summary>
    private bool CheckAllConditions()
    {
        // Проверяем инвентарь
        if (requireWateringCan)
        {
            if (InventorySystem.Instance == null || !InventorySystem.Instance.HasWateringCan)
            {
                return false;
            }
        }
        
        if (requireWater)
        {
            if (InventorySystem.Instance != null && InventorySystem.Instance.HasWateringCan)
            {
                var wateringCan = InventorySystem.Instance.GetWateringCan();
                if (wateringCan == null || !wateringCan.HasWater)
                {
                    return false;
                }
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Проверяет случайный спавн
    /// </summary>
    private void CheckRandomSpawn()
    {
        if (Random.Range(0f, 1f) <= randomSpawnChance)
        {
            CheckAndSpawnMold("RandomSpawn");
        }
    }
    
    /// <summary>
    /// Проверяет, находится ли игрок в области
    /// </summary>
    private void CheckPlayerInArea()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(player.transform.position, playerCheckArea.position);
            if (distance <= checkRadius)
            {
                CheckAndSpawnMold("PlayerInArea");
            }
        }
    }
    
    /// <summary>
    /// Вызывается при поливе растения
    /// </summary>
    public void OnPlantWatered()
    {
        if (!spawnOnPlantWatered) return;
        
        wateredPlantsCount++;
        Debug.Log($"[MoldConditionManager] Полито растений: {wateredPlantsCount}/{plantsToWater}");
        
        if (wateredPlantsCount >= plantsToWater)
        {
            CheckAndSpawnMold("PlantWatered");
            wateredPlantsCount = 0; // Сбрасываем счетчик
        }
    }
    
    /// <summary>
    /// Принудительно спавнит плесень в указанной точке
    /// </summary>
    public void ForceSpawnMold(string pointName)
    {
        if (moldSpawner != null)
        {
            moldSpawner.SpawnMoldAtPoint(pointName);
            Debug.Log($"[MoldConditionManager] Принудительный спавн плесени в точке '{pointName}'");
        }
    }
    
    /// <summary>
    /// Устанавливает временные условия
    /// </summary>
    public void SetTimeConditions(bool useTime, float spawnTime)
    {
        useTimeConditions = useTime;
        moldSpawnTime = spawnTime;
    }
    
    /// <summary>
    /// Устанавливает случайные условия
    /// </summary>
    public void SetRandomConditions(bool useRandom, float chance, float interval)
    {
        useRandomSpawn = useRandom;
        randomSpawnChance = chance;
        randomCheckInterval = interval;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (requirePlayerInArea && playerCheckArea != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(playerCheckArea.position, checkRadius);
        }
    }
}
