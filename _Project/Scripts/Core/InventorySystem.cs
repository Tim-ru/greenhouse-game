using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Единая система инвентаря для хранения лейки и семян без UI
/// Следует принципам SOLID: Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
/// </summary>
public class InventorySystem : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int maxSeeds = 10;
    // [SerializeField] private bool allowMultipleWateringCans = false; // Пока не используется
    
    [Header("Starting Items")]
    [SerializeField] private bool addStartingSeeds = true;
    [SerializeField] private PlantData startingSeedData;
    [SerializeField] private int startingSeedCount = 3;
    
    // Хранилище семян (используем PlantData для типизации)
    private Dictionary<PlantData, int> seeds = new Dictionary<PlantData, int>();
    
    // Хранилище лейки
    private WateringCan inventoryWateringCan;
    
    // События для уведомления об изменениях
    public event Action<Dictionary<PlantData, int>> OnSeedsChanged;
    public event Action<WateringCan> OnWateringCanChanged;
    
    // Свойства
    public bool HasWateringCan => inventoryWateringCan != null;
    public WateringCan WateringCan => inventoryWateringCan;
    public Dictionary<PlantData, int> Seeds => new Dictionary<PlantData, int>(seeds);
    public int TotalSeeds
    {
        get
        {
            int total = 0;
            foreach (var seed in seeds.Values)
                total += seed;
            return total;
        }
    }
    
    // Статический экземпляр для глобального доступа
    public static InventorySystem Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        Debug.Log("[InventorySystem] Inventory system initialized");
        
        // Добавляем стартовые семена если включено
        if (addStartingSeeds && startingSeedData != null)
        {
            AddSeeds(startingSeedData, startingSeedCount);
            Debug.Log($"[InventorySystem] Added {startingSeedCount} starting seeds: {startingSeedData.name}");
        }
        else if (addStartingSeeds && startingSeedData == null)
        {
            Debug.LogWarning("[InventorySystem] addStartingSeeds is enabled but startingSeedData is not assigned!");
        }
    }
    
    /// <summary>
    /// Добавляет лейку в инвентарь
    /// </summary>
    public bool AddWateringCan(WateringCan wateringCan)
    {
        Debug.Log($"[InventorySystem] AddWateringCan called with: {wateringCan?.name ?? "null"}");
        
        if (wateringCan == null)
        {
            Debug.LogWarning("[InventorySystem] Cannot add null watering can");
            return false;
        }
        
        if (HasWateringCan)
        {
            Debug.LogWarning("[InventorySystem] Already have a watering can in inventory");
            return false;
        }
        
        Debug.Log($"[InventorySystem] Setting watering can to inventory. GameObject active before: {wateringCan.gameObject.activeInHierarchy}");
        
        inventoryWateringCan = wateringCan;
        
        // Скрываем лейку со сцены
        wateringCan.gameObject.SetActive(false);
        
        Debug.Log($"[InventorySystem] GameObject active after: {wateringCan.gameObject.activeInHierarchy}");
        
        OnWateringCanChanged?.Invoke(inventoryWateringCan);
        Debug.Log("[InventorySystem] Added watering can to inventory");
        
        return true;
    }
    
    /// <summary>
    /// Убирает лейку из инвентаря и возвращает её на сцену
    /// </summary>
    public bool RemoveWateringCan(Vector3 position)
    {
        if (!HasWateringCan)
        {
            Debug.LogWarning("[InventorySystem] No watering can in inventory");
            return false;
        }
        
        var wateringCan = inventoryWateringCan;
        inventoryWateringCan = null;
        
        // Показываем лейку на сцене в указанной позиции
        wateringCan.gameObject.SetActive(true);
        wateringCan.transform.position = position;
        
        OnWateringCanChanged?.Invoke(null);
        Debug.Log("[InventorySystem] Removed watering can from inventory");
        
        return true;
    }
    
    /// <summary>
    /// Получает лейку из инвентаря для использования
    /// </summary>
    public WateringCan GetWateringCan()
    {
        return HasWateringCan ? inventoryWateringCan : null;
    }
    
    /// <summary>
    /// Добавляет семена в инвентарь
    /// </summary>
    public bool AddSeeds(PlantData plantData, int amount)
    {
        if (plantData == null)
        {
            Debug.LogWarning("[InventorySystem] Invalid plant data");
            return false;
        }
        
        if (amount <= 0)
        {
            Debug.LogWarning("[InventorySystem] Invalid seed amount");
            return false;
        }
        
        // Проверяем, не превысим ли лимит
        int currentTotal = TotalSeeds;
        if (currentTotal + amount > maxSeeds)
        {
            Debug.LogWarning($"[InventorySystem] Cannot add {amount} seeds. Would exceed max limit of {maxSeeds}");
            return false;
        }
        
        if (seeds.ContainsKey(plantData))
            seeds[plantData] += amount;
        else
            seeds[plantData] = amount;
        
        OnSeedsChanged?.Invoke(seeds);
        Debug.Log($"[InventorySystem] Added {amount} {plantData.name} seeds. Total: {seeds[plantData]}");
        
        return true;
    }
    
    /// <summary>
    /// Добавляет семена по строковому типу (для обратной совместимости)
    /// </summary>
    public bool AddSeeds(string seedType, int amount)
    {
        Debug.LogWarning("[InventorySystem] AddSeeds(string) is deprecated. Use AddSeeds(PlantData) instead.");
        return false;
    }
    
    /// <summary>
    /// Убирает семена из инвентаря
    /// </summary>
    public bool RemoveSeeds(PlantData plantData, int amount)
    {
        if (plantData == null || amount <= 0)
            return false;
        
        if (!seeds.ContainsKey(plantData) || seeds[plantData] < amount)
        {
            Debug.LogWarning($"[InventorySystem] Not enough {plantData.name} seeds. Have: {seeds.GetValueOrDefault(plantData, 0)}, Need: {amount}");
            return false;
        }
        
        seeds[plantData] -= amount;
        if (seeds[plantData] <= 0)
            seeds.Remove(plantData);
        
        OnSeedsChanged?.Invoke(seeds);
        Debug.Log($"[InventorySystem] Removed {amount} {plantData.name} seeds. Remaining: {seeds.GetValueOrDefault(plantData, 0)}");
        
        return true;
    }
    
    /// <summary>
    /// Убирает семена по строковому типу (для обратной совместимости)
    /// </summary>
    public bool RemoveSeeds(string seedType, int amount)
    {
        Debug.LogWarning("[InventorySystem] RemoveSeeds(string) is deprecated. Use RemoveSeeds(PlantData) instead.");
        return false;
    }
    
    /// <summary>
    /// Проверяет, есть ли семена определенного типа
    /// </summary>
    public bool HasSeeds(PlantData plantData, int amount = 1)
    {
        return seeds.ContainsKey(plantData) && seeds[plantData] >= amount;
    }
    
    /// <summary>
    /// Проверяет, есть ли семена по строковому типу (для обратной совместимости)
    /// </summary>
    public bool HasSeeds(string seedType, int amount = 1)
    {
        Debug.LogWarning("[InventorySystem] HasSeeds(string) is deprecated. Use HasSeeds(PlantData) instead.");
        return false;
    }
    
    /// <summary>
    /// Получает количество семян определенного типа
    /// </summary>
    public int GetSeedCount(PlantData plantData)
    {
        return seeds.GetValueOrDefault(plantData, 0);
    }
    
    /// <summary>
    /// Получает количество семян по строковому типу (для обратной совместимости)
    /// </summary>
    public int GetSeedCount(string seedType)
    {
        Debug.LogWarning("[InventorySystem] GetSeedCount(string) is deprecated. Use GetSeedCount(PlantData) instead.");
        return 0;
    }
    
    /// <summary>
    /// Получает информацию о всех семенах в инвентаре
    /// </summary>
    public string GetInventoryInfo()
    {
        var info = "Inventory:\n";
        
        if (HasWateringCan)
            info += $"• Watering Can: {inventoryWateringCan.CurrentWater:F1}/{inventoryWateringCan.WaterCapacity:F1} water\n";
        else
            info += "• No Watering Can\n";
        
        if (seeds.Count > 0)
        {
            info += "• Seeds:\n";
            foreach (var seed in seeds)
                info += $"  - {seed.Key.name}: {seed.Value}\n";
        }
        else
        {
            info += "• No Seeds\n";
        }
        
        info += $"• Total Seeds: {TotalSeeds}/{maxSeeds}";
        
        return info;
    }
    
    /// <summary>
    /// Получает список всех доступных семян
    /// </summary>
    public List<PlantData> GetAvailableSeeds()
    {
        List<PlantData> availableSeeds = new List<PlantData>();
        foreach (var seed in seeds)
        {
            if (seed.Value > 0)
                availableSeeds.Add(seed.Key);
        }
        return availableSeeds;
    }
    
    /// <summary>
    /// Использует семя (уменьшает количество на 1)
    /// </summary>
    public bool UseSeed(PlantData plantData)
    {
        return RemoveSeeds(plantData, 1);
    }
    
    /// <summary>
    /// Добавляет семена по имени (для удобства)
    /// </summary>
    public bool AddSeedsByName(string seedName, int amount)
    {
        // Ищем PlantData по имени
        PlantData[] allPlantData = Resources.FindObjectsOfTypeAll<PlantData>();
        foreach (var plantData in allPlantData)
        {
            if (plantData.name.ToLower().Contains(seedName.ToLower()))
            {
                return AddSeeds(plantData, amount);
            }
        }
        
        Debug.LogWarning($"[InventorySystem] PlantData with name containing '{seedName}' not found!");
        return false;
    }
    
    /// <summary>
    /// Очищает весь инвентарь (для тестирования)
    /// </summary>
    public void ClearInventory()
    {
        if (HasWateringCan)
        {
            inventoryWateringCan.gameObject.SetActive(true);
            inventoryWateringCan = null;
        }
        
        seeds.Clear();
        
        OnWateringCanChanged?.Invoke(null);
        OnSeedsChanged?.Invoke(seeds);
        
        Debug.Log("[InventorySystem] Inventory cleared");
    }
}
