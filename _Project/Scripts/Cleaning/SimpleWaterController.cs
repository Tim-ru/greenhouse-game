using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Упрощенный контроллер воды для стирания плесени под курсором.
/// Создает эффекты голубого следа-кисти и разлетающихся партиклов.
/// </summary>
public class SimpleWaterController : MonoBehaviour
{
    [Header("Cleaning Settings")]
    [SerializeField] private float cleanRadius = 0.5f;
    [SerializeField] private float cleanStrength = 1f;
    [SerializeField] private float cleanInterval = 0.05f;
    [SerializeField] private float wateringRange = 3f; // Максимальная дальность полива
    
    [Header("Effects")]
    [SerializeField] private WaterBrushTrail brushTrail;
    [SerializeField] private WaterParticleEffect particleEffect;
    
    [Header("Input")]
    [SerializeField] private bool usePlayerInput = true;
    
    private Camera mainCamera;
    private float lastCleanTime;
    private bool isCleaning = false;
    
    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        // Автоматически создаем эффекты, если они не назначены
        if (brushTrail == null)
        {
            CreateBrushTrail();
        }
        
        if (particleEffect == null)
        {
            CreateParticleEffect();
        }
    }
    
    private void CreateBrushTrail()
    {
        GameObject brushTrailObj = new GameObject("WaterBrushTrail");
        brushTrailObj.transform.SetParent(transform);
        brushTrail = brushTrailObj.AddComponent<WaterBrushTrail>();
    }
    
    private void CreateParticleEffect()
    {
        GameObject particleEffectObj = new GameObject("WaterParticleEffect");
        particleEffectObj.transform.SetParent(transform);
        particleEffect = particleEffectObj.AddComponent<WaterParticleEffect>();
    }
    
    private void Update()
    {
        HandleInput();
        
        if (isCleaning)
        {
            CleanMoldAtCursor();
        }
    }
    
    private void HandleInput()
    {
        bool fireInput = false;
        
        if (usePlayerInput)
        {
            // Используем Input System для получения состояния мыши
            var mouse = Mouse.current;
            if (mouse != null)
            {
                fireInput = mouse.leftButton.isPressed;
            }
        }
        
        isCleaning = fireInput;
        
        // Очищаем следы при остановке
        if (!isCleaning && brushTrail != null)
        {
            brushTrail.ClearTrails();
        }
    }
    
    private void CleanMoldAtCursor()
    {
        if (Time.time - lastCleanTime < cleanInterval) return;
        
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        mouseWorldPos.z = 0f; // Для 2D
        
        // Проверяем, находится ли курсор в пределах радиуса полива
        float distanceToCursor = Vector3.Distance(transform.position, mouseWorldPos);
        if (distanceToCursor > wateringRange)
        {
            // Если курсор слишком далеко, ограничиваем позицию радиусом полива
            Vector3 direction = (mouseWorldPos - transform.position).normalized;
            mouseWorldPos = transform.position + direction * wateringRange;
        }
        
        // Добавляем след кисти
        if (brushTrail != null)
        {
            brushTrail.AddBrushTrail(mouseWorldPos);
        }
        
        // Ищем плесень под курсором
        MoldSurface moldSurface = FindMoldUnderCursor(mouseWorldPos);
        if (moldSurface != null)
        {
            // Проверяем, не скрыта ли уже плесень
            if (!moldSurface.IsHidden())
            {
                // Запоминаем процент очистки до стирания
                float cleanPercentBefore = moldSurface.GetCleanPercent();
                
                // Стираем плесень
                moldSurface.EraseAtWorldPoint(mouseWorldPos);
                
                // Проверяем процент очистки после стирания
                float cleanPercentAfter = moldSurface.GetCleanPercent();
                
                // Создаем эффект партиклов только если плесень действительно была очищена
                if (cleanPercentAfter > cleanPercentBefore)
                {
                    if (particleEffect != null)
                    {
                        Vector3 direction = (mouseWorldPos - transform.position).normalized;
                        particleEffect.CreateParticlesAt(mouseWorldPos, direction);
                    }
                }
                
                lastCleanTime = Time.time;
            }
        }
    }
    
    private MoldSurface FindMoldUnderCursor(Vector3 worldPosition)
    {
        // Ищем все объекты плесени в сцене
        MoldSurface[] allMoldSurfaces = FindObjectsOfType<MoldSurface>();
        
        foreach (MoldSurface moldSurface in allMoldSurfaces)
        {
            // Проверяем, находится ли курсор в пределах плесени
            if (IsPointInsideMold(moldSurface, worldPosition))
            {
                return moldSurface;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Проверяет, находится ли точка внутри плесени
    /// </summary>
    private bool IsPointInsideMold(MoldSurface moldSurface, Vector3 worldPoint)
    {
        SpriteRenderer spriteRenderer = moldSurface.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null) return false;
        
        // Получаем границы спрайта
        Bounds spriteBounds = spriteRenderer.bounds;
        
        // Проверяем, находится ли точка внутри границ
        return spriteBounds.Contains(worldPoint);
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
    /// Принудительно активирует очистку (для использования из других скриптов)
    /// </summary>
    public void ForceClean()
    {
        isCleaning = true;
        CleanMoldAtCursor();
    }
    
    private void OnDrawGizmosSelected()
    {
        // Рисуем радиус очистки
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, cleanRadius);
        
        // Рисуем радиус полива
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, wateringRange);
    }
}
