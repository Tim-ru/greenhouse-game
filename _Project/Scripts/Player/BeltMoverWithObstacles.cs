using UnityEngine;

/// <summary>
/// Улучшенная версия BeltMover с поддержкой препятствий
/// Автоматически блокирует движение при приближении к интерактивным объектам
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BeltMoverWithObstacles : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 4f;
    
    [Header("Walk Area Constraints")]
    [SerializeField] private Vector2 walkYRange = new Vector2(-1.5f, -0.2f);
    [SerializeField] private Vector2 walkXRange = new Vector2(-4f, 4f);
    
    [Header("Obstacle Detection")]
    [SerializeField] private float obstacleCheckDistance = 0.5f;
    [SerializeField] private LayerMask obstacleLayerMask = -1;
    
    private Rigidbody2D rb;
    private Vector2 input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    /// <summary>
    /// Устанавливает входной вектор движения
    /// </summary>
    public void SetInput(Vector2 move)
    {
        input = move.sqrMagnitude > 1 ? move.normalized : move;
    }

    private void FixedUpdate()
    {
        Vector2 newPosition = rb.position + input * speed * Time.fixedDeltaTime;
        
        // Ограничиваем Y-координату
        newPosition.y = Mathf.Clamp(newPosition.y, walkYRange.x, walkYRange.y);
        
        // Проверяем препятствия по X
        newPosition.x = CheckObstacleCollision(newPosition.x, newPosition.y);
        
        // Ограничиваем X-координату границами области
        newPosition.x = Mathf.Clamp(newPosition.x, walkXRange.x, walkXRange.y);
        
        rb.MovePosition(newPosition);
    }

    /// <summary>
    /// Проверяет столкновение с препятствиями и корректирует позицию
    /// </summary>
    private float CheckObstacleCollision(float targetX, float currentY)
    {
        // Проверяем движение влево
        if (input.x < 0)
        {
            Vector2 leftCheck = new Vector2(targetX - obstacleCheckDistance, currentY);
            if (Physics2D.OverlapPoint(leftCheck, obstacleLayerMask))
            {
                // Найдено препятствие слева, останавливаем движение
                return rb.position.x;
            }
        }
        
        // Проверяем движение вправо
        if (input.x > 0)
        {
            Vector2 rightCheck = new Vector2(targetX + obstacleCheckDistance, currentY);
            if (Physics2D.OverlapPoint(rightCheck, obstacleLayerMask))
            {
                // Найдено препятствие справа, останавливаем движение
                return rb.position.x;
            }
        }
        
        return targetX;
    }

    /// <summary>
    /// Проверяет, есть ли препятствие в указанной позиции
    /// </summary>
    public bool HasObstacleAt(Vector2 position)
    {
        return Physics2D.OverlapPoint(position, obstacleLayerMask) != null;
    }

    /// <summary>
    /// Получает ближайшую свободную позицию в указанном направлении
    /// </summary>
    public Vector2 GetNearestFreePosition(Vector2 direction, float maxDistance = 1f)
    {
        Vector2 currentPos = rb.position;
        Vector2 step = direction.normalized * 0.1f;
        
        for (float distance = 0; distance <= maxDistance; distance += 0.1f)
        {
            Vector2 testPos = currentPos + direction.normalized * distance;
            testPos.y = Mathf.Clamp(testPos.y, walkYRange.x, walkYRange.y);
            testPos.x = Mathf.Clamp(testPos.x, walkXRange.x, walkXRange.y);
            
            if (!HasObstacleAt(testPos))
            {
                return testPos;
            }
        }
        
        return currentPos; // Возвращаем текущую позицию, если свободное место не найдено
    }

    // Методы для настройки границ (аналогично BeltMover)
    public void SetWalkAreaBounds(float minX, float maxX, float minY, float maxY)
    {
        walkXRange = new Vector2(minX, maxX);
        walkYRange = new Vector2(minY, maxY);
    }

    public void SetWalkAreaBounds(float minY, float maxY)
    {
        walkYRange = new Vector2(minY, maxY);
    }

    public Vector2 GetWalkAreaBounds() => walkYRange;
    public Vector2 GetWalkAreaBoundsX() => walkXRange;

    private void OnDrawGizmosSelected()
    {
        // Визуализация границ области ходьбы
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
        
        // Визуализация зоны проверки препятствий
        Gizmos.color = Color.red;
        Vector3 obstacleCheckSize = new Vector3(obstacleCheckDistance * 2, 0.2f, 0.1f);
        Gizmos.DrawWireCube(transform.position, obstacleCheckSize);
    }
}
