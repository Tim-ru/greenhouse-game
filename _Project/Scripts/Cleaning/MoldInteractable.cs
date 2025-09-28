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
        
        // Получаем компонент WaterJetController от взаимодействующего объекта
        WaterJetController waterJet = interactor.GetComponent<WaterJetController>();
        if (waterJet == null)
        {
            // Если у взаимодействующего объекта нет WaterJetController, 
            // создаем временный для очистки этой плесени
            waterJet = interactor.AddComponent<WaterJetController>();
            
            // Настраиваем временный WaterJetController
            SetupTemporaryWaterJet(waterJet, interactor);
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
    
    private void SetupTemporaryWaterJet(WaterJetController waterJet, GameObject interactor)
    {
        // Создаем точку выхода воды (nozzle)
        GameObject nozzle = new GameObject("TemporaryNozzle");
        nozzle.transform.SetParent(interactor.transform);
        nozzle.transform.localPosition = Vector3.up * 0.5f; // Немного выше персонажа
        
        // Настраиваем WaterJetController
        var nozzleField = typeof(WaterJetController).GetField("nozzle", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        nozzleField?.SetValue(waterJet, nozzle.transform);
        
        // Настраиваем слой для плесени
        var moldMaskField = typeof(WaterJetController).GetField("moldMask", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (moldMaskField != null)
        {
            int moldLayer = gameObject.layer;
            LayerMask moldMask = 1 << moldLayer;
            moldMaskField.SetValue(waterJet, moldMask);
        }
        
        // Настраиваем визуальные эффекты
        SetupTemporaryVFX(waterJet, nozzle);
        
        Debug.Log($"[MoldInteractable] Настроен временный WaterJetController для {gameObject.name}");
    }
    
    private void SetupTemporaryVFX(WaterJetController waterJet, GameObject nozzle)
    {
        // Создаем временный VFX объект
        GameObject vfxObject = new GameObject("TemporaryJetVFX");
        vfxObject.transform.SetParent(nozzle.transform);
        
        // Добавляем LineRenderer и JetVFX
        LineRenderer lineRenderer = vfxObject.AddComponent<LineRenderer>();
        JetVFX jetVFX = vfxObject.AddComponent<JetVFX>();
        
        // Настраиваем ссылку в WaterJetController
        var vfxField = typeof(WaterJetController).GetField("vfx", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        vfxField?.SetValue(waterJet, jetVFX);
        
        // Настраиваем nozzle в JetVFX
        var nozzleField = typeof(JetVFX).GetField("nozzle", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        nozzleField?.SetValue(jetVFX, nozzle.transform);
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
