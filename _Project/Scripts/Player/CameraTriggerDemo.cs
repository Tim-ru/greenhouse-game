using UnityEngine;

/// <summary>
/// Демонстрационный скрипт для тестирования системы триггеров камеры
/// </summary>
public class CameraTriggerDemo : MonoBehaviour
{
    [Header("Demo Settings")]
    [SerializeField] private bool enableKeyboardControls = true;
    [SerializeField] private KeyCode leftDoorKey = KeyCode.Q;
    [SerializeField] private KeyCode rightDoorKey = KeyCode.E;
    [SerializeField] private KeyCode centerKey = KeyCode.Space;
    
    private CameraController cameraController;
    
    private void Start()
    {
        // Находим контроллер камеры
        cameraController = FindObjectOfType<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("[CameraTriggerDemo] CameraController не найден в сцене!");
            enabled = false;
            return;
        }
        
        Debug.Log("[CameraTriggerDemo] Демо-скрипт активирован. Используйте клавиши для тестирования:");
        Debug.Log($"- {leftDoorKey} - перемещение к левой двери");
        Debug.Log($"- {rightDoorKey} - перемещение к правой двери");
        Debug.Log($"- {centerKey} - возврат в центр");
    }
    
    private void Update()
    {
        if (!enableKeyboardControls || cameraController == null) return;
        
        // Проверяем нажатия клавиш
        if (Input.GetKeyDown(leftDoorKey))
        {
            cameraController.MoveToLeftDoor();
        }
        else if (Input.GetKeyDown(rightDoorKey))
        {
            cameraController.MoveToRightDoor();
        }
        else if (Input.GetKeyDown(centerKey))
        {
            cameraController.MoveToCenter();
        }
    }
    
    /// <summary>
    /// Включает/выключает управление с клавиатуры
    /// </summary>
    public void SetKeyboardControls(bool enabled)
    {
        enableKeyboardControls = enabled;
        Debug.Log($"[CameraTriggerDemo] Управление с клавиатуры: {(enabled ? "включено" : "выключено")}");
    }
    
    /// <summary>
    /// Перемещает камеру к левой двери (для вызова из UI)
    /// </summary>
    public void MoveToLeftDoor()
    {
        if (cameraController != null)
        {
            cameraController.MoveToLeftDoor();
        }
    }
    
    /// <summary>
    /// Перемещает камеру к правой двери (для вызова из UI)
    /// </summary>
    public void MoveToRightDoor()
    {
        if (cameraController != null)
        {
            cameraController.MoveToRightDoor();
        }
    }
    
    /// <summary>
    /// Возвращает камеру в центр (для вызова из UI)
    /// </summary>
    public void MoveToCenter()
    {
        if (cameraController != null)
        {
            cameraController.MoveToCenter();
        }
    }
}
