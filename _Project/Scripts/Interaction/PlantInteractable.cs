using UnityEngine;

/// <summary>
/// Пример использования системы инвентаря для посадки растений
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PlantInteractable : MonoBehaviour, IInteractable
{
    [Header("Plant Settings")]
    [SerializeField] private string seedType = "Tomato";
    [SerializeField] private int seedsNeeded = 1;
    [SerializeField] private float waterNeeded = 5f;
    
    [Header("Visual")]
    [SerializeField] private SpriteRenderer plantRenderer;
    [SerializeField] private Sprite plantedSprite;
    
    private bool isPlanted = false;
    private GameRoot gameRoot;
    
    // IInteractable implementation
    public string Prompt => GetInteractionPrompt();
    public bool CanInteract(GameObject interactor) => true;
    
    void Start()
    {
        gameRoot = FindObjectOfType<GameRoot>();
    }
    
    public void Interact(GameObject interactor)
    {
        if (InventorySystem.Instance == null)
        {
            Debug.LogWarning("[PlantInteractable] No inventory system found");
            return;
        }
        
        if (!isPlanted)
        {
            TryPlantSeed();
        }
        else
        {
            TryWaterPlant();
        }
    }
    
    private void TryPlantSeed()
    {
        var inventory = InventorySystem.Instance;
        
        // Ищем подходящие семена в инвентаре
        var availableSeeds = inventory.GetAvailableSeeds();
        PlantData seedToPlant = null;
        
        // Ищем семена по имени (для обратной совместимости)
        foreach (var seed in availableSeeds)
        {
            if (seed.name.ToLower().Contains(seedType.ToLower()))
            {
                seedToPlant = seed;
                break;
            }
        }
        
        if (seedToPlant == null)
        {
            Debug.Log($"[PlantInteractable] Need {seedType} seeds to plant");
            return;
        }
        
        // Убираем семена из инвентаря
        if (inventory.UseSeed(seedToPlant))
        {
            isPlanted = true;
            
            // Обновляем визуал
            if (plantRenderer != null && plantedSprite != null)
                plantRenderer.sprite = plantedSprite;
            
            Debug.Log($"[PlantInteractable] Planted {seedToPlant.name} plant!");
        }
    }
    
    private void TryWaterPlant()
    {
        if (!isPlanted)
        {
            Debug.Log("[PlantInteractable] Nothing planted here");
            return;
        }
        
        var inventory = InventorySystem.Instance;
        
        // Проверяем, есть ли лейка в инвентаре
        if (!inventory.HasWateringCan)
        {
            Debug.Log("[PlantInteractable] Need watering can to water plants");
            return;
        }
        
        var wateringCan = inventory.GetWateringCan();
        
        // Проверяем, есть ли вода в лейке
        if (!wateringCan.HasWater)
        {
            Debug.Log("[PlantInteractable] Watering can is empty");
            return;
        }
        
        // Поливаем растение
        if (wateringCan.UseWater(waterNeeded))
        {
            Debug.Log($"[PlantInteractable] Watered {seedType} plant with {waterNeeded} water");
            
            // Здесь можно добавить логику роста растения
            // Например, увеличить размер или изменить цвет
            if (plantRenderer != null)
            {
                Color currentColor = plantRenderer.color;
                currentColor.g = Mathf.Min(1f, currentColor.g + 0.1f); // Делаем более зеленым
                plantRenderer.color = currentColor;
            }
        }
        else
        {
            Debug.Log("[PlantInteractable] Not enough water in watering can");
        }
    }
    
    private string GetInteractionPrompt()
    {
        if (!isPlanted)
        {
            var inventory = InventorySystem.Instance;
            if (inventory != null)
            {
                var availableSeeds = inventory.GetAvailableSeeds();
                bool hasMatchingSeed = false;
                foreach (var seed in availableSeeds)
                {
                    if (seed.name.ToLower().Contains(seedType.ToLower()))
                    {
                        hasMatchingSeed = true;
                        break;
                    }
                }
                
                if (hasMatchingSeed)
                    return $"Press E to Plant {seedType}";
                else
                    return $"Need {seedType} seeds to plant";
            }
            else
                return $"Need {seedType} seeds to plant";
        }
        else
        {
            var inventory = InventorySystem.Instance;
            if (inventory != null && inventory.HasWateringCan)
            {
                var wateringCan = inventory.GetWateringCan();
                if (wateringCan.HasWater)
                    return $"Press E to Water {seedType} Plant";
                else
                    return "Watering can is empty";
            }
            else
                return "Need watering can to water plant";
        }
    }
}
