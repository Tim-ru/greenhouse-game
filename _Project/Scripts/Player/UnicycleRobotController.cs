using UnityEngine;

/// <summary>
/// Продвинутый контроллер для одноколесного робота с балансировкой и реалистичной физикой
/// </summary>
[RequireComponent(typeof(BeltMover))]
public class UnicycleRobotController : MonoBehaviour
{
    [Header("Balance Settings")]
    [SerializeField] private float balanceForce = 10f;
    [SerializeField] private float balanceDamping = 5f;
    [SerializeField] private float maxTiltAngle = 30f;
    
    [Header("Wheel Physics")]
    [SerializeField] private float wheelRadius = 0.5f;
    [SerializeField] private float wheelInertia = 0.5f;
    
    [Header("Visual Settings")]
    [SerializeField] private Transform wheelVisual;
    [SerializeField] private Transform bodyVisual;
    [SerializeField] private float wheelRotationSpeed = 1f;
    
    private BeltMover beltMover;
    private Rigidbody2D rb;
    private float currentTilt;
    private float wheelAngularVelocity;
    private Vector2 lastPosition;
    
    // События для анимации и звуков
    public System.Action<float> OnWheelSpin;
    public System.Action<float> OnTiltChanged;
    public System.Action OnBalanceLost;

    private void Awake()
    {
        beltMover = GetComponent<BeltMover>();
        rb = GetComponent<Rigidbody2D>();
        lastPosition = transform.position;
    }

    private void Start()
    {
        // Настраиваем параметры движения для одноколесного робота
        beltMover.SetMovementSettings(
            maxSpeed: 4f,
            acceleration: 6f,
            deceleration: 4f
        );
        
        beltMover.SetFrictionSettings(
            friction: 0.92f,
            airResistance: 0.98f
        );
    }

    private void FixedUpdate()
    {
        UpdateBalance();
        UpdateWheelPhysics();
        UpdateVisuals();
    }

    /// <summary>
    /// Обновляет балансировку робота
    /// </summary>
    private void UpdateBalance()
    {
        Vector2 velocity = beltMover.GetCurrentVelocity();
        float targetTilt = 0f;
        
        // Вычисляем желаемый наклон на основе скорости
        if (velocity.magnitude > 0.1f)
        {
            targetTilt = Mathf.Clamp(velocity.x * 5f, -maxTiltAngle, maxTiltAngle);
        }
        
        // Плавно изменяем наклон
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, balanceForce * Time.fixedDeltaTime);
        
        // Применяем балансировочную силу
        float balanceError = targetTilt - currentTilt;
        float balanceTorque = balanceError * balanceForce - rb.angularVelocity * balanceDamping;
        
        rb.AddTorque(balanceTorque);
        
        // Проверяем потерю баланса
        if (Mathf.Abs(currentTilt) > maxTiltAngle * 1.5f)
        {
            OnBalanceLost?.Invoke();
        }
        
        // Уведомляем о изменении наклона
        OnTiltChanged?.Invoke(currentTilt);
    }

    /// <summary>
    /// Обновляет физику колеса
    /// </summary>
    private void UpdateWheelPhysics()
    {
        Vector2 currentPosition = transform.position;
        Vector2 movement = currentPosition - lastPosition;
        
        // Вычисляем угловую скорость колеса
        float distance = movement.magnitude;
        wheelAngularVelocity = distance / wheelRadius;
        
        // Обновляем позицию для следующего кадра
        lastPosition = currentPosition;
        
        // Уведомляем о вращении колеса
        OnWheelSpin?.Invoke(wheelAngularVelocity);
    }

    /// <summary>
    /// Обновляет визуальные элементы
    /// </summary>
    private void UpdateVisuals()
    {
        // Вращение колеса
        if (wheelVisual != null)
        {
            float rotation = wheelAngularVelocity * wheelRotationSpeed * Time.fixedDeltaTime;
            wheelVisual.Rotate(0, 0, -rotation);
        }
        
        // Наклон тела
        if (bodyVisual != null)
        {
            bodyVisual.rotation = Quaternion.Euler(0, 0, currentTilt);
        }
    }

    /// <summary>
    /// Применяет толчок к роботу (например, от взрыва)
    /// </summary>
    /// <param name="force">Сила толчка</param>
    /// <param name="direction">Направление толчка</param>
    public void ApplyPush(Vector2 force, Vector2 direction)
    {
        beltMover.ApplyImpulse(force);
        
        // Добавляем вращательный импульс
        float torque = Vector2.Dot(force, new Vector2(-direction.y, direction.x)) * 0.1f;
        rb.AddTorque(torque);
    }

    /// <summary>
    /// Получает текущий наклон робота
    /// </summary>
    public float GetCurrentTilt() => currentTilt;

    /// <summary>
    /// Получает угловую скорость колеса
    /// </summary>
    public float GetWheelAngularVelocity() => wheelAngularVelocity;

    /// <summary>
    /// Проверяет, потерял ли робот баланс
    /// </summary>
    public bool IsBalanced() => Mathf.Abs(currentTilt) < maxTiltAngle;

    private void OnDrawGizmosSelected()
    {
        // Визуализация наклона
        Gizmos.color = Color.yellow;
        Vector3 tiltDirection = new Vector3(Mathf.Sin(currentTilt * Mathf.Deg2Rad), 
                                          Mathf.Cos(currentTilt * Mathf.Deg2Rad), 0);
        Gizmos.DrawLine(transform.position, transform.position + tiltDirection * 2f);
        
        // Визуализация колеса
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, wheelRadius);
    }
}
