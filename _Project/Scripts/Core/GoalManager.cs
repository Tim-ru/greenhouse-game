using System;
using UnityEngine;

/// <summary>
/// Менеджер целей игры - отслеживает прогресс выполнения различных задач
/// </summary>
public class GoalManager : MonoBehaviour
{
    [Header("Goal Settings")]
    [SerializeField] private int targetPlantsToGrow = 6;
    [SerializeField] private bool goalCompleted = false;
    [SerializeField] private int harvestedPlants = 0;
    
    [Header("References")]
    public GreenhousePotManager potManager;
    
    // Events
    public event Action<int, int> OnPlantsProgressChanged; // current, target
    public event Action<int, int> OnHarvestedProgressChanged; // harvested, target
    public event Action OnGoalCompleted;
    
    // Свойства
    public int TargetPlantsToGrow => targetPlantsToGrow;
    public int CurrentBloomedPlants => potManager != null ? potManager.GetBloomedPlants().Count : 0;
    public int HarvestedPlants => harvestedPlants;
    public bool IsGoalCompleted => goalCompleted;
    public float GoalProgress => (float)harvestedPlants / targetPlantsToGrow;
    
    public static GoalManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Находим менеджер горшков если не назначен
        if (potManager == null)
        {
            potManager = FindFirstObjectByType<GreenhousePotManager>();
        }
        
        // Подписываемся на события
        if (potManager != null)
        {
            // Проверяем прогресс при старте
            CheckGoalProgress();
        }
        else
        {
            Debug.LogWarning("[GoalManager] GreenhousePotManager not found!");
        }
    }
    
    void Update()
    {
        // Проверяем прогресс каждые несколько секунд
        if (Time.frameCount % 300 == 0) // Каждые 5 секунд при 60 FPS
        {
            CheckGoalProgress();
        }
    }
    
    /// <summary>
    /// Проверяет прогресс выполнения цели
    /// </summary>
    public void CheckGoalProgress()
    {
        if (potManager == null || goalCompleted) return;
        
        int currentBloomed = CurrentBloomedPlants;
        
        // Уведомляем о изменении прогресса
        OnPlantsProgressChanged?.Invoke(currentBloomed, targetPlantsToGrow);
        OnHarvestedProgressChanged?.Invoke(harvestedPlants, targetPlantsToGrow);
        
        // Проверяем завершение цели
        if (harvestedPlants >= targetPlantsToGrow && !goalCompleted)
        {
            CompleteGoal();
        }
        
        Debug.Log($"[GoalManager] Progress: {currentBloomed}/{targetPlantsToGrow} plants bloomed, {harvestedPlants}/{targetPlantsToGrow} harvested");
    }
    
    /// <summary>
    /// Вызывается когда растение собрано
    /// </summary>
    public void OnPlantHarvested()
    {
        if (goalCompleted) return;
        
        harvestedPlants++;
        OnHarvestedProgressChanged?.Invoke(harvestedPlants, targetPlantsToGrow);
        
        Debug.Log($"[GoalManager] Plant harvested! Progress: {harvestedPlants}/{targetPlantsToGrow}");
        
        // Проверяем завершение цели
        if (harvestedPlants >= targetPlantsToGrow)
        {
            CompleteGoal();
        }
    }
    
    /// <summary>
    /// Завершает цель
    /// </summary>
    private void CompleteGoal()
    {
        goalCompleted = true;
        OnGoalCompleted?.Invoke();
        Debug.Log($"[GoalManager] GOAL COMPLETED! Successfully grew {targetPlantsToGrow} plants!");
    }
    
    /// <summary>
    /// Сбрасывает цель (для новой игры)
    /// </summary>
    public void ResetGoal()
    {
        goalCompleted = false;
        harvestedPlants = 0;
        OnPlantsProgressChanged?.Invoke(0, targetPlantsToGrow);
        OnHarvestedProgressChanged?.Invoke(0, targetPlantsToGrow);
        Debug.Log("[GoalManager] Goal reset");
    }
    
    /// <summary>
    /// Устанавливает новую цель по количеству растений
    /// </summary>
    public void SetTargetPlants(int target)
    {
        targetPlantsToGrow = Mathf.Max(1, target);
        goalCompleted = false;
        CheckGoalProgress();
        Debug.Log($"[GoalManager] New target set: {targetPlantsToGrow} plants");
    }
}