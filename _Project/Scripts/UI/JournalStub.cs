using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Журнал/роадмап с чекбоксами для отслеживания прогресса игры
/// </summary>
public class JournalStub : MonoBehaviour
{
    [Header("Journal Checkboxes")]
    public Toggle seedPlanted;
    public Toggle miniGame1Completed;
    public Toggle stageHalf;
    public Toggle stageFull;
    public Toggle goalCompleted;
    
    [Header("References")]
    public PlantEntity plant;
    public GreenhouseState state;
    public GoalManager goalManager;
    
    [Header("Settings")]
    public float stageHalfThreshold = 0.5f;
    public float stageFullThreshold = 1.0f;
    
    private bool flaggedClean;
    private bool flaggedMiniGame1;
    
    void Start()
    {
        // Инициализируем чекбоксы
        if (seedPlanted != null) seedPlanted.isOn = false;
        if (miniGame1Completed != null) miniGame1Completed.isOn = false;
        if (stageHalf != null) stageHalf.isOn = false;
        if (stageFull != null) stageFull.isOn = false;
        if (goalCompleted != null) goalCompleted.isOn = false;
        
        // Находим GoalManager если не назначен
        if (goalManager == null)
        {
            goalManager = FindFirstObjectByType<GoalManager>();
        }
        
        // Подписываемся на события GoalManager
        if (goalManager != null)
        {
            goalManager.OnGoalCompleted += OnGoalCompleted;
        }
        
        Debug.Log("[JournalStub] Journal initialized");
    }
    
    void Update()
    {
        UpdateJournalProgress();
    }
    
    private void UpdateJournalProgress()
    {
        if (state == null) return;
        
        // Проверяем мини-игру 1 (очистка стекла)
        if (!flaggedClean && state.Dirt < 0.2f) 
        { 
            flaggedClean = true; 
            if (miniGame1Completed != null) miniGame1Completed.isOn = true;
            Debug.Log("[JournalStub] Mini-game 1 completed: Glass cleaned");
        }
        
        // Проверяем прогресс растения
        if (plant != null)
        {
            // Стадия 50%
            if (stageHalf != null && !stageHalf.isOn && plant.Progress >= stageHalfThreshold)
            {
                stageHalf.isOn = true;
                Debug.Log($"[JournalStub] Plant reached half stage: {plant.Progress:F2}");
            }
            
            // Стадия 100%
            if (stageFull != null && !stageFull.isOn && plant.Progress >= stageFullThreshold)
            {
                stageFull.isOn = true;
                Debug.Log($"[JournalStub] Plant reached full stage: {plant.Progress:F2}");
            }
        }
        
        // Проверяем посадку семени
        if (seedPlanted != null && !seedPlanted.isOn && plant != null && plant.data != null)
        {
            seedPlanted.isOn = true;
            Debug.Log("[JournalStub] Seed planted");
        }
        
        // Проверяем цель выращивания 6 растений
        if (goalManager != null && goalCompleted != null && !goalCompleted.isOn && goalManager.IsGoalCompleted)
        {
            goalCompleted.isOn = true;
            Debug.Log("[JournalStub] Goal completed: 6 plants grown!");
        }
    }
    
    /// <summary>
    /// Принудительно отмечает мини-игру 1 как завершенную
    /// </summary>
    public void MarkMiniGame1Completed()
    {
        if (miniGame1Completed != null && !flaggedMiniGame1)
        {
            miniGame1Completed.isOn = true;
            flaggedMiniGame1 = true;
            Debug.Log("[JournalStub] Mini-game 1 marked as completed");
        }
    }
    
    /// <summary>
    /// Принудительно отмечает семя как посаженное
    /// </summary>
    public void MarkSeedPlanted()
    {
        if (seedPlanted != null && !seedPlanted.isOn)
        {
            seedPlanted.isOn = true;
            Debug.Log("[JournalStub] Seed marked as planted");
        }
    }
    
    /// <summary>
    /// Получает общий прогресс журнала (0-1)
    /// </summary>
    public float GetJournalProgress()
    {
        int completedTasks = 0;
        int totalTasks = 5; // Добавили цель выращивания 6 растений
        
        if (seedPlanted != null && seedPlanted.isOn) completedTasks++;
        if (miniGame1Completed != null && miniGame1Completed.isOn) completedTasks++;
        if (stageHalf != null && stageHalf.isOn) completedTasks++;
        if (stageFull != null && stageFull.isOn) completedTasks++;
        if (goalCompleted != null && goalCompleted.isOn) completedTasks++;
        
        return (float)completedTasks / totalTasks;
    }
    
    /// <summary>
    /// Проверяет, завершен ли журнал полностью
    /// </summary>
    public bool IsJournalComplete()
    {
        return GetJournalProgress() >= 1.0f;
    }
    
    /// <summary>
    /// Обработчик события завершения цели
    /// </summary>
    private void OnGoalCompleted()
    {
        if (goalCompleted != null)
        {
            goalCompleted.isOn = true;
            Debug.Log("[JournalStub] Goal completed event received!");
        }
    }
    
    void OnDestroy()
    {
        // Отписываемся от событий
        if (goalManager != null)
        {
            goalManager.OnGoalCompleted -= OnGoalCompleted;
        }
    }
}
