using UnityEngine;

/// <summary>
/// Компонент для движения персонажа с ограничением по Y-координате в стиле bitmap
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BeltMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 4f;
    
    [Header("Walk Area Constraints")]
    [SerializeField] private Vector2 walkYRange = new Vector2(-1.5f, -0.2f);
    [SerializeField] private Vector2 walkXRange = new Vector2(-4f, 4f);
    
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
    /// <param name="move">Вектор движения от Input System</param>
    public void SetInput(Vector2 move)
    {
        // Нормализуем если длина больше 1 (диагональное движение)
        input = move.sqrMagnitude > 1 ? move.normalized : move;
    }

    private void FixedUpdate()
    {
        // Вычисляем новую позицию
        Vector2 newPosition = rb.position + input * speed * Time.fixedDeltaTime;
        
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
    }
}
