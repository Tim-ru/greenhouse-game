using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Контроллер струи воды для очистки плесени.
/// Обрабатывает ввод и выполняет raycast для определения целей стирания.
/// </summary>
public class WaterJetController : MonoBehaviour
{
    [Header("Jet Settings")]
    [SerializeField] private Transform nozzle;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private LayerMask moldMask = -1;
    [SerializeField] private float paintInterval = 0.01f;
    
    [Header("Visual Effects")]
    [SerializeField] private JetVFX vfx;
    [SerializeField] private WaterTrailSystem trailSystem;
    [SerializeField] private WaterDropletsEffect dropletsEffect;
    
    [Header("Input")]
    [SerializeField] private bool usePlayerInput = true;
    [SerializeField] private InputActionReference fireAction;
    
    // Примечание: Для полной поддержки PlayerInputActions добавьте действие "Fire" 
    // в Input Action Map "Gameplay" и привяжите его к левой кнопке мыши
    
    // Внутренние переменные
    private Camera mainCamera;
    private PlayerInputActions playerInputActions;
    private float lastPaintTime;
    private bool isFiring = false;
    
    // Публичные свойства
    public Vector2 AimDirection { get; private set; }
    
    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        // Инициализация системы ввода
        if (usePlayerInput)
        {
            playerInputActions = new PlayerInputActions();
        }
    }
    
    private void OnEnable()
    {
        if (usePlayerInput && playerInputActions != null)
        {
            playerInputActions.Enable();
        }
    }
    
    private void OnDisable()
    {
        if (usePlayerInput && playerInputActions != null)
        {
            playerInputActions.Disable();
        }
        
        if (vfx != null)
        {
            vfx.Hide();
        }
    }
    
    private void Update()
    {
        HandleInput();
        UpdateAimDirection();
        PerformRaycast();
    }
    
    private void HandleInput()
    {
        bool fireInput = false;
        
        if (usePlayerInput && playerInputActions != null)
        {
            // Используем Input System для получения состояния мыши
            fireInput = GetMouseButtonInput();
        }
        else if (fireAction != null)
        {
            fireInput = fireAction.action.IsPressed();
        }
        else
        {
            // Fallback на Input System
            fireInput = GetMouseButtonInput();
        }
        
        isFiring = fireInput;
        
        if (!isFiring)
        {
            if (vfx != null)
            {
                vfx.Hide();
            }
            
            // Очищаем следы при остановке стрельбы
            if (trailSystem != null)
            {
                trailSystem.ClearTrails();
            }
        }
    }
    
    private void UpdateAimDirection()
    {
        if (mainCamera == null) return;
        
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        mouseWorldPos.z = 0f; // Для 2D
        
        Vector3 nozzlePos = nozzle != null ? nozzle.position : transform.position;
        AimDirection = (mouseWorldPos - nozzlePos).normalized;
    }
    
    private bool GetMouseButtonInput()
    {
        // Используем Input System для получения состояния мыши
        var mouse = Mouse.current;
        if (mouse != null)
        {
            return mouse.leftButton.isPressed;
        }
        
        // Fallback - возвращаем false
        return false;
    }
    
    private Vector3 GetMouseWorldPosition()
    {
        // Используем Input System для получения позиции мыши
        var mouse = Mouse.current;
        if (mouse != null)
        {
            return mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());
        }
        
        // Fallback - возвращаем позицию игрока
        return transform.position;
    }
    
    /// <summary>
    /// Принудительно активирует струю воды (для использования из HoseEquipment)
    /// </summary>
    public void ForceFire()
    {
        isFiring = true;
        PerformRaycast();
    }
    
    public void PerformRaycast()
    {
        if (!isFiring || mainCamera == null) return;
        
        Vector3 nozzlePos = nozzle != null ? nozzle.position : transform.position;
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        mouseWorldPos.z = 0f; // Для 2D
        
        Vector3 direction = (mouseWorldPos - nozzlePos).normalized;
        float distance = Mathf.Min(Vector3.Distance(nozzlePos, mouseWorldPos), maxDistance);
        
        // Выполняем raycast
        RaycastHit2D hit = Physics2D.Raycast(nozzlePos, direction, distance, moldMask);
        
        if (hit.collider != null)
        {
            // Добавляем след воды в точку попадания
            if (trailSystem != null)
            {
                trailSystem.AddTrailAt(hit.point);
            }
            
            // Проверяем, есть ли компонент MoldSurface
            MoldSurface moldSurface = hit.collider.GetComponent<MoldSurface>();
            if (moldSurface != null)
            {
                // Проверяем интервал стирания
                if (Time.time - lastPaintTime >= paintInterval)
                {
                    moldSurface.EraseAtWorldPoint(hit.point);
                    lastPaintTime = Time.time;
                    
                    // Создаем эффект разлетающихся капель
                    if (dropletsEffect != null)
                    {
                        dropletsEffect.CreateDropletsAt(hit.point, direction);
                    }
                }
                
                // Показываем визуальные эффекты
                if (vfx != null)
                {
                    vfx.DrawTo(hit.point);
                }
            }
            else
            {
                // Попали в коллайдер, но не в MoldSurface
                if (vfx != null)
                {
                    vfx.DrawTo(hit.point);
                }
            }
        }
        else
        {
            // Не попали никуда - показываем струю до максимальной дистанции
            Vector3 endPoint = nozzlePos + direction * distance;
            
            // Добавляем след воды в конечную точку
            if (trailSystem != null)
            {
                trailSystem.AddTrailAt(endPoint);
            }
            
            if (vfx != null)
            {
                vfx.DrawTo(endPoint);
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (nozzle == null) return;
        
        // Рисуем направление прицеливания
        Gizmos.color = Color.blue;
        Vector3 nozzlePos = nozzle.position;
        Vector3 mouseWorldPos = Camera.main != null ? GetMouseWorldPosition() : nozzlePos;
        mouseWorldPos.z = 0f;
        
        Vector3 direction = (mouseWorldPos - nozzlePos).normalized;
        float distance = Mathf.Min(Vector3.Distance(nozzlePos, mouseWorldPos), maxDistance);
        
        Gizmos.DrawLine(nozzlePos, nozzlePos + direction * distance);
        
        // Рисуем максимальную дистанцию
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(nozzlePos, maxDistance);
    }
}
