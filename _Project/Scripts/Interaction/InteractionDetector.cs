using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    [Header("Projection Settings")]
    [SerializeField] private float radius = 1f;
    [SerializeField] private float projectionHeight = 0.5f; // Высота проекции на пол
    [SerializeField] private LayerMask mask = -1;
    
    [Header("References")]
    [SerializeField] private InteractionPromptUI promptUI;
    
    private PlayerInputActions input;
    private IInteractable current;

    void Awake() 
    { 
        input = new PlayerInputActions();
        Debug.Log($"[InteractionDetector] Инициализация: радиус={radius}, маска={mask.value}, UI={promptUI?.name ?? "НЕ НАЗНАЧЕН"}");
    }
    void OnEnable()
    {
        input.Enable();
        input.Gameplay.Interact.performed += OnInteract;
    }
    void OnDisable()
    {
        input.Gameplay.Interact.performed -= OnInteract;
        input.Disable();
    }

    void Update()
    {
        // Создаем проекцию окружности на пол
        Vector2 projectionCenter = new Vector2(transform.position.x, transform.position.y - projectionHeight);
        
        // Используем OverlapArea для эллиптической области
        // Создаем прямоугольник, который имитирует проекцию окружности
        Vector2 bottomLeft = projectionCenter - new Vector2(radius, projectionHeight * 0.5f);
        Vector2 topRight = projectionCenter + new Vector2(radius, projectionHeight * 0.5f);
        
        // Находим ВСЕ объекты в области проекции
        Collider2D[] colliders = Physics2D.OverlapAreaAll(bottomLeft, topRight, mask);
        
        // Выбираем приоритетный объект для взаимодействия
        IInteractable newCurrent = GetPriorityInteractable(colliders, projectionCenter);
        
        // Логирование изменения текущего объекта взаимодействия
        if (newCurrent != current)
        {
            if (newCurrent != null)
                Debug.Log($"[InteractionDetector] Обнаружен новый объект для взаимодействия: {newCurrent.GetType().Name}");
            else
                Debug.Log("[InteractionDetector] Объект взаимодействия потерян");
            current = newCurrent;
        }
        
        // Показ/скрытие подсказки
        if (current != null && current.CanInteract(gameObject))
        {
            if (promptUI != null)
                promptUI.Show(current.Prompt);
        }
        else
        {
            if (current != null)
                Debug.Log("[InteractionDetector] Объект не может взаимодействовать сейчас");
            if (promptUI != null)
                promptUI.Hide();
        }
    }
    
    /// <summary>
    /// Выбирает приоритетный объект для взаимодействия из списка коллайдеров
    /// </summary>
    private IInteractable GetPriorityInteractable(Collider2D[] colliders, Vector2 projectionCenter)
    {
        if (colliders == null || colliders.Length == 0)
            return null;
        
        IInteractable priorityInteractable = null;
        int highestPriority = -1;
        
        foreach (var collider in colliders)
        {
            // Проверяем, что объект находится в пределах проекции окружности
            float distanceX = Mathf.Abs(collider.transform.position.x - projectionCenter.x);
            if (distanceX > radius)
                continue;
            
            IInteractable interactable = collider.GetComponent<IInteractable>();
            if (interactable == null) continue;
            
            int priority = GetInteractionPriority(interactable);
            if (priority > highestPriority)
            {
                highestPriority = priority;
                priorityInteractable = interactable;
            }
        }
        
        return priorityInteractable;
    }
    
    /// <summary>
    /// Определяет приоритет взаимодействия для разных типов объектов
    /// </summary>
    private int GetInteractionPriority(IInteractable interactable)
    {
        // Приоритеты (чем больше число, тем выше приоритет):
        // 1. PotEntity с растением, которое нужно полить (если есть лейка в инвентаре) - 100
        // 2. HoseInteractable для взятия шланга (если есть лейка с водой) - 98 ⭐
        // 3. MoldInteractable для очистки плесени (если есть лейка с водой) - 95
        // 4. WateringCan - только если нет лейки в инвентаре - 90
        // 5. HoseInteractable для взятия шланга (без лейки) - 88
        // 6. MoldInteractable для очистки плесени (без лейки) - 85
        // 7. PotEntity с растением, которое нужно полить (без лейки) - 80
        // 8. PotEntity с растением для проверки - 60
        // 9. MoldInteractable (нельзя очистить) - 30
        // 10. PotEntity пустой для посадки - 40
        // 11. HoseInteractable (нельзя взять) - 25
        // 12. WateringCan (уже есть в инвентаре) - 20
        // 13. Другие объекты - 10
        
        if (interactable is PotEntity pot)
        {
            Debug.Log($"[InteractionDetector] PotEntity priority check - HasPlant: {pot.HasPlant}, NeedsWater: {pot.NeedsWater}, CanBeWatered: {pot.CanBeWatered}");
            
            if (pot.HasPlant && pot.NeedsWater && pot.CanBeWatered)
            {
                Debug.Log($"[InteractionDetector] Plant needs watering!");
                // Проверяем, есть ли лейка в инвентаре
                bool hasWateringCanInInventory = InventorySystem.Instance != null && InventorySystem.Instance.HasWateringCan;
                Debug.Log($"[InteractionDetector] Has watering can in inventory: {hasWateringCanInInventory}");
                
                if (hasWateringCanInInventory)
                {
                    var wateringCan = InventorySystem.Instance.GetWateringCan();
                    bool hasWater = wateringCan != null && wateringCan.HasWater;
                    Debug.Log($"[InteractionDetector] Watering can has water: {hasWater}");
                    
                    if (hasWater)
                    {
                        Debug.Log($"[InteractionDetector] Returning priority 100 for watering");
                        return 100; // Самый высокий приоритет для полива, если есть лейка с водой
                    }
                }
                Debug.Log($"[InteractionDetector] Returning priority 80 for watering (no watering can)");
                return 80; // Высокий приоритет для полива, но нет лейки
            }
            else if (pot.HasPlant)
            {
                return 60; // Средний приоритет для проверки растения
            }
            else if (pot.CanPlantSeed())
            {
                return 40; // Низкий приоритет для посадки
            }
        }
        
        // Проверяем шланг для взятия
        if (interactable is HoseInteractable hose)
        {
            Debug.Log($"[InteractionDetector] HoseInteractable priority check - CanInteract: {hose.CanInteract(gameObject)}");
            
            if (hose.CanInteract(gameObject))
            {
                // Проверяем, есть ли лейка в инвентаре
                bool hasWateringCanInInventory = InventorySystem.Instance != null && InventorySystem.Instance.HasWateringCan;
                Debug.Log($"[InteractionDetector] Has watering can for hose: {hasWateringCanInInventory}");
                
                if (hasWateringCanInInventory)
                {
                    var wateringCan = InventorySystem.Instance.GetWateringCan();
                    bool hasWater = wateringCan != null && wateringCan.HasWater;
                    Debug.Log($"[InteractionDetector] Watering can has water for hose: {hasWater}");
                    
                    if (hasWater)
                    {
                        Debug.Log($"[InteractionDetector] Returning priority 98 for hose");
                        return 98; // Самый высокий приоритет для взятия шланга, если есть лейка с водой
                    }
                }
                Debug.Log($"[InteractionDetector] Returning priority 88 for hose (no watering can)");
                return 88; // Высокий приоритет для взятия шланга, но нет лейки
            }
            return 25; // Низкий приоритет, если шланг нельзя взять
        }
        
        // Проверяем плесень для очистки
        if (interactable is MoldInteractable mold)
        {
            Debug.Log($"[InteractionDetector] MoldInteractable priority check - CleanPercent: {mold.GetCleanPercent():P1}");
            
            if (mold.CanInteract(gameObject))
            {
                // Проверяем, есть ли лейка в инвентаре
                bool hasWateringCanInInventory = InventorySystem.Instance != null && InventorySystem.Instance.HasWateringCan;
                Debug.Log($"[InteractionDetector] Has watering can for mold cleaning: {hasWateringCanInInventory}");
                
                if (hasWateringCanInInventory)
                {
                    var wateringCan = InventorySystem.Instance.GetWateringCan();
                    bool hasWater = wateringCan != null && wateringCan.HasWater;
                    Debug.Log($"[InteractionDetector] Watering can has water for mold: {hasWater}");
                    
                    if (hasWater)
                    {
                        Debug.Log($"[InteractionDetector] Returning priority 95 for mold cleaning");
                        return 95; // Очень высокий приоритет для очистки плесени, если есть лейка с водой
                    }
                }
                Debug.Log($"[InteractionDetector] Returning priority 85 for mold cleaning (no watering can)");
                return 85; // Высокий приоритет для очистки плесени, но нет лейки
            }
            return 30; // Низкий приоритет, если плесень нельзя очистить
        }
        
        if (interactable is WateringCan)
        {
            // Лейка имеет приоритет только если её нет в инвентаре
            bool hasWateringCanInInventory = InventorySystem.Instance != null && InventorySystem.Instance.HasWateringCan;
            if (!hasWateringCanInInventory)
            {
                return 90; // Высокий приоритет для лейки, если её нет в инвентаре
            }
            return 20; // Низкий приоритет, если лейка уже есть в инвентаре
        }
        
        return 10; // Базовый приоритет для других объектов
    }

    void OnInteract(InputAction.CallbackContext ctx)
    {
        if (current != null && current.CanInteract(gameObject))
        {
            Debug.Log($"[InteractionDetector] Выполняется взаимодействие с {current.GetType().Name}");
            current.Interact(gameObject);
        }
        else
        {
            Debug.Log("[InteractionDetector] Попытка взаимодействия, но нет доступного объекта");
        }
    }

    /// <summary>
    /// Устанавливает радиус проекции
    /// </summary>
    public void SetRadius(float newRadius)
    {
        radius = newRadius;
    }

    /// <summary>
    /// Устанавливает высоту проекции
    /// </summary>
    public void SetProjectionHeight(float newHeight)
    {
        projectionHeight = newHeight;
    }

    void OnDrawGizmosSelected()
    {
        // Визуализация проекции окружности на пол
        Vector2 projectionCenter = new Vector2(transform.position.x, transform.position.y - projectionHeight);
        
        // Рисуем эллипс (проекцию окружности)
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3(projectionCenter.x, projectionCenter.y, 0);
        Vector3 size = new Vector3(radius * 2, projectionHeight, 0.1f);
        Gizmos.DrawWireCube(center, size);
        
        // Рисуем исходную окружность для сравнения
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
        
        // Рисуем линии проекции
        Gizmos.color = Color.gray;
        Gizmos.DrawLine(
            new Vector3(transform.position.x - radius, transform.position.y, 0),
            new Vector3(projectionCenter.x - radius, projectionCenter.y, 0)
        );
        Gizmos.DrawLine(
            new Vector3(transform.position.x + radius, transform.position.y, 0),
            new Vector3(projectionCenter.x + radius, projectionCenter.y, 0)
        );
    }
}
