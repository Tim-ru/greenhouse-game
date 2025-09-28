using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Контроллер экрана завершения игры
/// </summary>
public class EndingController : MonoBehaviour
{
    [Header("References")]
    public GoalManager goalManager;
    public Canvas endingCanvas;
    public JournalStub journal;
    
    [Header("Settings")]
    public float holdTime = 2f;
    public string endingText = "Жизнь возвращается";
    
    [Header("UI Elements")]
    public Text endingTextUI;
    public Button restartButton;
    public Button quitButton;
    
    private float t;
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
        
        // Инициализируем UI
        if (endingCanvas != null)
        {
            endingCanvas.enabled = false;
        }
        
        if (endingTextUI != null)
        {
            endingTextUI.text = endingText;
        }
        
        // Настраиваем кнопки
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
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
        
        if (endingCanvas != null)
        {
            endingCanvas.enabled = true;
        }
        
        // Останавливаем время
        Time.timeScale = 0f;
        
        // Отключаем этот скрипт
        enabled = false;
        
        Debug.Log("[EndingController] Game ending triggered!");
    }
    
    /// <summary>
    /// Перезапускает игру
    /// </summary>
    public void RestartGame()
    {
        // Восстанавливаем время
        Time.timeScale = 1f;
        
        // Перезагружаем сцену
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
        
        Debug.Log("[EndingController] Game restarted");
    }
    
    /// <summary>
    /// Выходит из игры
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[EndingController] Quitting game");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    /// <summary>
    /// Принудительно показывает экран завершения (для тестирования)
    /// </summary>
    [ContextMenu("Force Show Ending")]
    public void ForceShowEnding()
    {
        TriggerEnding();
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
