using UnityEngine;

/// <summary>
/// Компонент для движения одноколесного робота с плавным замедлением и инерцией
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BeltMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 4f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float deceleration = 6f;
    
    [Header("Friction Settings")]
    [SerializeField] private float friction = 0.9f;
    [SerializeField] private float airResistance = 0.95f;
    
    [Header("Walk Area Constraints")]
    [SerializeField] private Vector2 walkYRange = new Vector2(-1.5f, -0.2f);
    [SerializeField] private Vector2 walkXRange = new Vector2(-4f, 4f);
    
    private Rigidbody2D rb;
    private Vector2 input;
    private Vector2 currentVelocity;
    private Vector2 targetVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        currentVelocity = Vector2.zero;
    }

    /// <summary>
    /// Устанавливает входной вектор движения
    /// </summary>
    /// <param name="move">Вектор движения от Input System</param>
    public void SetInput(Vector2 move)
    {
        // Нормализуем если длина больше 1 (диагональное движение)
        input = move.sqrMagnitude > 1 ? move.normalized : move;
    }

    private void FixedUpdate()
    {
        // Вычисляем целевую скорость на основе ввода
        targetVelocity = input * maxSpeed;
        
        // Плавно изменяем скорость с учетом ускорения/замедления
        if (input.magnitude > 0.1f)
        {
            // Ускорение при наличии ввода
            currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, 
                acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // Плавное замедление при отсутствии ввода
            currentVelocity = Vector2.MoveTowards(currentVelocity, Vector2.zero, 
                deceleration * Time.fixedDeltaTime);
        }
        
        // Применяем трение и сопротивление воздуха
        currentVelocity *= friction;
        currentVelocity *= airResistance;
        
        // Вычисляем новую позицию
        Vector2 newPosition = rb.position + currentVelocity * Time.fixedDeltaTime;
        
        // Ограничиваем Y-координату в пределах заданного диапазона
        newPosition.y = Mathf.Clamp(newPosition.y, walkYRange.x, walkYRange.y);
        
        // Ограничиваем X-координату в пределах заданного диапазона
        newPosition.x = Mathf.Clamp(newPosition.x, walkXRange.x, walkXRange.y);
        
        // Применяем движение
        rb.MovePosition(newPosition);
    }

    /// <summary>
    /// Устанавливает границы области ходьбы по Y
    /// </summary>
    /// <param name="minY">Минимальная Y-координата</param>
    /// <param name="maxY">Максимальная Y-координата</param>
    public void SetWalkAreaBounds(float minY, float maxY)
    {
        walkYRange = new Vector2(minY, maxY);
    }

    /// <summary>
    /// Устанавливает границы области ходьбы по X
    /// </summary>
    /// <param name="minX">Минимальная X-координата</param>
    /// <param name="maxX">Максимальная X-координата</param>
    public void SetWalkAreaBoundsX(float minX, float maxX)
    {
        walkXRange = new Vector2(minX, maxX);
    }

    /// <summary>
    /// Устанавливает границы области ходьбы по обеим осям
    /// </summary>
    /// <param name="minX">Минимальная X-координата</param>
    /// <param name="maxX">Максимальная X-координата</param>
    /// <param name="minY">Минимальная Y-координата</param>
    /// <param name="maxY">Максимальная Y-координата</param>
    public void SetWalkAreaBounds(float minX, float maxX, float minY, float maxY)
    {
        walkXRange = new Vector2(minX, maxX);
        walkYRange = new Vector2(minY, maxY);
    }

    /// <summary>
    /// Получает текущие границы области ходьбы по Y
    /// </summary>
    public Vector2 GetWalkAreaBounds() => walkYRange;

    /// <summary>
    /// Получает текущие границы области ходьбы по X
    /// </summary>
    public Vector2 GetWalkAreaBoundsX() => walkXRange;
    
    /// <summary>
    /// Получает текущую скорость движения
    /// </summary>
    public Vector2 GetCurrentVelocity() => currentVelocity;
    
    /// <summary>
    /// Получает текущую скорость движения (магнитуда)
    /// </summary>
    public float GetCurrentSpeed() => currentVelocity.magnitude;
    
    /// <summary>
    /// Устанавливает параметры движения
    /// </summary>
    /// <param name="maxSpeed">Максимальная скорость</param>
    /// <param name="acceleration">Ускорение</param>
    /// <param name="deceleration">Замедление</param>
    public void SetMovementSettings(float maxSpeed, float acceleration, float deceleration)
    {
        this.maxSpeed = maxSpeed;
        this.acceleration = acceleration;
        this.deceleration = deceleration;
    }
    
    /// <summary>
    /// Устанавливает параметры трения
    /// </summary>
    /// <param name="friction">Трение (0-1, где 1 = нет трения)</param>
    /// <param name="airResistance">Сопротивление воздуха (0-1, где 1 = нет сопротивления)</param>
    public void SetFrictionSettings(float friction, float airResistance)
    {
        this.friction = Mathf.Clamp01(friction);
        this.airResistance = Mathf.Clamp01(airResistance);
    }
    
    /// <summary>
    /// Принудительно останавливает робота
    /// </summary>
    public void Stop()
    {
        currentVelocity = Vector2.zero;
    }
    
    /// <summary>
    /// Применяет импульс к роботу (например, от взрыва или толчка)
    /// </summary>
    /// <param name="impulse">Вектор импульса</param>
    public void ApplyImpulse(Vector2 impulse)
    {
        currentVelocity += impulse;
    }

    private void OnDrawGizmosSelected()
    {
        // Визуализация границ области ходьбы в Scene View
        Gizmos.color = Color.green;
        Vector3 center = new Vector3(
            (walkXRange.x + walkXRange.y) * 0.5f, 
            (walkYRange.x + walkYRange.y) * 0.5f, 
            0
        );
        Vector3 size = new Vector3(
            walkXRange.y - walkXRange.x, 
            walkYRange.y - walkYRange.x, 
            0.1f
        );
        Gizmos.DrawWireCube(center, size);
        
        // Визуализация текущей скорости
        if (Application.isPlaying && currentVelocity.magnitude > 0.1f)
        {
            Gizmos.color = Color.red;
            Vector3 velocityEnd = transform.position + (Vector3)currentVelocity * 0.5f;
            Gizmos.DrawLine(transform.position, velocityEnd);
            
            // Стрелка направления
            Vector3 direction = currentVelocity.normalized;
            Vector3 arrowHead = velocityEnd - direction * 0.2f;
            Vector3 arrowLeft = arrowHead + new Vector3(-direction.y, direction.x, 0) * 0.1f;
            Vector3 arrowRight = arrowHead + new Vector3(direction.y, -direction.x, 0) * 0.1f;
            
            Gizmos.DrawLine(velocityEnd, arrowLeft);
            Gizmos.DrawLine(velocityEnd, arrowRight);
        }
    }
}
