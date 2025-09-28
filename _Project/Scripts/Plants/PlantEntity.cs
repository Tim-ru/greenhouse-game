using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlantEntity : MonoBehaviour
{
    [Header("Plant Configuration")]
    public PlantData data;
    public GreenhouseState state;
    
    [Header("Plant State")]
    [SerializeField] private float health = 1f;   // 0..1
    [SerializeField] private float progress = 0f; // 0..1
    [SerializeField] private float waterLevel = 1f; // 0..1
    
    private SpriteRenderer spriteRenderer;
    
    public float Health 
    { 
        get => health; 
        set => health = Mathf.Clamp01(value); 
    }
    
    public float Progress 
    { 
        get => progress; 
        set => progress = Mathf.Clamp01(value); 
    }
    
    public float WaterLevel 
    { 
        get => waterLevel; 
        set => waterLevel = Mathf.Clamp01(value); 
    }
    
    public bool IsBloomed => progress >= 1f && health >= 0.8f;
    public bool NeedsWater => waterLevel < 0.3f;
    public bool IsHealthy => health > 0.5f;

    void Awake() 
    { 
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        Debug.Log($"[PlantEntity] Awake - SpriteRenderer: {spriteRenderer != null}");
        
        // Если SpriteRenderer не найден, попробуем найти его
        if (spriteRenderer == null)
        {
            Debug.LogWarning("[PlantEntity] SpriteRenderer not found in Awake, will try to get it later");
        }
        
        UpdateSprite(); 
    }

    void Update()
    {
        if (data == null || state == null) return;
        
        float deltaTime = Time.deltaTime;
        UpdateGrowth(deltaTime);
        UpdateHealth(deltaTime);
        UpdateWaterConsumption(deltaTime);
        UpdateSprite();
    }
    
    private void UpdateGrowth(float deltaTime)
    {
        // В MVP рост зависит только от воды и света
        bool waterOk = waterLevel > 0.2f;
        bool lightOk = state.Light >= 0.3f;
        
        float light = state.Light;
        float growthRate = (waterOk && lightOk) ? data.growthRateByLight.Evaluate(light) : 0f;
        
        // Логирование роста каждые 2 секунды
        if (Time.frameCount % 120 == 0)
        {
            Debug.Log($"[PlantEntity] Growth conditions - Water: {waterOk}, Light: {lightOk}, GrowthRate: {growthRate:F4}");
        }
        
        progress = Mathf.Clamp01(progress + growthRate * deltaTime * 0.1f);
    }
    
    private void UpdateHealth(float deltaTime)
    {
        // В MVP только вода и свет влияют на здоровье
        bool waterOk = waterLevel > 0.1f;
        bool lightOk = state.Light >= 0.3f;
        
        // Логирование условий каждые 2 секунды
        if (Time.frameCount % 120 == 0)
        {
            Debug.Log($"[PlantEntity] Health conditions - Water: {waterOk}, Light: {lightOk}");
            Debug.Log($"[PlantEntity] Current health: {health:F2}, Water level: {waterLevel:F2}");
        }
        
        if (waterOk && lightOk)
        {
            // Растение здорово - восстанавливаем здоровье
            health = Mathf.Clamp01(health + 0.05f * deltaTime);
        }
        else
        {
            // Растение умирает только из-за недостатка воды или света
            float damageRate = data.damagePerSecondOutside;
            
            if (!waterOk && !lightOk)
            {
                // Критический урон - нет ни воды, ни света
                health = Mathf.Clamp01(health - damageRate * 2f * deltaTime);
            }
            else if (!waterOk)
            {
                // Урон от недостатка воды
                health = Mathf.Clamp01(health - damageRate * deltaTime);
            }
            else if (!lightOk)
            {
                // Урон от недостатка света
                health = Mathf.Clamp01(health - damageRate * deltaTime);
            }
        }
    }
    
    private void UpdateWaterConsumption(float deltaTime)
    {
        float consumption = data.waterConsumptionRate * deltaTime;
        waterLevel = Mathf.Clamp01(waterLevel - consumption);
    }
    
    private bool IsTemperatureComfortable()
    {
        bool isComfortable = state.Temperature >= data.tempComfort.x && state.Temperature <= data.tempComfort.y;
        
        return isComfortable;
    }
    
    private bool IsHumidityComfortable()
    {
        bool isComfortable = state.Humidity >= data.humidityComfort.x && state.Humidity <= data.humidityComfort.y;
        
        // Логирование для отладки
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[PlantEntity] Humidity check: {state.Humidity:F2} in range [{data.humidityComfort.x:F2}, {data.humidityComfort.y:F2}] = {isComfortable}");
        }
        
        return isComfortable;
    }

    public void UpdateSprite()
    {
        Debug.Log($"[PlantEntity] UpdateSprite called - Data: {data != null}, StageSprites: {data?.stageSprites?.Length ?? 0}");
        
        // Если SpriteRenderer не найден, попробуем найти его
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            Debug.Log($"[PlantEntity] UpdateSprite - SpriteRenderer found: {spriteRenderer != null}");
        }
        
        if (spriteRenderer == null)
        {
            Debug.LogError("[PlantEntity] No SpriteRenderer found! Cannot display plant sprite.");
            return;
        }
        
        if (data?.stageSprites == null || data.stageSprites.Length == 0) 
        {
            Debug.LogWarning($"[PlantEntity] No stage sprites available! Data: {data != null}, Sprites count: {data?.stageSprites?.Length ?? 0}");
            return;
        }
        
        // Выбираем спрайт по прогрессу
        int spriteIndex = 0;
        for (int i = 0; i < data.stageThresholds.Length; i++)
        {
            if (progress >= data.stageThresholds[i]) 
                spriteIndex = i;
        }
        
        spriteIndex = Mathf.Clamp(spriteIndex, 0, data.stageSprites.Length - 1);
        spriteRenderer.sprite = data.stageSprites[spriteIndex];
        
        Debug.Log($"[PlantEntity] Sprite updated - Index: {spriteIndex}, Progress: {progress:F2}, Sprite: {spriteRenderer.sprite?.name ?? "null"}");
        
        // Визуализация увядания по альфе
        Color color = spriteRenderer.color;
        color.a = Mathf.Lerp(0.5f, 1f, health);
        spriteRenderer.color = color;
    }
    
    public void WaterPlant(float waterAmount)
    {
        Debug.Log($"[PlantEntity] WaterPlant called with amount: {waterAmount:F2}");
        Debug.Log($"[PlantEntity] Before watering - waterLevel: {waterLevel:F2}, effectiveness: {data?.wateringEffectiveness:F2}");
        
        float oldWaterLevel = waterLevel;
        waterLevel = Mathf.Clamp01(waterLevel + waterAmount * data.wateringEffectiveness);
        
        Debug.Log($"[PlantEntity] After watering - waterLevel: {oldWaterLevel:F2} -> {waterLevel:F2}");
    }
    
    public void PlantInPot(PlantData plantData)
    {
        Debug.Log($"[PlantEntity] PlantInPot called with: {plantData?.name ?? "null"}");
        Debug.Log($"[PlantEntity] Current spriteRenderer: {spriteRenderer != null}");
        
        data = plantData;
        health = 1f;
        progress = 0f;
        waterLevel = 1f;
        
        Debug.Log($"[PlantEntity] Before UpdateSprite - data: {data != null}, stageSprites: {data?.stageSprites?.Length ?? 0}");
        UpdateSprite();
        
        Debug.Log($"[PlantEntity] After UpdateSprite - sprite: {spriteRenderer?.sprite?.name ?? "null"}");
        Debug.Log($"[PlantEntity] Planted in pot - Data: {data != null}, State: {state != null}");
        if (data != null)
        {
            Debug.Log($"[PlantEntity] Plant requirements - Temp: [{data.tempComfort.x}-{data.tempComfort.y}], Humidity: [{data.humidityComfort.x}-{data.humidityComfort.y}]");
        }
    }
    
    public void RemoveFromPot()
    {
        data = null;
        health = 0f;
        progress = 0f;
        waterLevel = 0f;
        spriteRenderer.sprite = null;
    }
}
