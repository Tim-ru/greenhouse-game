using UnityEngine;

/// <summary>
/// Утилита для автоматической настройки триггеров камеры
/// </summary>
public class CameraTriggerSetup : MonoBehaviour
{
    [Header("Setup Settings")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private Vector3 leftDoorTriggerSize = new Vector3(2f, 2f, 1f);
    [SerializeField] private Vector3 rightDoorTriggerSize = new Vector3(2f, 2f, 1f);
    [SerializeField] private Vector3 leftDoorTriggerOffset = new Vector3(-8f, 0f, 0f);
    [SerializeField] private Vector3 rightDoorTriggerOffset = new Vector3(8f, 0f, 0f);
    
    [Header("References")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Transform leftDoorTriggerParent;
    [SerializeField] private Transform rightDoorTriggerParent;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupCameraTriggers();
        }
    }
    
    /// <summary>
    /// Автоматически настраивает триггеры для камеры
    /// </summary>
    [ContextMenu("Setup Camera Triggers")]
    public void SetupCameraTriggers()
    {
        // Находим CameraController если не назначен
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CameraController>();
            if (cameraController == null)
            {
                Debug.LogError("[CameraTriggerSetup] CameraController не найден в сцене!");
                return;
            }
        }
        
        // Создаем триггер для левой двери
        CreateDoorTrigger("LeftDoorTrigger", leftDoorTriggerOffset, leftDoorTriggerSize, CameraTriggerType.LeftDoor, leftDoorTriggerParent);
        
        // Создаем триггер для правой двери
        CreateDoorTrigger("RightDoorTrigger", rightDoorTriggerOffset, rightDoorTriggerSize, CameraTriggerType.RightDoor, rightDoorTriggerParent);
        
        Debug.Log("[CameraTriggerSetup] Триггеры камеры настроены успешно!");
    }
    
    private void CreateDoorTrigger(string triggerName, Vector3 offset, Vector3 size, CameraTriggerType triggerType, Transform parent)
    {
        // Создаем объект триггера
        GameObject triggerObject = new GameObject(triggerName);
        triggerObject.transform.SetParent(parent != null ? parent : transform);
        triggerObject.transform.localPosition = offset;
        
        // Добавляем компоненты
        BoxCollider2D collider = triggerObject.AddComponent<BoxCollider2D>();
        CameraTrigger cameraTrigger = triggerObject.AddComponent<CameraTrigger>();
        
        // Настраиваем коллайдер
        collider.isTrigger = true;
        collider.size = size;
        
        // Настраиваем триггер
        cameraTrigger.SetTriggerType(triggerType);
        cameraTrigger.SetCameraController(cameraController);
        
        // Добавляем визуальный индикатор (опционально)
        CreateTriggerVisual(triggerObject, size);
        
        Debug.Log($"[CameraTriggerSetup] Создан триггер {triggerName} в позиции {offset}");
    }
    
    private void CreateTriggerVisual(GameObject triggerObject, Vector3 size)
    {
        // Создаем простой визуальный индикатор для отладки
        GameObject visual = new GameObject("TriggerVisual");
        visual.transform.SetParent(triggerObject.transform);
        visual.transform.localPosition = Vector3.zero;
        
        SpriteRenderer spriteRenderer = visual.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateDebugSprite(size);
        spriteRenderer.color = new Color(1f, 0f, 0f, 0.3f); // Полупрозрачный красный
        spriteRenderer.sortingOrder = -1; // Позади других объектов
    }
    
    private Sprite CreateDebugSprite(Vector3 size)
    {
        // Создаем простой спрайт для визуализации триггера
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        return sprite;
    }
    
    /// <summary>
    /// Удаляет все созданные триггеры
    /// </summary>
    [ContextMenu("Remove Camera Triggers")]
    public void RemoveCameraTriggers()
    {
        CameraTrigger[] triggers = FindObjectsOfType<CameraTrigger>();
        foreach (CameraTrigger trigger in triggers)
        {
            if (trigger.gameObject.name.Contains("DoorTrigger"))
            {
                DestroyImmediate(trigger.gameObject);
            }
        }
        
        Debug.Log("[CameraTriggerSetup] Все триггеры камеры удалены!");
    }
}

