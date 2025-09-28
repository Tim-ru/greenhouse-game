using UnityEngine;

/// <summary>
/// Интерактивное стекло для мини-игры очистки
/// Наследуется от InteractableObstacle для поддержки bitmap-стиля движения
/// </summary>
public class GlassPanelInteractable : InteractableObstacle
{
    [Header("Mini Game Settings")]
    public MiniGameController controller;
    public GlassCleaningMiniGame gameUI;

    private void Awake()
    {
        // Устанавливаем подсказку для взаимодействия
        prompt = "Очистить стекло [E]";
    }

    void Start()
    {
        // Автоматически найти компоненты если не назначены
        if (!controller)
            controller = FindObjectOfType<MiniGameController>();
            
        if (!gameUI)
            gameUI = FindObjectOfType<GlassCleaningMiniGame>();
    }

    protected override void OnInteract(GameObject interactor)
    {
        // Проверки на null
        if (!controller)
        {
            Debug.LogWarning("GlassPanelInteractable: MiniGameController не найден!");
            return;
        }
        
        if (!gameUI)
        {
            Debug.LogWarning("GlassPanelInteractable: GlassCleaningMiniGame не найден!");
            return;
        }
        
        Debug.Log("Запуск мини-игры очистки стекла");
        
        controller.Run(gameUI, success => 
        { 
            if (success)
            {
                Debug.Log("Стекло успешно очищено!");
                // Здесь можно добавить звук или визуальные эффекты
                // Например, изменить спрайт на чистое стекло
            }
            else
            {
                Debug.Log("Очистка стекла не удалась");
            }
        });
    }

    public override bool CanInteract(GameObject interactor)
    {
        // Можно добавить дополнительные условия
        // Например: только если у игрока есть нужные предметы
        return base.CanInteract(interactor);
    }
}
