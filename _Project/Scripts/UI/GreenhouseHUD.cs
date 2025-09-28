using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD для отображения параметров теплицы и состояния растений
/// </summary>
public class GreenhouseHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GreenhouseState greenhouseState;
    [SerializeField] private PlantEntity plant;
    
    [Header("Environment Indicators")]
    [SerializeField] private Slider lightBar;
    [SerializeField] private Slider temperatureBar;
    [SerializeField] private Slider humidityBar;
    [SerializeField] private Slider oxygenBar;
    [SerializeField] private Slider dirtBar;
    
    [Header("Plant Indicators")]
    [SerializeField] private Slider plantHealthBar;
    [SerializeField] private Slider plantProgressBar;
    
    [Header("Inventory Indicators")]
    [SerializeField] private Image wateringCanIcon;
    [SerializeField] private Text wateringCanText;
    [SerializeField] private Slider wateringCanWaterLevel;
    
    [Header("UI Text Labels (Optional)")]
    [SerializeField] private Text temperatureText;
    [SerializeField] private Text humidityText;
    [SerializeField] private Text oxygenText;
    [SerializeField] private Text lightText;
    
    private void Start()
    {
        // Подписываемся на изменения состояния теплицы для более эффективного обновления
        if (greenhouseState != null)
        {
            greenhouseState.OnChanged += UpdateUI;
        }
        
        // Подписываемся на изменения инвентаря
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnWateringCanChanged += UpdateInventoryUI;
        }
        
        // Первоначальное обновление UI
        UpdateUI();
        UpdateInventoryUI(InventorySystem.Instance?.GetWateringCan());
    }
    
    private void OnDestroy()
    {
        // Отписываемся от событий
        if (greenhouseState != null)
        {
            greenhouseState.OnChanged -= UpdateUI;
        }
        
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnWateringCanChanged -= UpdateInventoryUI;
        }
    }
    
    private void UpdateUI()
    {
        if (greenhouseState == null) return;
        
        // Обновляем показатели окружающей среды
        UpdateEnvironmentIndicators();
        
        // Обновляем показатели растения
        if (plant != null)
        {
            UpdatePlantIndicators();
        }
    }
    
    private void UpdateEnvironmentIndicators()
    {
        // Свет (0..1)
        if (lightBar != null)
            lightBar.value = greenhouseState.Light;
            
        // Температура (нормализуем к 0..1, где 0-40°C)
        if (temperatureBar != null)
            temperatureBar.value = Mathf.InverseLerp(0f, 40f, greenhouseState.Temperature);
            
        // Влажность (0..1)
        if (humidityBar != null)
            humidityBar.value = greenhouseState.Humidity;
            
        // Кислород (0..1)
        if (oxygenBar != null)
            oxygenBar.value = greenhouseState.Oxygen;
            
        // Грязь (0..1)
        if (dirtBar != null)
            dirtBar.value = greenhouseState.Dirt;
        
        // Обновляем текстовые метки если они есть
        UpdateTextLabels();
    }
    
    private void UpdatePlantIndicators()
    {
        if (plantHealthBar != null)
            plantHealthBar.value = plant.Health;
            
        if (plantProgressBar != null)
            plantProgressBar.value = plant.Progress;
    }
    
    private void UpdateTextLabels()
    {
        // Температура с единицами измерения
        if (temperatureText != null)
            temperatureText.text = $"{greenhouseState.Temperature:F1}°C";
            
        // Влажность в процентах
        if (humidityText != null)
            humidityText.text = $"{(greenhouseState.Humidity * 100):F0}%";
            
        // Кислород в процентах
        if (oxygenText != null)
            oxygenText.text = $"{(greenhouseState.Oxygen * 100):F0}%";
            
        // Свет в процентах
        if (lightText != null)
            lightText.text = $"{(greenhouseState.Light * 100):F0}%";
    }
    
    // Методы для ручного обновления (можно вызывать из других скриптов)
    public void ForceUpdateUI()
    {
        UpdateUI();
    }
    
    public void SetGreenhouseState(GreenhouseState newState)
    {
        // Отписываемся от старого состояния
        if (greenhouseState != null)
        {
            greenhouseState.OnChanged -= UpdateUI;
        }
        
        greenhouseState = newState;
        
        // Подписываемся на новое состояние
        if (greenhouseState != null)
        {
            greenhouseState.OnChanged += UpdateUI;
            UpdateUI();
        }
    }
    
    public void SetPlant(PlantEntity newPlant)
    {
        plant = newPlant;
        UpdateUI();
    }
    
    private void UpdateInventoryUI(WateringCan wateringCan)
    {
        // Обновляем индикацию лейки
        if (wateringCanIcon != null)
        {
            wateringCanIcon.gameObject.SetActive(wateringCan != null);
        }
        
        if (wateringCanText != null)
        {
            if (wateringCan != null)
            {
                wateringCanText.text = $"Лейка: {wateringCan.CurrentWater:F0}/{wateringCan.WaterCapacity:F0} ({wateringCan.WaterPercentage:P0})";
                wateringCanText.color = wateringCan.HasWater ? Color.blue : Color.red;
            }
            else
            {
                wateringCanText.text = "Нет лейки";
                wateringCanText.color = Color.gray;
            }
        }
        
        if (wateringCanWaterLevel != null)
        {
            if (wateringCan != null)
            {
                wateringCanWaterLevel.value = wateringCan.WaterPercentage;
                wateringCanWaterLevel.gameObject.SetActive(true);
            }
            else
            {
                wateringCanWaterLevel.gameObject.SetActive(false);
            }
        }
    }
}
