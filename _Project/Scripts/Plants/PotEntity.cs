using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class PotEntity : MonoBehaviour, IInteractable
{
    [Header("Pot Configuration")]
    public PotData data;
    
    [Header("References")]
    public SeedSelectionUI seedSelectionUI;
    
    [Header("Plant Settings")]
    [SerializeField] private PlantEntity plantEntity;
    [SerializeField] private Transform plantPosition;
    
    [Header("State")]
    [SerializeField] private bool hasPlant = false;
    [SerializeField] private float waterLevel = 0f;
    
    private SpriteRenderer spriteRenderer;
    private Collider2D potCollider;
    
    public bool HasPlant => hasPlant;
    public float WaterLevel => waterLevel;
    public PlantEntity Plant => plantEntity;
    public bool CanBeWatered => hasPlant && plantEntity != null && plantEntity.NeedsWater;
    public bool NeedsWater => hasPlant && plantEntity != null && plantEntity.NeedsWater;
    
    // IInteractable implementation
    public string Prompt => GetInteractionPrompt();
    public bool CanInteract(GameObject interactor) => true;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        potCollider = GetComponent<Collider2D>();
        
        // Создаем позицию для растения если её нет
        if (plantPosition == null)
        {
            GameObject plantPos = new GameObject("PlantPosition");
            plantPos.transform.SetParent(transform);
            plantPos.transform.localPosition = Vector3.zero;
            plantPosition = plantPos.transform;
        }
        
        UpdateVisuals();
    }
    
    void Start()
    {
        Debug.Log($"[PotEntity] {gameObject.name} starting...");
        
        // Проверяем обязательные компоненты
        if (data == null)
        {
            Debug.LogError($"[PotEntity] {gameObject.name} has no PotData assigned!");
            return;
        }
        
        if (spriteRenderer == null)
        {
            Debug.LogError($"[PotEntity] {gameObject.name} has no SpriteRenderer!");
            return;
        }
        
        // Регистрируем горшок в менеджере
        var manager = Object.FindFirstObjectByType<GreenhousePotManager>();
        if (manager != null)
        {
            manager.RegisterPot(this);
            Debug.Log($"[PotEntity] {gameObject.name} registered with manager");
        }
        else
        {
            Debug.LogWarning($"[PotEntity] {gameObject.name} - no GreenhousePotManager found");
        }
        
        // Автоматическая посадка только если включена в настройках
        if (data.autoPlantOnStart && data.defaultPlantData != null && !hasPlant)
        {
            PlantSeed(data.defaultPlantData);
            Debug.Log($"[PotEntity] {gameObject.name} planted default seed (auto-plant enabled)");
        }
        
        Debug.Log($"[PotEntity] {gameObject.name} started successfully");
    }
    
    void OnDestroy()
    {
        // Отменяем регистрацию при уничтожении
        var manager = Object.FindFirstObjectByType<GreenhousePotManager>();
        if (manager != null)
        {
            manager.UnregisterPot(this);
        }
    }

    public void Interact(GameObject interactor)
    {
        Debug.Log($"[PotEntity] {gameObject.name} - взаимодействие начато");
        Debug.Log($"  - Has Plant: {hasPlant}");
        Debug.Log($"  - Can Plant Seed: {CanPlantSeed()}");
        Debug.Log($"  - Seed Selection UI: {seedSelectionUI != null}");
        Debug.Log($"  - Default Plant Data: {data.defaultPlantData != null}");
        Debug.Log($"[PotEntity] {gameObject.name} - INTERACT CALLED!");
        
        if (hasPlant)
        {
            Debug.Log($"[PotEntity] {gameObject.name} - горшок уже содержит растение");
            Debug.Log($"[PotEntity] NeedsWater: {NeedsWater}, CanBeWatered: {CanBeWatered}");
            Debug.Log($"[PotEntity] Plant water level: {plantEntity?.WaterLevel:F2}, Plant needs water: {plantEntity?.NeedsWater}");
            Debug.Log($"[PotEntity] Pot water level: {waterLevel:F2}/{data.waterCapacity:F2}");
            
            if (NeedsWater && CanBeWatered)
            {
                Debug.Log($"[PotEntity] {gameObject.name} - растение нуждается в поливе!");
                // Проверяем, есть ли лейка в инвентаре
                if (InventorySystem.Instance != null && InventorySystem.Instance.HasWateringCan)
                {
                    var wateringCan = InventorySystem.Instance.GetWateringCan();
                    if (wateringCan != null && wateringCan.HasWater)
                    {
                        Debug.Log($"[PotEntity] {gameObject.name} - поливаем растение лейкой из инвентаря");
                        WaterPotWithInventoryCan(wateringCan);
                    }
                    else
                    {
                        Debug.Log($"[PotEntity] {gameObject.name} - лейка в инвентаре пуста");
                    }
                }
                else
                {
                    Debug.Log($"[PotEntity] {gameObject.name} - нет лейки в инвентаре для полива");
                }
            }
            else if (plantEntity != null && plantEntity.IsBloomed)
            {
                Debug.Log($"[PotEntity] {gameObject.name} - растение готово к сбору урожая!");
                HarvestPlant();
            }
            else
            {
                Debug.Log($"[PotEntity] {gameObject.name} - проверяем состояние растения");
                Debug.Log($"Plant health: {plantEntity.Health:F2}, Progress: {plantEntity.Progress:F2}");
                Debug.Log($"[PotEntity] Почему нельзя полить: NeedsWater={NeedsWater}, CanBeWatered={CanBeWatered}");
            }
        }
        else if (CanPlantSeed())
        {
            Debug.Log($"[PotEntity] {gameObject.name} - пытаемся посадить семя");
            
            // Сначала пытаемся использовать инвентарь игрока
            if (InventorySystem.Instance != null)
            {
                var availableSeeds = InventorySystem.Instance.GetAvailableSeeds();
                if (availableSeeds.Count > 0)
                {
                    PlantData seedToPlant = availableSeeds[0];
                    if (InventorySystem.Instance.UseSeed(seedToPlant))
                    {
                        PlantSeed(seedToPlant);
                        Debug.Log($"[PotEntity] {gameObject.name} - семя {seedToPlant.name} посажено из инвентаря");
                        return;
                    }
                }
            }
            
            // Fallback на UI выбора семени
            if (seedSelectionUI != null)
            {
                Debug.Log($"[PotEntity] {gameObject.name} - показываем UI выбора семени");
                seedSelectionUI.ShowSeedSelection(this);
            }
            else if (data.defaultPlantData != null)
            {
                Debug.Log($"[PotEntity] {gameObject.name} - используем растение по умолчанию");
                PlantSeed(data.defaultPlantData);
            }
            else
            {
                Debug.LogWarning($"[PotEntity] {gameObject.name} - нет семян в инвентаре, UI или растения по умолчанию!");
            }
        }
        else
        {
            Debug.LogWarning($"[PotEntity] {gameObject.name} - нельзя посадить семя. Причины:");
            Debug.LogWarning($"  - hasPlant: {hasPlant}");
            Debug.LogWarning($"  - data.canHoldPlant: {data?.canHoldPlant}");
            Debug.LogWarning($"  - data: {data != null}");
        }
    }
    
    public bool CanPlantSeed()
    {
        bool canPlant = !hasPlant && data != null && data.canHoldPlant;
        Debug.Log($"[PotEntity] {gameObject.name} - CanPlantSeed: {canPlant} (hasPlant: {hasPlant}, data: {data != null}, canHoldPlant: {data?.canHoldPlant})");
        return canPlant;
    }
    
    public void PlantSeed(PlantData plantData)
    {
        Debug.Log($"[PotEntity] PlantSeed called with: {plantData?.name ?? "null"}");
        Debug.Log($"[PotEntity] CanPlantSeed: {CanPlantSeed()}, hasPlant: {hasPlant}");
        
        if (!CanPlantSeed() || plantData == null) 
        {
            Debug.LogWarning($"[PotEntity] Cannot plant seed - CanPlantSeed: {CanPlantSeed()}, plantData: {plantData != null}");
            return;
        }
        
        // Создаем растение если его нет
        if (plantEntity == null)
        {
            Debug.Log($"[PotEntity] Creating new plant entity");
            GameObject plantObj = new GameObject("Plant");
            plantObj.transform.SetParent(plantPosition);
            plantObj.transform.localPosition = Vector3.zero;
            plantObj.transform.localScale = Vector3.one * 2f; // Увеличиваем размер растения
            
            // Добавляем SpriteRenderer для растения
            var plantSpriteRenderer = plantObj.AddComponent<SpriteRenderer>();
            plantSpriteRenderer.sortingOrder = 10; // Растение должно быть поверх горшка
            plantSpriteRenderer.color = Color.white; // Убеждаемся, что растение видно
            
            plantEntity = plantObj.AddComponent<PlantEntity>();
            
            // Находим и присваиваем GreenhouseState
            var greenhouseState = Object.FindFirstObjectByType<GreenhouseState>();
            if (greenhouseState != null)
            {
                plantEntity.state = greenhouseState;
                Debug.Log($"[PotEntity] {gameObject.name} - GreenhouseState assigned to plant");
            }
            else
            {
                Debug.LogError($"[PotEntity] {gameObject.name} - No GreenhouseState found in scene!");
            }
        }
        
        Debug.Log($"[PotEntity] Calling PlantInPot with data: {plantData.name}");
        plantEntity.PlantInPot(plantData);
        hasPlant = true;
        waterLevel = data.waterCapacity; // Начальная вода в горшке
        
        // Принудительно обновляем спрайт растения
        if (plantEntity != null)
        {
            plantEntity.UpdateSprite();
        }
        
        Debug.Log($"[PotEntity] Plant seeded successfully! hasPlant: {hasPlant}, plantEntity: {plantEntity != null}");
        Debug.Log($"[PotEntity] Plant GameObject active: {plantEntity.gameObject.activeInHierarchy}");
        Debug.Log($"[PotEntity] Plant SpriteRenderer: {plantEntity.GetComponent<SpriteRenderer>()?.sprite?.name ?? "null"}");
        
        // Проверяем позицию и видимость растения
        var plantRenderer = plantEntity.GetComponent<SpriteRenderer>();
        if (plantRenderer != null)
        {
            Debug.Log($"[PotEntity] Plant position: {plantEntity.transform.position}");
            Debug.Log($"[PotEntity] Plant local position: {plantEntity.transform.localPosition}");
            Debug.Log($"[PotEntity] Plant scale: {plantEntity.transform.localScale}");
            Debug.Log($"[PotEntity] Plant sorting order: {plantRenderer.sortingOrder}");
            Debug.Log($"[PotEntity] Plant color: {plantRenderer.color}");
            Debug.Log($"[PotEntity] Plant sprite: {plantRenderer.sprite?.name ?? "null"}");
        }
        
        UpdateVisuals();
    }
    
    public void RemovePlant()
    {
        if (!hasPlant) return;
        
        if (plantEntity != null)
        {
            plantEntity.RemoveFromPot();
            DestroyImmediate(plantEntity.gameObject);
            plantEntity = null;
        }
        
        hasPlant = false;
        waterLevel = 0f;
        UpdateVisuals();
    }
    
    /// <summary>
    /// Собирает урожай с растения - удаляет растение и добавляет семена в инвентарь
    /// </summary>
    public void HarvestPlant()
    {
        if (!hasPlant || plantEntity == null || !plantEntity.IsBloomed)
        {
            Debug.LogWarning($"[PotEntity] {gameObject.name} - Cannot harvest plant. HasPlant: {hasPlant}, PlantEntity: {plantEntity != null}, IsBloomed: {plantEntity?.IsBloomed}");
            return;
        }
        
        Debug.Log($"[PotEntity] {gameObject.name} - Harvesting plant: {plantEntity.data?.name ?? "Unknown"}");
        
        // Сохраняем данные растения для добавления семян
        PlantData harvestedPlantData = plantEntity.data;
        
        // Удаляем растение из горшка
        RemovePlant();
        
        // Добавляем 2 семени этого растения в инвентарь
        if (harvestedPlantData != null && InventorySystem.Instance != null)
        {
            bool seedsAdded = InventorySystem.Instance.AddSeeds(harvestedPlantData, 2);
            if (seedsAdded)
            {
                Debug.Log($"[PotEntity] {gameObject.name} - Added 2 {harvestedPlantData.name} seeds to inventory");
            }
            else
            {
                Debug.LogWarning($"[PotEntity] {gameObject.name} - Failed to add seeds to inventory (inventory full?)");
            }
        }
        
        // Уведомляем GoalManager о собранном растении
        if (GoalManager.Instance != null)
        {
            GoalManager.Instance.OnPlantHarvested();
            Debug.Log($"[PotEntity] {gameObject.name} - Notified GoalManager about harvested plant");
        }
        
        Debug.Log($"[PotEntity] {gameObject.name} - Plant harvested successfully!");
    }
    
    public void WaterPot()
    {
        if (!CanBeWatered) return;
        
        float waterToAdd = Mathf.Min(data.waterRefillAmount, data.waterCapacity - waterLevel);
        waterLevel += waterToAdd;
        
        if (hasPlant && plantEntity != null)
        {
            plantEntity.WaterPlant(waterToAdd);
        }
        
        UpdateVisuals();
        
        Debug.Log($"Pot watered! Water level: {waterLevel:F2}");
    }
    
    /// <summary>
    /// Поливает горшок используя лейку из инвентаря
    /// </summary>
    public void WaterPotWithInventoryCan(WateringCan wateringCan)
    {
        Debug.Log($"[PotEntity] WaterPotWithInventoryCan called - CanBeWatered: {CanBeWatered}, wateringCan: {wateringCan != null}");
        Debug.Log($"[PotEntity] Current water level: {waterLevel}/{data.waterCapacity}, plant water: {plantEntity?.WaterLevel:F2}");
        
        if (!CanBeWatered || wateringCan == null) 
        {
            Debug.Log($"[PotEntity] Cannot water - CanBeWatered: {CanBeWatered}, wateringCan null: {wateringCan == null}");
            return;
        }
        
        // Поливаем растение напрямую, используя фиксированное количество воды
        float waterToAdd = data.waterRefillAmount; // Используем полное количество воды из настроек
        Debug.Log($"[PotEntity] Water calculation - waterRefillAmount: {data.waterRefillAmount:F2}, waterToAdd: {waterToAdd:F2}");
        
        bool waterUsed = wateringCan.UseWater(waterToAdd);
        
        if (waterUsed)
        {
            // Добавляем воду в горшок (если есть место)
            float potWaterToAdd = Mathf.Min(waterToAdd, data.waterCapacity - waterLevel);
            waterLevel += potWaterToAdd;
            
            // Поливаем растение
            if (hasPlant && plantEntity != null)
            {
                plantEntity.WaterPlant(waterToAdd); // Растение получает полное количество воды
                Debug.Log($"[PotEntity] Plant watered! Plant water level: {plantEntity.WaterLevel:F2}");
            }
            
            UpdateVisuals();
            
            Debug.Log($"[PotEntity] Plant watered with inventory can! Used {waterToAdd:F2} water. Pot water level: {waterLevel:F2}");
        }
        else
        {
            Debug.Log("[PotEntity] Not enough water in watering can to water plant");
        }
    }
    
    private void UpdateVisuals()
    {
        if (data == null) return;
        
        // Обновляем спрайт горшка
        spriteRenderer.sprite = hasPlant ? data.potWithPlantSprite : data.emptyPotSprite;
        
        // Убеждаемся, что горшок имеет низкий sorting order
        spriteRenderer.sortingOrder = 0;
        
        // Можно добавить визуальные эффекты воды
        Color color = spriteRenderer.color;
        color.a = Mathf.Lerp(0.7f, 1f, waterLevel);
        spriteRenderer.color = color;
        
        Debug.Log($"[PotEntity] UpdateVisuals - Pot sprite: {spriteRenderer.sprite?.name ?? "null"}, sorting order: {spriteRenderer.sortingOrder}");
    }
    
    private string GetInteractionPrompt()
    {
        if (!hasPlant)
        {
            if (CanPlantSeed())
            {
                bool hasSeeds = InventorySystem.Instance != null && InventorySystem.Instance.GetAvailableSeeds().Count > 0;
                return hasSeeds ? "Press E to Plant Seed" : "No Seeds Available";
            }
            return "Empty Pot";
        }
        else
        {
            if (NeedsWater && CanBeWatered)
            {
                // Проверяем, есть ли лейка в инвентаре
                bool hasWateringCan = InventorySystem.Instance != null && InventorySystem.Instance.HasWateringCan;
                if (hasWateringCan)
                {
                    var wateringCan = InventorySystem.Instance.GetWateringCan();
                    if (wateringCan != null && wateringCan.HasWater)
                        return "Press E to Water Plant";
                    else
                        return "Watering Can is Empty";
                }
                else
                {
                    return "Need Watering Can to Water";
                }
            }
            else if (plantEntity != null && plantEntity.IsBloomed)
                return "Press E to Harvest Plant";
            else
                return "Press E to Check Plant";
        }
    }
    
    // Метод для получения состояния горшка (для сохранения/загрузки)
    public PotState GetState()
    {
        return new PotState
        {
            hasPlant = hasPlant,
            waterLevel = waterLevel,
            plantHealth = hasPlant && plantEntity ? plantEntity.Health : 0f,
            plantProgress = hasPlant && plantEntity ? plantEntity.Progress : 0f,
            plantWaterLevel = hasPlant && plantEntity ? plantEntity.WaterLevel : 0f
        };
    }
    
    // Метод для восстановления состояния горшка
    public void LoadState(PotState state)
    {
        hasPlant = state.hasPlant;
        waterLevel = state.waterLevel;
        
        if (hasPlant && plantEntity != null)
        {
            // Восстанавливаем состояние растения
            // Примечание: PlantData нужно будет сохранять отдельно
            plantEntity.Health = state.plantHealth;
            plantEntity.Progress = state.plantProgress;
            plantEntity.WaterLevel = state.plantWaterLevel;
        }
        
        UpdateVisuals();
    }
}

[System.Serializable]
public class PotState
{
    public bool hasPlant;
    public float waterLevel;
    public float plantHealth;
    public float plantProgress;
    public float plantWaterLevel;
}
