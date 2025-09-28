using UnityEngine;

/// <summary>
/// Простой кран для пополнения воды в лейке - интерактивный объект
/// Пополняет воду в лейке при нажатии E
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WaterRefillStation : MonoBehaviour, IInteractable
{
    [Header("Refill Settings")]
    [SerializeField] private float refillAmount = 50f; // Количество воды за одно нажатие
    
    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer waterIndicator;
    [SerializeField] private Color activeColor = Color.cyan;
    [SerializeField] private Color inactiveColor = Color.gray;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip refillSound;
    
    // IInteractable implementation
    public string Prompt => GetInteractionPrompt();
    public bool CanInteract(GameObject interactor) => CanRefill();
    
    void Start()
    {
        UpdateVisuals();
        Debug.Log("[WaterRefillStation] Water refill station initialized");
    }
    
    public void Interact(GameObject interactor)
    {
        if (!CanRefill())
        {
            Debug.Log("[WaterRefillStation] Cannot refill - no watering can in inventory or already full");
            return;
        }
        
        RefillWateringCan();
    }
    
    /// <summary>
    /// Проверяет, можно ли заправить лейку
    /// </summary>
    private bool CanRefill()
    {
        if (InventorySystem.Instance == null || !InventorySystem.Instance.HasWateringCan)
        {
            return false;
        }
        
        var wateringCan = InventorySystem.Instance.GetWateringCan();
        return wateringCan != null && !wateringCan.IsFull;
    }
    
    /// <summary>
    /// Заправляет лейку водой
    /// </summary>
    private void RefillWateringCan()
    {
        if (InventorySystem.Instance == null || !InventorySystem.Instance.HasWateringCan)
        {
            Debug.LogWarning("[WaterRefillStation] No watering can in inventory to refill");
            return;
        }
        
        var wateringCan = InventorySystem.Instance.GetWateringCan();
        if (wateringCan != null)
        {
            wateringCan.RefillWater(refillAmount);
            
            // Звуковой эффект
            if (audioSource != null && refillSound != null)
            {
                audioSource.PlayOneShot(refillSound);
            }
            
            UpdateVisuals();
            
            Debug.Log($"[WaterRefillStation] Refilled watering can with {refillAmount} water. Current: {wateringCan.CurrentWater:F1}/{wateringCan.WaterCapacity:F1}");
        }
    }
    
    /// <summary>
    /// Обновляет визуальные индикаторы
    /// </summary>
    private void UpdateVisuals()
    {
        if (waterIndicator != null)
        {
            waterIndicator.color = CanRefill() ? activeColor : inactiveColor;
        }
    }
    
    /// <summary>
    /// Получает текст подсказки для взаимодействия
    /// </summary>
    private string GetInteractionPrompt()
    {
        if (InventorySystem.Instance == null || !InventorySystem.Instance.HasWateringCan)
        {
            return "Press E to Check Water Tap (No Watering Can)";
        }
        
        var wateringCan = InventorySystem.Instance.GetWateringCan();
        if (wateringCan == null)
        {
            return "Press E to Check Water Tap (No Watering Can)";
        }
        
        if (wateringCan.IsFull)
        {
            return "Press E to Check Water Tap (Watering Can Full)";
        }
        
        return $"Press E to Refill Water ({wateringCan.WaterPercentage:P0})";
    }
    
    /// <summary>
    /// Принудительно заправляет лейку до полного состояния
    /// </summary>
    public void RefillToFull()
    {
        if (InventorySystem.Instance == null || !InventorySystem.Instance.HasWateringCan)
        {
            Debug.LogWarning("[WaterRefillStation] No watering can in inventory to refill");
            return;
        }
        
        var wateringCan = InventorySystem.Instance.GetWateringCan();
        if (wateringCan != null)
        {
            wateringCan.RefillToFull();
            UpdateVisuals();
            Debug.Log("[WaterRefillStation] Forced refill to full capacity");
        }
    }
    
    /// <summary>
    /// Получает информацию о состоянии крана
    /// </summary>
    public string GetStationInfo()
    {
        string info = "Water Refill Station:\n";
        info += $"Refill Amount: {refillAmount} water\n";
        info += $"Can Refill: {CanRefill()}";
        
        if (InventorySystem.Instance != null && InventorySystem.Instance.HasWateringCan)
        {
            var wateringCan = InventorySystem.Instance.GetWateringCan();
            if (wateringCan != null)
            {
                info += $"\nWatering Can: {wateringCan.CurrentWater:F1}/{wateringCan.WaterCapacity:F1} ({wateringCan.WaterPercentage:P0})";
            }
        }
        
        return info;
    }
}
