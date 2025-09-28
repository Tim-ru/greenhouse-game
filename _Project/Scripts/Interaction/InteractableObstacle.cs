using UnityEngine;

/// <summary>
/// Компонент для интерактивных объектов, которые должны быть препятствиями в bitmap-стиле
/// Использует триггерный коллайдер для взаимодействия, но блокирует движение через BeltMover
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class InteractableObstacle : InteractableBase
{
    [Header("Obstacle Settings")]
    [SerializeField] private bool isObstacle = true;
    [SerializeField] private Vector2 obstacleSize = Vector2.one;
    
    private BoxCollider2D boxCollider;

    private void Awake()
    {
        SetupObstacle();
    }

    private void SetupObstacle()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        
        // Настраиваем коллайдер как триггер для взаимодействия
        boxCollider.isTrigger = true;
        boxCollider.size = obstacleSize;
    }

    /// <summary>
    /// Получает границы препятствия в мировых координатах
    /// </summary>
    public Bounds GetObstacleBounds()
    {
        return boxCollider.bounds;
    }

    /// <summary>
    /// Проверяет, находится ли точка в пределах препятствия
    /// </summary>
    public bool IsPointInsideObstacle(Vector2 point)
    {
        return boxCollider.bounds.Contains(point);
    }

    /// <summary>
    /// Получает ближайшую точку на границе препятствия
    /// </summary>
    public Vector2 GetClosestPointOnBounds(Vector2 point)
    {
        return boxCollider.bounds.ClosestPoint(point);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Логика входа в зону взаимодействия
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Игрок вошел в зону взаимодействия с {gameObject.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Логика выхода из зоны взаимодействия
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Игрок вышел из зоны взаимодействия с {gameObject.name}");
        }
    }

    /// <summary>
    /// Реализация абстрактного метода из InteractableBase
    /// </summary>
    protected override void OnInteract(GameObject interactor)
    {
        Debug.Log($"Игрок {interactor.name} взаимодействует с препятствием {gameObject.name}!");
        
        // Здесь можно добавить логику взаимодействия
        // Например: открыть диалог, поднять предмет, активировать механизм и т.д.
        
        // Простой пример - изменить цвет объекта
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Random.ColorHSV();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Визуализация области препятствия
        Gizmos.color = isObstacle ? Color.red : Color.green;
        Gizmos.DrawWireCube(transform.position, obstacleSize);
        
        // Показываем, что это триггер
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, obstacleSize * 1.1f);
    }
}
