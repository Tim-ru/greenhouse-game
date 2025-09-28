using UnityEngine;

/// <summary>
/// Визуальные эффекты струи воды.
/// Отвечает только за отображение LineRenderer без логики стирания.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class JetVFX : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Transform nozzle;
    [SerializeField] private float wobble = 0.05f;
    [SerializeField] private Gradient color = new Gradient();
    [SerializeField] private float width = 0.1f;
    [SerializeField] private int segments = 10;
    
    private LineRenderer lineRenderer;
    private bool isVisible = false;
    
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetupLineRenderer();
    }
    
    private void SetupLineRenderer()
    {
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.colorGradient = color;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width * 0.5f;
        lineRenderer.positionCount = segments;
        lineRenderer.useWorldSpace = true;
        lineRenderer.sortingOrder = 10; // Поверх других объектов
        lineRenderer.enabled = false;
    }
    
    /// <summary>
    /// Рисует струю до указанной точки
    /// </summary>
    /// <param name="end">Конечная точка струи</param>
    public void DrawTo(Vector3 end)
    {
        if (lineRenderer == null) return;
        
        Vector3 start = nozzle != null ? nozzle.position : transform.position;
        
        // Создаем точки с легким отклонением для реалистичности
        Vector3[] points = new Vector3[segments];
        
        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / (segments - 1);
            Vector3 basePoint = Vector3.Lerp(start, end, t);
            
            // Добавляем шумовое отклонение
            if (t > 0.1f && t < 0.9f) // Не отклоняем начало и конец
            {
                float noise = Mathf.PerlinNoise(Time.time * 10f + i, 0f) * 2f - 1f;
                Vector3 perpendicular = Vector3.Cross(Vector3.forward, (end - start).normalized);
                basePoint += perpendicular * noise * wobble;
            }
            
            points[i] = basePoint;
        }
        
        lineRenderer.SetPositions(points);
        lineRenderer.enabled = true;
        isVisible = true;
    }
    
    /// <summary>
    /// Скрывает струю
    /// </summary>
    public void Hide()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
        isVisible = false;
    }
    
    /// <summary>
    /// Проверяет, видна ли струя
    /// </summary>
    public bool IsVisible => isVisible;
    
    private void Update()
    {
        // Обновляем цвет градиента в реальном времени
        if (isVisible && lineRenderer != null)
        {
            lineRenderer.colorGradient = color;
        }
    }
    
    private void OnValidate()
    {
        if (Application.isPlaying && lineRenderer != null)
        {
            SetupLineRenderer();
        }
    }
}
