using UnityEngine;

/// <summary>
/// Компонент для автоматической сортировки спрайтов по Y-координате (глубине)
/// Принцип: чем ниже объект на экране, тем выше его sortingOrder (ближе к камере)
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class YSorter : MonoBehaviour
{
    [Header("Sorting Settings")]
    [SerializeField] private int offset = 0;      // Базовое смещение порядка сортировки
    [SerializeField] private int multiplier = 100; // Множитель для точности сортировки
    
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        // Сортируем по Y-координате: чем ниже объект, тем выше sortingOrder
        // Отрицательный Y означает "выше на экране", поэтому умножаем на -1
        spriteRenderer.sortingOrder = offset + (int)(-transform.position.y * multiplier);
    }

    /// <summary>
    /// Устанавливает смещение для сортировки
    /// </summary>
    /// <param name="newOffset">Новое значение смещения</param>
    public void SetOffset(int newOffset)
    {
        offset = newOffset;
    }

    /// <summary>
    /// Устанавливает множитель для точности сортировки
    /// </summary>
    /// <param name="newMultiplier">Новый множитель</param>
    public void SetMultiplier(int newMultiplier)
    {
        multiplier = newMultiplier;
    }

    /// <summary>
    /// Получает текущее значение sortingOrder
    /// </summary>
    public int GetCurrentSortingOrder()
    {
        return offset + (int)(-transform.position.y * multiplier);
    }
}
