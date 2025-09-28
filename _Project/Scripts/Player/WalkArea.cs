using UnityEngine;

/// <summary>
/// Компонент для определения области ходьбы персонажа в bitmap-стиле
/// Создает невидимую область с коллайдером для ограничения движения
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class WalkArea : MonoBehaviour
{
    [Header("Walk Area Settings")]
    [SerializeField] private float walkWidth = 10f;    // Ширина области ходьбы
    [SerializeField] private float walkHeight = 1.5f;  // Высота области ходьбы
    [SerializeField] private bool isTrigger = true;    // Является ли коллайдер триггером
    
    private BoxCollider2D boxCollider;

    private void Awake()
    {
        SetupWalkArea();
    }

    private void SetupWalkArea()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        
        // Настраиваем коллайдер
        boxCollider.isTrigger = isTrigger;
        boxCollider.size = new Vector2(walkWidth, walkHeight);
        
        // Позиционируем коллайдер в центре объекта
        boxCollider.offset = Vector2.zero;
    }

    /// <summary>
    /// Получает границы области ходьбы в мировых координатах
    /// </summary>
    /// <returns>Минимальная и максимальная Y-координаты</returns>
    public Vector2 GetWalkBounds()
    {
        float minY = transform.position.y - walkHeight * 0.5f;
        float maxY = transform.position.y + walkHeight * 0.5f;
        
        return new Vector2(minY, maxY);
    }

    /// <summary>
    /// Получает границы области ходьбы в мировых координатах по X
    /// </summary>
    /// <returns>Минимальная и максимальная X-координаты</returns>
    public Vector2 GetWalkBoundsX()
    {
        float minX = transform.position.x - walkWidth * 0.5f;
        float maxX = transform.position.x + walkWidth * 0.5f;
        
        return new Vector2(minX, maxX);
    }

    /// <summary>
    /// Устанавливает размеры области ходьбы
    /// </summary>
    /// <param name="width">Ширина области</param>
    /// <param name="height">Высота области</param>
    public void SetWalkAreaSize(float width, float height)
    {
        walkWidth = width;
        walkHeight = height;
        
        if (boxCollider != null)
        {
            boxCollider.size = new Vector2(walkWidth, walkHeight);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Визуализация области ходьбы в Scene View
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(walkWidth, walkHeight, 0.1f));
        
        // Показываем границы
        Gizmos.color = Color.yellow;
        Vector2 bounds = GetWalkBounds();
        Vector3 minPoint = new Vector3(transform.position.x - walkWidth * 0.5f, bounds.x, 0);
        Vector3 maxPoint = new Vector3(transform.position.x + walkWidth * 0.5f, bounds.y, 0);
        
        Gizmos.DrawLine(minPoint, maxPoint);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Можно добавить логику при входе в область ходьбы
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered walk area");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Можно добавить логику при выходе из области ходьбы
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited walk area");
        }
    }
}
