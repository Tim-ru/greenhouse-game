using UnityEngine;

/// <summary>
/// Компонент плесени для интеграции с системой взаимодействий.
/// Объединяет MoldSurface и IInteractable для работы с InteractionDetector.
/// </summary>
[RequireComponent(typeof(MoldSurface))]
public class MoldInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private string promptText = "Очистить плесень";
    [SerializeField] private bool requiresWateringCan = true;
    
    private MoldSurface moldSurface;
    private bool isBeingCleaned = false;
    
    public string Prompt => promptText;
    
    private void Awake()
    {
        moldSurface = GetComponent<MoldSurface>();
    }
    
    /// <summary>
    /// Проверяет, можно ли взаимодействовать с плесенью
    /// </summary>
    public bool CanInteract(GameObject interactor)
    {
        if (moldSurface == null) return false;
        
        // Проверяем, не скрыта ли уже плесень
        if (moldSurface.IsHidden())
        {
            return false;
        }
        
        // Проверяем, не очищена ли уже плесень полностью
        float cleanPercent = moldSurface.GetCleanPercent();
        if (cleanPercent >= 0.9f) // 90% очищено = плесень исчезла
        {
            return false;
        }
        
        // Если требуется лейка, проверяем её наличие
        if (requiresWateringCan)
        {
            if (InventorySystem.Instance == null) return false;
            if (!InventorySystem.Instance.HasWateringCan) return false;
            
            var wateringCan = InventorySystem.Instance.GetWateringCan();
            if (wateringCan == null || !wateringCan.HasWater) return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Запускает процесс очистки плесени
    /// </summary>
    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;
        
        StartCleaning(interactor);
    }
    
    private void StartCleaning(GameObject interactor)
    {
        if (isBeingCleaned) return;
        
        isBeingCleaned = true;
        
        // Получаем компонент SimpleWaterController от взаимодействующего объекта
        SimpleWaterController waterController = interactor.GetComponent<SimpleWaterController>();
        if (waterController == null)
        {
            // Если у взаимодействующего объекта нет SimpleWaterController, 
            // создаем временный для очистки этой плесени
            waterController = interactor.AddComponent<SimpleWaterController>();
            
            // Настраиваем временный SimpleWaterController
            SetupTemporaryWaterController(waterController, interactor);
        }
        
        // Расходуем воду из лейки
        if (requiresWateringCan && InventorySystem.Instance != null)
        {
            var wateringCan = InventorySystem.Instance.GetWateringCan();
            if (wateringCan != null)
            {
                wateringCan.UseWater(1); // Расходуем 1 единицу воды
            }
        }
        
        Debug.Log($"[MoldInteractable] Начата очистка плесени на {gameObject.name}");
    }
    
    private void SetupTemporaryWaterController(SimpleWaterController waterController, GameObject interactor)
    {
        // SimpleWaterController автоматически создает свои эффекты
        // Никакой дополнительной настройки не требуется
        
        Debug.Log($"[MoldInteractable] Настроен временный SimpleWaterController для {gameObject.name}");
    }
    
    
    /// <summary>
    /// Останавливает процесс очистки
    /// </summary>
    public void StopCleaning()
    {
        isBeingCleaned = false;
        Debug.Log($"[MoldInteractable] Остановлена очистка плесени на {gameObject.name}");
    }
    
    /// <summary>
    /// Получает процент очистки плесени
    /// </summary>
    public float GetCleanPercent()
    {
        return moldSurface != null ? moldSurface.GetCleanPercent() : 0f;
    }
    
    /// <summary>
    /// Проверяет, полностью ли очищена плесень
    /// </summary>
    public bool IsFullyCleaned()
    {
        return GetCleanPercent() >= 0.95f;
    }
    
    private void OnDestroy()
    {
        StopCleaning();
    }
}
