using UnityEngine;

/// <summary>
/// Утилитарный скрипт для автоматической настройки области ходьбы
/// Подключает WalkArea к BeltMover для автоматической синхронизации границ
/// </summary>
public class WalkAreaSetup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject walkAreaObject;
    [SerializeField] private BeltMover beltMover;
    
    // Автоматически получаемый компонент
    private WalkArea walkArea;
    
    [Header("Auto Setup")]
    [SerializeField] private bool autoFindComponents = true;
    [SerializeField] private bool syncOnStart = true;

    private void Awake()
    {
        if (autoFindComponents)
        {
            SetupComponents();
        }
    }

    private void Start()
    {
        if (syncOnStart)
        {
            SyncWalkAreaBounds();
        }
    }

    /// <summary>
    /// Автоматически находит и настраивает компоненты
    /// </summary>
    private void SetupComponents()
    {
        // Если задан GameObject, получаем из него компонент WalkArea
        if (walkAreaObject != null)
        {
            walkArea = walkAreaObject.GetComponent<WalkArea>();
            if (walkArea == null)
            {
                Debug.LogError($"WalkAreaSetup: На объекте {walkAreaObject.name} не найден компонент WalkArea!");
            }
        }
        else
        {
            // Ищем WalkArea в дочерних объектах или на этом же объекте
            walkArea = GetComponentInChildren<WalkArea>();
        }
        
        // Ищем BeltMover на этом же объекте
        if (beltMover == null)
        {
            beltMover = GetComponent<BeltMover>();
        }
    }

    /// <summary>
    /// Синхронизирует границы области ходьбы с BeltMover
    /// </summary>
    public void SyncWalkAreaBounds()
    {
        if (walkArea == null || beltMover == null)
        {
            Debug.LogWarning("WalkAreaSetup: Не найдены необходимые компоненты для синхронизации");
            return;
        }

        Vector2 boundsY = walkArea.GetWalkBounds();
        Vector2 boundsX = walkArea.GetWalkBoundsX();
        
        // Синхронизируем границы по обеим осям
        beltMover.SetWalkAreaBounds(boundsX.x, boundsX.y, boundsY.x, boundsY.y);
        
        Debug.Log($"WalkAreaSetup: Синхронизированы границы области ходьбы:");
        Debug.Log($"  X: от {boundsX.x:F2} до {boundsX.y:F2}");
        Debug.Log($"  Y: от {boundsY.x:F2} до {boundsY.y:F2}");
    }

    /// <summary>
    /// Создает область ходьбы программно
    /// </summary>
    /// <param name="center">Центр области</param>
    /// <param name="width">Ширина</param>
    /// <param name="height">Высота</param>
    /// <returns>Созданный компонент WalkArea</returns>
    public WalkArea CreateWalkArea(Vector3 center, float width, float height)
    {
        GameObject walkAreaObject = new GameObject("WalkArea");
        walkAreaObject.transform.SetParent(transform);
        walkAreaObject.transform.position = center;
        
        WalkArea newWalkArea = walkAreaObject.AddComponent<WalkArea>();
        newWalkArea.SetWalkAreaSize(width, height);
        
        walkArea = newWalkArea;
        
        if (syncOnStart)
        {
            SyncWalkAreaBounds();
        }
        
        return newWalkArea;
    }

    private void OnDrawGizmos()
    {
        if (walkArea != null && beltMover != null)
        {
            // Показываем связь между компонентами
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(walkArea.transform.position, beltMover.transform.position);
        }
    }
}
