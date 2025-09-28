using UnityEngine;

/// <summary>
/// Лейка для полива растений - интерактивный объект
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WateringCan : MonoBehaviour, IInteractable
{
    [Header("Watering Settings")]
    [SerializeField] private float waterCapacity = 100f;
    [SerializeField] private float currentWater = 100f;
    [SerializeField] private float waterPerUse = 10f;
    [SerializeField] private float refillRate = 20f; // Вода в секунду при заправке
    
    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer waterLevelIndicator;
    [SerializeField] private Color emptyColor = Color.red;
    [SerializeField] private Color fullColor = Color.blue;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip wateringSound;
    [SerializeField] private AudioClip refillSound;
    
    // Свойства
    public float WaterCapacity => waterCapacity;
    public float CurrentWater => currentWater;
    public float WaterPercentage => currentWater / waterCapacity;
    public bool HasWater => currentWater > 0f;
    public bool IsFull => currentWater >= waterCapacity;
    
    // IInteractable implementation
    public string Prompt => GetInteractionPrompt();
    public bool CanInteract(GameObject interactor) => true;
    
    void Start()
    {
        // Инициализируем с полной лейкой
        currentWater = waterCapacity;
        UpdateVisuals();
        
        Debug.Log($"[WateringCan] Initialized with {currentWater}/{waterCapacity} water");
    }
    
    void Update()
    {
        // Автоматическая заправка (пока что бесконечная)
        if (!IsFull)
        {
            RefillWater(Time.deltaTime * refillRate);
        }
    }
    
    public void Interact(GameObject interactor)
    {
        Debug.Log($"[WateringCan] Player interacted with watering can");
        Debug.Log($"[WateringCan] Current water: {currentWater}/{waterCapacity} ({WaterPercentage:P0})");
        
        // Проверяем, есть ли InventorySystem
        if (InventorySystem.Instance == null)
        {
            Debug.LogError("[WateringCan] InventorySystem.Instance is null! Create a GameObject with InventorySystem component.");
            ShowWateringCanInfo();
            return;
        }
        
        // Проверяем, есть ли уже лейка в инвентаре
        if (InventorySystem.Instance.HasWateringCan)
        {
            Debug.LogWarning("[WateringCan] Already have a watering can in inventory!");
            ShowWateringCanInfo();
            return;
        }
        
        Debug.Log("[WateringCan] Attempting to add watering can to inventory...");
        
        // Пытаемся добавить лейку в инвентарь
        bool added = InventorySystem.Instance.AddWateringCan(this);
        if (added)
        {
            Debug.Log("[WateringCan] Watering can added to inventory successfully!");
            Debug.Log($"[WateringCan] GameObject active state: {gameObject.activeInHierarchy}");
            // Лейка автоматически скроется через InventorySystem
            return;
        }
        else
        {
            Debug.LogError("[WateringCan] Failed to add watering can to inventory!");
            ShowWateringCanInfo();
        }
    }
    
    /// <summary>
    /// Использует воду из лейки для полива
    /// </summary>
    public bool UseWater(float amount = -1f)
    {
        if (amount < 0f) amount = waterPerUse;
        
        if (currentWater >= amount)
        {
            currentWater -= amount;
            currentWater = Mathf.Max(0f, currentWater);
            
            UpdateVisuals();
            PlayWateringSound();
            
            Debug.Log($"[WateringCan] Used {amount} water. Remaining: {currentWater}/{waterCapacity}");
            return true;
        }
        
        Debug.Log($"[WateringCan] Not enough water! Need {amount}, have {currentWater}");
        return false;
    }
    
    /// <summary>
    /// Заправляет лейку водой
    /// </summary>
    public void RefillWater(float amount)
    {
        if (IsFull) return;
        
        float oldWater = currentWater;
        currentWater = Mathf.Min(waterCapacity, currentWater + amount);
        
        if (currentWater > oldWater)
        {
            UpdateVisuals();
            
            // Играем звук заправки только если лейка стала полной
            if (IsFull)
            {
                PlayRefillSound();
            }
        }
    }
    
    /// <summary>
    /// Полностью заправляет лейку
    /// </summary>
    public void RefillToFull()
    {
        currentWater = waterCapacity;
        UpdateVisuals();
        PlayRefillSound();
        Debug.Log("[WateringCan] Refilled to full capacity");
    }
    
    private void UpdateVisuals()
    {
        if (waterLevelIndicator != null)
        {
            // Изменяем цвет в зависимости от уровня воды
            Color waterColor = Color.Lerp(emptyColor, fullColor, WaterPercentage);
            waterLevelIndicator.color = waterColor;
            
            // Масштабируем индикатор по уровню воды
            Vector3 scale = waterLevelIndicator.transform.localScale;
            scale.y = WaterPercentage;
            waterLevelIndicator.transform.localScale = scale;
        }
    }
    
    private void PlayWateringSound()
    {
        if (audioSource != null && wateringSound != null)
        {
            audioSource.PlayOneShot(wateringSound);
        }
    }
    
    private void PlayRefillSound()
    {
        if (audioSource != null && refillSound != null)
        {
            audioSource.PlayOneShot(refillSound);
        }
    }
    
    private string GetInteractionPrompt()
    {
        // Проверяем, есть ли уже лейка в инвентаре
        bool hasWateringCanInInventory = InventorySystem.Instance?.HasWateringCan ?? false;
        
        if (hasWateringCanInInventory)
        {
            return "Press E to Check Watering Can (Already have one)";
        }
        
        if (IsFull)
            return "Press E to Take Watering Can (Full)";
        else if (HasWater)
            return $"Press E to Take Watering Can ({WaterPercentage:P0})";
        else
            return "Press E to Take Watering Can (Empty)";
    }
    
    private void ShowWateringCanInfo()
    {
        string message = $"Watering Can Status:\n" +
                        $"Water: {currentWater:F1}/{waterCapacity:F1} ({WaterPercentage:P0})\n" +
                        $"Status: {(IsFull ? "Full" : HasWater ? "Ready" : "Empty")}";
        
        Debug.Log($"[WateringCan] {message}");
        
        // Здесь можно показать UI с информацией о лейке
        // Например, через UIManager или напрямую
    }
    
    /// <summary>
    /// Проверяет, может ли лейка полить растение
    /// </summary>
    public bool CanWaterPlant()
    {
        return HasWater;
    }
    
    /// <summary>
    /// Получает количество воды для полива
    /// </summary>
    public float GetWateringAmount()
    {
        return Mathf.Min(waterPerUse, currentWater);
    }
}
