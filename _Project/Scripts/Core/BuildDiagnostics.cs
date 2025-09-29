using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Диагностический скрипт для проверки проблем сборки Unity
/// </summary>
public class BuildDiagnostics : MonoBehaviour
{
    [Header("Diagnostic Settings")]
    [SerializeField] private bool runOnStart = true;
    [SerializeField] private bool checkMissingReferences = true;
    [SerializeField] private bool checkMissingComponents = true;
    [SerializeField] private bool checkScriptCompilation = true;
    
    void Start()
    {
        if (runOnStart)
        {
            RunDiagnostics();
        }
    }
    
    [ContextMenu("Run Diagnostics")]
    public void RunDiagnostics()
    {
        Debug.Log("=== Unity Build Diagnostics ===");
        
        if (checkScriptCompilation)
        {
            CheckScriptCompilation();
        }
        
        if (checkMissingReferences)
        {
            CheckMissingReferences();
        }
        
        if (checkMissingComponents)
        {
            CheckMissingComponents();
        }
        
        Debug.Log("=== Diagnostics Complete ===");
    }
    
    private void CheckScriptCompilation()
    {
        Debug.Log("--- Checking Script Compilation ---");
        
        // Проверяем основные компоненты
        var requiredComponents = new System.Type[]
        {
            typeof(InventorySystem),
            typeof(WateringCan),
            typeof(PlantEntity),
            typeof(PotEntity),
            typeof(GoalManager),
            typeof(GreenhouseState)
        };
        
        foreach (var componentType in requiredComponents)
        {
            var instances = FindObjectsOfType(componentType);
            Debug.Log($"✓ {componentType.Name}: {instances.Length} instances found");
        }
    }
    
    private void CheckMissingReferences()
    {
        Debug.Log("--- Checking Missing References ---");
        
        // Проверяем InventorySystem
        var inventorySystem = FindObjectOfType<InventorySystem>();
        if (inventorySystem == null)
        {
            Debug.LogError("❌ InventorySystem not found in scene!");
        }
        else
        {
            Debug.Log("✓ InventorySystem found");
        }
        
        // Проверяем GoalManager
        var goalManager = FindObjectOfType<GoalManager>();
        if (goalManager == null)
        {
            Debug.LogError("❌ GoalManager not found in scene!");
        }
        else
        {
            Debug.Log("✓ GoalManager found");
        }
        
        // Проверяем GreenhouseState
        var greenhouseState = FindObjectOfType<GreenhouseState>();
        if (greenhouseState == null)
        {
            Debug.LogError("❌ GreenhouseState not found in scene!");
        }
        else
        {
            Debug.Log("✓ GreenhouseState found");
        }
    }
    
    private void CheckMissingComponents()
    {
        Debug.Log("--- Checking Missing Components ---");
        
        // Проверяем все объекты с отсутствующими компонентами
        var allObjects = FindObjectsOfType<GameObject>();
        int missingComponents = 0;
        
        foreach (var obj in allObjects)
        {
            var components = obj.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component == null)
                {
                    missingComponents++;
                    Debug.LogWarning($"❌ Missing component on {obj.name}");
                }
            }
        }
        
        if (missingComponents == 0)
        {
            Debug.Log("✓ No missing components found");
        }
        else
        {
            Debug.LogWarning($"❌ Found {missingComponents} missing components");
        }
    }
    
    [ContextMenu("Fix Common Issues")]
    public void FixCommonIssues()
    {
        Debug.Log("=== Attempting to Fix Common Issues ===");
        
        // Перезагружаем сцену
        Debug.Log("Reloading scene...");
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
