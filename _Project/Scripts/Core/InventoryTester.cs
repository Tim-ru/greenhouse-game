using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Тестовый скрипт для демонстрации работы системы инвентаря без UI
/// Следует принципам SOLID: Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
/// </summary>
public class InventoryTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private PlantData testSeedData;
    [SerializeField] private WateringCan testWateringCan;
    [SerializeField] private bool enableDebugLogs = true;
    
    [Header("Test Seeds")]
    [SerializeField] private int tomatoSeeds = 3;
    [SerializeField] private int carrotSeeds = 2;
    [SerializeField] private int lettuceSeeds = 1;
    
    void Start()
    {
        if (enableDebugLogs)
        {
            Debug.Log("=== [InventoryTester] Система инвентаря без UI ===");
            TestInventorySystem();
        }
    }
    
    void Update()
    {
        // Горячие клавиши для тестирования (используем новую Input System)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                AddTestSeeds();
            }
            
            if (Keyboard.current.wKey.wasPressedThisFrame)
            {
                TestWateringCan();
            }
            
            if (Keyboard.current.iKey.wasPressedThisFrame)
            {
                ShowInventoryInfo();
            }
            
            if (Keyboard.current.cKey.wasPressedThisFrame)
            {
                ClearInventory();
            }
            
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                DebugInventoryStatus();
            }
        }
    }
    
    private void TestInventorySystem()
    {
        if (InventorySystem.Instance == null)
        {
            Debug.LogError("[InventoryTester] InventorySystem.Instance == null!");
            Debug.LogError("  → Создайте GameObject с компонентом InventorySystem");
            return;
        }
        
        Debug.Log("✓ InventorySystem найден");
        
        // Добавляем тестовые семена
        AddTestSeeds();
        
        // Тестируем лейку
        TestWateringCan();
        
        // Показываем информацию об инвентаре
        ShowInventoryInfo();
    }
    
    private void AddTestSeeds()
    {
        if (InventorySystem.Instance == null) return;
        
        Debug.Log("[InventoryTester] Добавление тестовых семян...");
        
        // Добавляем семена по типу (если есть PlantData)
        if (testSeedData != null)
        {
            bool added = InventorySystem.Instance.AddSeeds(testSeedData, tomatoSeeds);
            if (added)
            {
                Debug.Log($"✓ Добавлено {tomatoSeeds} семян {testSeedData.name}");
            }
        }
        else
        {
            // Пытаемся добавить семена по имени
            Debug.Log("[InventoryTester] testSeedData не назначен, пытаемся добавить по имени...");
            bool added = InventorySystem.Instance.AddSeedsByName("tomato", tomatoSeeds);
            if (added)
            {
                Debug.Log($"✓ Добавлено {tomatoSeeds} семян tomato по имени");
            }
            else
            {
                Debug.LogWarning("[InventoryTester] Не удалось добавить семена tomato! Создайте PlantData asset с именем содержащим 'tomato'");
            }
        }
        
        // Показываем текущее состояние
        ShowInventoryInfo();
    }
    
    private void TestWateringCan()
    {
        if (InventorySystem.Instance == null) return;
        
        Debug.Log("[InventoryTester] Тестирование лейки...");
        
        if (testWateringCan != null)
        {
            bool added = InventorySystem.Instance.AddWateringCan(testWateringCan);
            if (added)
            {
                Debug.Log("✓ Лейка добавлена в инвентарь");
            }
            else
            {
                Debug.LogWarning("✗ Не удалось добавить лейку в инвентарь");
            }
        }
        else
        {
            Debug.LogWarning("[InventoryTester] testWateringCan не назначен! Назначьте WateringCan в инспекторе");
        }
    }
    
    private void ShowInventoryInfo()
    {
        if (InventorySystem.Instance == null) return;
        
        Debug.Log("=== [InventoryTester] Информация об инвентаре ===");
        Debug.Log(InventorySystem.Instance.GetInventoryInfo());
        
        // Дополнительная информация
        Debug.Log($"Всего семян: {InventorySystem.Instance.TotalSeeds}");
        Debug.Log($"Есть лейка: {InventorySystem.Instance.HasWateringCan}");
        
        var availableSeeds = InventorySystem.Instance.GetAvailableSeeds();
        Debug.Log($"Доступных типов семян: {availableSeeds.Count}");
        
        foreach (var seed in availableSeeds)
        {
            int count = InventorySystem.Instance.GetSeedCount(seed);
            Debug.Log($"  - {seed.name}: {count} шт.");
        }
    }
    
    private void ClearInventory()
    {
        if (InventorySystem.Instance == null) return;
        
        Debug.Log("[InventoryTester] Очистка инвентаря...");
        InventorySystem.Instance.ClearInventory();
        Debug.Log("✓ Инвентарь очищен");
    }
    
    [ContextMenu("Test Add Seeds")]
    public void TestAddSeeds()
    {
        AddTestSeeds();
    }
    
    [ContextMenu("Test Add Watering Can")]
    public void TestAddWateringCan()
    {
        TestWateringCan();
    }
    
    [ContextMenu("Show Inventory Info")]
    public void ShowInventoryInfoMenu()
    {
        ShowInventoryInfo();
    }
    
    [ContextMenu("Clear Inventory")]
    public void ClearInventoryMenu()
    {
        ClearInventory();
    }
    
    private void DebugInventoryStatus()
    {
        Debug.Log("=== [InventoryTester] Debug Inventory Status ===");
        
        if (InventorySystem.Instance == null)
        {
            Debug.LogError("❌ InventorySystem.Instance is NULL!");
            Debug.LogError("   → Create a GameObject with InventorySystem component");
            return;
        }
        
        Debug.Log("✅ InventorySystem.Instance found");
        Debug.Log($"   → Has Watering Can: {InventorySystem.Instance.HasWateringCan}");
        Debug.Log($"   → Total Seeds: {InventorySystem.Instance.TotalSeeds}");
        Debug.Log($"   → Available Seeds Count: {InventorySystem.Instance.GetAvailableSeeds().Count}");
        
        // Проверяем лейки в сцене
        WateringCan[] wateringCans = FindObjectsOfType<WateringCan>();
        Debug.Log($"   → WateringCans in scene: {wateringCans.Length}");
        
        foreach (var can in wateringCans)
        {
            Debug.Log($"     - {can.name}: Active={can.gameObject.activeInHierarchy}, Water={can.CurrentWater:F1}/{can.WaterCapacity:F1}");
        }
        
        ShowInventoryInfo();
    }
}
