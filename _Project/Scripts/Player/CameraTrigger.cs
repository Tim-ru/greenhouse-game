using UnityEngine;

/// <summary>
/// Триггер для перемещения камеры при входе игрока в зону
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CameraTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private CameraTriggerType triggerType = CameraTriggerType.LeftDoor;
    [SerializeField] private bool requirePlayerTag = true;
    [SerializeField] private string playerTag = "Player";
    
    [Header("Camera Controller")]
    [SerializeField] private CameraController cameraController;
    
    private void Start()
    {
        // Автоматически находим CameraController если не назначен
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CameraController>();
            if (cameraController == null)
            {
                Debug.LogError($"[CameraTrigger] CameraController не найден на объекте {gameObject.name}");
            }
        }
        
        // Убеждаемся, что коллайдер настроен как триггер
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        else
        {
            Debug.LogError($"[CameraTrigger] Collider2D не найден на объекте {gameObject.name}");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, что это игрок
        if (requirePlayerTag && !other.CompareTag(playerTag))
            return;
        
        // Перемещаем камеру в зависимости от типа триггера
        if (cameraController != null)
        {
            switch (triggerType)
            {
                case CameraTriggerType.LeftDoor:
                    cameraController.MoveToLeftDoor();
                    Debug.Log($"[CameraTrigger] Игрок вошел в зону левой двери");
                    break;
                    
                case CameraTriggerType.RightDoor:
                    cameraController.MoveToRightDoor();
                    Debug.Log($"[CameraTrigger] Игрок вошел в зону правой двери");
                    break;
                    
                case CameraTriggerType.Center:
                    cameraController.MoveToCenter();
                    Debug.Log($"[CameraTrigger] Игрок вошел в центральную зону");
                    break;
            }
        }
        else
        {
            Debug.LogError($"[CameraTrigger] CameraController не назначен на триггере {gameObject.name}");
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        // Опционально: можно добавить логику при выходе из зоны
        // Например, возврат камеры в центр
        if (requirePlayerTag && !other.CompareTag(playerTag))
            return;
        
        // Раскомментируйте, если хотите возвращать камеру в центр при выходе
        // if (cameraController != null)
        // {
        //     cameraController.MoveToCenter();
        // }
    }
    
    /// <summary>
    /// Устанавливает тип триггера
    /// </summary>
    public void SetTriggerType(CameraTriggerType type)
    {
        triggerType = type;
    }
    
    /// <summary>
    /// Устанавливает контроллер камеры
    /// </summary>
    public void SetCameraController(CameraController controller)
    {
        cameraController = controller;
    }
}

/// <summary>
/// Типы триггеров для камеры
/// </summary>
public enum CameraTriggerType
{
    LeftDoor,   // Левая дверь
    RightDoor,  // Правая дверь
    Center      // Центральная позиция
}
