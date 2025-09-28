using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Контроллер экрана завершения игры
/// </summary>
public class EndingController : MonoBehaviour
{
    [Header("References")]
    public GoalManager goalManager;
    
    [Header("Settings")]
    public float holdTime = 2f;
    
    private bool endingTriggered = false;
    
    void Start()
    {
        // Находим GoalManager если не назначен
        if (goalManager == null)
        {
            goalManager = GoalManager.Instance;
        }
        
        // Подписываемся на событие завершения цели
        if (goalManager != null)
        {
            goalManager.OnGoalCompleted += OnGoalCompleted;
            Debug.Log("[EndingController] Subscribed to GoalManager events");
        }
        else
        {
            Debug.LogWarning("[EndingController] GoalManager not found!");
        }
        
        Debug.Log("[EndingController] Ending controller initialized");
    }
    
    void OnDestroy()
    {
        // Отписываемся от событий
        if (goalManager != null)
        {
            goalManager.OnGoalCompleted -= OnGoalCompleted;
        }
    }
    
    /// <summary>
    /// Вызывается когда цель выполнена
    /// </summary>
    private void OnGoalCompleted()
    {
        if (endingTriggered) return;
        
        Debug.Log("[EndingController] Goal completed! Starting ending sequence...");
        
        // Запускаем корутину с задержкой
        StartCoroutine(TriggerEndingWithDelay());
    }
    
    /// <summary>
    /// Корутина для запуска экрана завершения с задержкой
    /// </summary>
    private System.Collections.IEnumerator TriggerEndingWithDelay()
    {
        yield return new WaitForSeconds(holdTime);
        TriggerEnding();
    }
    
    /// <summary>
    /// Запускает экран завершения
    /// </summary>
    private void TriggerEnding()
    {
        if (endingTriggered) return;
        
        endingTriggered = true;
        
        // Загружаем сцену титров
        SceneManager.LoadScene("EndGame");
        
        Debug.Log("[EndingController] Loading EndGame scene!");
    }
    
    
    /// <summary>
    /// Принудительно загружает сцену титров (для тестирования)
    /// </summary>
    [ContextMenu("Force Load EndGame Scene")]
    public void ForceLoadEndGameScene()
    {
        SceneManager.LoadScene("EndGame");
    }
    
    /// <summary>
    /// Принудительно завершает цель (для тестирования)
    /// </summary>
    [ContextMenu("Force Complete Goal")]
    public void ForceCompleteGoal()
    {
        if (goalManager != null)
        {
            // Устанавливаем цель как выполненную
            for (int i = 0; i < goalManager.TargetPlantsToGrow; i++)
            {
                goalManager.OnPlantHarvested();
            }
            Debug.Log("[EndingController] Goal force completed for testing");
        }
    }
    
    /// <summary>
    /// Проверяет статус завершения игры
    /// </summary>
    public bool IsEndingTriggered()
    {
        return endingTriggered;
    }
}
