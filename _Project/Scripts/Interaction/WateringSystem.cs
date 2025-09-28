using UnityEngine;

/// <summary>
/// Система полива - управляет взаимодействием между лейкой и растениями
/// </summary>
public class WateringSystem : MonoBehaviour
{
    [Header("Watering Settings")]
    [SerializeField] private float wateringRange = 2f;
    [SerializeField] private LayerMask potLayerMask = -1;
    
    [Header("References")]
    public WateringCan wateringCan;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject wateringEffectPrefab;
    [SerializeField] private Transform effectParent;
    
    public static WateringSystem Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Находим лейку если не назначена
        if (wateringCan == null)
        {
            wateringCan = FindFirstObjectByType<WateringCan>();
        }
        
        if (wateringCan == null)
        {
            Debug.LogWarning("[WateringSystem] No WateringCan found in scene!");
        }
    }
    
    /// <summary>
    /// Пытается полить растение в указанном горшке
    /// </summary>
    public bool TryWaterPlant(PotEntity pot)
    {
        if (wateringCan == null)
        {
            Debug.LogWarning("[WateringSystem] No watering can available!");
            return false;
        }
        
        if (pot == null)
        {
            Debug.LogWarning("[WateringSystem] Pot is null!");
            return false;
        }
        
        if (!pot.HasPlant)
        {
            Debug.Log("[WateringSystem] Pot has no plant to water");
            return false;
        }
        
        if (!pot.CanBeWatered)
        {
            Debug.Log("[WateringSystem] Pot cannot be watered");
            return false;
        }
        
        if (!wateringCan.CanWaterPlant())
        {
            Debug.Log("[WateringSystem] Watering can is empty!");
            return false;
        }
        
        // Используем воду из лейки
        float waterAmount = wateringCan.GetWateringAmount();
        if (wateringCan.UseWater(waterAmount))
        {
            // Поливаем растение
            pot.WaterPot();
            
            // Создаем визуальный эффект
            CreateWateringEffect(pot.transform.position);
            
            Debug.Log($"[WateringSystem] Successfully watered plant in {pot.name}");
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Пытается полить ближайшее растение к указанной позиции
    /// </summary>
    public bool TryWaterNearestPlant(Vector3 position)
    {
        // Ищем ближайший горшок в радиусе
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(position, wateringRange, potLayerMask);
        
        PotEntity nearestPot = null;
        float nearestDistance = float.MaxValue;
        
        foreach (var collider in nearbyColliders)
        {
            PotEntity pot = collider.GetComponent<PotEntity>();
            if (pot != null && pot.HasPlant && pot.CanBeWatered)
            {
                float distance = Vector3.Distance(position, pot.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPot = pot;
                }
            }
        }
        
        if (nearestPot != null)
        {
            return TryWaterPlant(nearestPot);
        }
        
        Debug.Log("[WateringSystem] No waterable plants found nearby");
        return false;
    }
    
    /// <summary>
    /// Создает визуальный эффект полива
    /// </summary>
    private void CreateWateringEffect(Vector3 position)
    {
        if (wateringEffectPrefab != null)
        {
            Transform parent = effectParent != null ? effectParent : transform;
            GameObject effect = Instantiate(wateringEffectPrefab, position, Quaternion.identity, parent);
            
            // Уничтожаем эффект через 2 секунды
            Destroy(effect, 2f);
        }
    }
    
    /// <summary>
    /// Проверяет, есть ли растения для полива в радиусе
    /// </summary>
    public bool HasWaterablePlantsNearby(Vector3 position)
    {
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(position, wateringRange, potLayerMask);
        
        foreach (var collider in nearbyColliders)
        {
            PotEntity pot = collider.GetComponent<PotEntity>();
            if (pot != null && pot.HasPlant && pot.CanBeWatered)
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Получает ближайшее растение для полива
    /// </summary>
    public PotEntity GetNearestWaterablePlant(Vector3 position)
    {
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(position, wateringRange, potLayerMask);
        
        PotEntity nearestPot = null;
        float nearestDistance = float.MaxValue;
        
        foreach (var collider in nearbyColliders)
        {
            PotEntity pot = collider.GetComponent<PotEntity>();
            if (pot != null && pot.HasPlant && pot.CanBeWatered)
            {
                float distance = Vector3.Distance(position, pot.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPot = pot;
                }
            }
        }
        
        return nearestPot;
    }
    
    void OnDrawGizmosSelected()
    {
        // Показываем радиус полива в редакторе
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, wateringRange);
    }
}
