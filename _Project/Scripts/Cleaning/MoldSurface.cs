using UnityEngine;

/// <summary>
/// Компонент для редактируемой текстуры плесени поверх базового изображения.
/// Поддерживает стирание плесени в рантайме с помощью альфа-маски.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class MoldSurface : MonoBehaviour
{
    [Header("Mold Settings")]
    [SerializeField] private Sprite sourceMoldSprite;
    [SerializeField] private int brushRadiusPixels = 16;
    [SerializeField] private float eraseStrength01 = 1f;
    
    [Header("Debug")]
    [SerializeField] private float cleanThreshold = 0.1f; // Порог для определения "чистых" пикселей
    [SerializeField] private float disappearThreshold = 0.9f; // Порог для исчезновения плесени (90%)
    
    private SpriteRenderer spriteRenderer;
    private Texture2D runtimeTex;
    private Color32[] pixels;
    private bool isInitialized = false;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (sourceMoldSprite == null)
        {
            Debug.LogError($"[MoldSurface] Source mold sprite is null on {gameObject.name}. Disabling component.");
            enabled = false;
            return;
        }
        
        Debug.Log($"[MoldSurface] Awake called for {gameObject.name}, sourceMoldSprite: {sourceMoldSprite.name}");
        
        if (sourceMoldSprite.texture.isReadable == false)
        {
            Debug.LogError($"[MoldSurface] Source mold sprite texture is not readable on {gameObject.name}. " +
                          "Please enable 'Read/Write Enabled' in the sprite import settings. Disabling component.");
            enabled = false;
            return;
        }
        
        InitializeRuntimeTexture();
    }
    
    private void InitializeRuntimeTexture()
    {
        // Получаем размеры и область спрайта
        Rect spriteRect = sourceMoldSprite.textureRect;
        int width = Mathf.RoundToInt(spriteRect.width);
        int height = Mathf.RoundToInt(spriteRect.height);
        
        // Создаем новую текстуру
        runtimeTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        runtimeTex.filterMode = FilterMode.Bilinear;
        
        // Копируем пиксели из исходного спрайта
        Color[] originalPixels = sourceMoldSprite.texture.GetPixels(
            Mathf.RoundToInt(spriteRect.x),
            Mathf.RoundToInt(spriteRect.y),
            width,
            height
        );
        
        runtimeTex.SetPixels(originalPixels);
        runtimeTex.Apply();
        
        // Создаем новый спрайт из runtime текстуры
        Sprite newSprite = Sprite.Create(
            runtimeTex,
            new Rect(0, 0, width, height),
            sourceMoldSprite.pivot / sourceMoldSprite.pixelsPerUnit,
            sourceMoldSprite.pixelsPerUnit
        );
        
        spriteRenderer.sprite = newSprite;
        
        // Инициализируем буфер пикселей
        pixels = new Color32[width * height];
        
        isInitialized = true;
        Debug.Log($"[MoldSurface] Initialized runtime texture {width}x{height} for {gameObject.name}");
    }
    
    /// <summary>
    /// Стирает плесень в указанной точке мира
    /// </summary>
    /// <param name="worldPoint">Точка в мировых координатах</param>
    public void EraseAtWorldPoint(Vector3 worldPoint)
    {
        Debug.Log($"[MoldSurface] EraseAtWorldPoint called with point: {worldPoint}");
        
        if (!isInitialized) 
        {
            Debug.Log($"[MoldSurface] Not initialized, returning");
            return;
        }
        
        if (WorldToPixel(worldPoint, out int px, out int py))
        {
            Debug.Log($"[MoldSurface] WorldToPixel success: ({px}, {py}), calling EraseAtPixel");
            EraseAtPixel(px, py);
        }
        else
        {
            Debug.Log($"[MoldSurface] WorldToPixel failed for point: {worldPoint}");
        }
    }
    
    /// <summary>
    /// Стирает плесень в пиксельных координатах
    /// </summary>
    private void EraseAtPixel(int centerX, int centerY)
    {
        int width = runtimeTex.width;
        int height = runtimeTex.height;
        
        Debug.Log($"[MoldSurface] EraseAtPixel: center=({centerX}, {centerY}), brushRadius={brushRadiusPixels}, eraseStrength={eraseStrength01}");
        
        // Получаем текущие пиксели
        Color32[] currentPixels = runtimeTex.GetPixels32();
        
        int pixelsChanged = 0;
        
        // Применяем стирание в круглой области
        for (int y = centerY - brushRadiusPixels; y <= centerY + brushRadiusPixels; y++)
        {
            for (int x = centerX - brushRadiusPixels; x <= centerX + brushRadiusPixels; x++)
            {
                if (x < 0 || x >= width || y < 0 || y >= height) continue;
                
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                if (distance > brushRadiusPixels) continue;
                
                int index = y * width + x;
                Color32 pixel = currentPixels[index];
                
                // Уменьшаем альфу
                float falloff = 1f - (distance / brushRadiusPixels);
                float alphaReduction = eraseStrength01 * falloff;
                byte oldAlpha = pixel.a;
                pixel.a = (byte)Mathf.Max(0, pixel.a - (alphaReduction * 255));
                
                // Слегка осветляем для визуального эффекта "мокрой дорожки"
                pixel.r = (byte)Mathf.Min(255, pixel.r + 10);
                pixel.g = (byte)Mathf.Min(255, pixel.g + 10);
                pixel.b = (byte)Mathf.Min(255, pixel.b + 10);
                
                currentPixels[index] = pixel;
                
                if (pixel.a != oldAlpha)
                {
                    pixelsChanged++;
                }
            }
        }
        
        Debug.Log($"[MoldSurface] EraseAtPixel: changed {pixelsChanged} pixels");
        
        // Применяем изменения
        runtimeTex.SetPixels32(currentPixels);
        runtimeTex.Apply(false);
        
        Debug.Log($"[MoldSurface] EraseAtPixel: texture applied");
        
        // Проверяем, нужно ли скрыть плесень
        CheckAndHideIfClean();
    }
    
    /// <summary>
    /// Преобразует мировую точку в пиксельные координаты текстуры
    /// </summary>
    private bool WorldToPixel(Vector3 worldPoint, out int px, out int py)
    {
        px = 0;
        py = 0;
        
        Debug.Log($"[MoldSurface] WorldToPixel: worldPoint={worldPoint}, transform.position={transform.position}");
        
        if (!isInitialized) 
        {
            Debug.Log($"[MoldSurface] WorldToPixel: not initialized");
            return false;
        }
        
        // Преобразуем в локальные координаты
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        Debug.Log($"[MoldSurface] WorldToPixel: localPoint={localPoint}");
        
        // Получаем размеры спрайта в единицах мира
        float spriteWidth = spriteRenderer.sprite.bounds.size.x;
        float spriteHeight = spriteRenderer.sprite.bounds.size.y;
        Debug.Log($"[MoldSurface] WorldToPixel: sprite bounds=({spriteWidth}, {spriteHeight}), texture size=({runtimeTex.width}, {runtimeTex.height})");
        
        // Преобразуем в UV координаты (0-1)
        float u = (localPoint.x + spriteWidth * 0.5f) / spriteWidth;
        float v = (localPoint.y + spriteHeight * 0.5f) / spriteHeight;
        Debug.Log($"[MoldSurface] WorldToPixel: UV=({u:F3}, {v:F3})");
        
        // Если UV координаты выходят за границы, обрезаем их
        u = Mathf.Clamp01(u);
        v = Mathf.Clamp01(v);
        Debug.Log($"[MoldSurface] WorldToPixel: Clamped UV=({u:F3}, {v:F3})");
        
        // Преобразуем в пиксельные координаты
        px = Mathf.RoundToInt(u * runtimeTex.width);
        py = Mathf.RoundToInt(v * runtimeTex.height);
        Debug.Log($"[MoldSurface] WorldToPixel: pixel=({px}, {py})");
        
        // Проверяем границы
        bool inBounds = px >= 0 && px < runtimeTex.width && py >= 0 && py < runtimeTex.height;
        Debug.Log($"[MoldSurface] WorldToPixel: inBounds={inBounds}");
        
        return inBounds;
    }
    
    /// <summary>
    /// Возвращает процент очищенных пикселей
    /// </summary>
    public float GetCleanPercent()
    {
        if (!isInitialized) return 0f;
        
        Color32[] currentPixels = runtimeTex.GetPixels32();
        int cleanPixels = 0;
        int totalPixels = currentPixels.Length;
        
        for (int i = 0; i < totalPixels; i++)
        {
            if (currentPixels[i].a < cleanThreshold * 255)
            {
                cleanPixels++;
            }
        }
        
        return (float)cleanPixels / totalPixels;
    }
    
    /// <summary>
    /// Проверяет процент очистки и скрывает плесень при достижении порога
    /// </summary>
    private void CheckAndHideIfClean()
    {
        float cleanPercent = GetCleanPercent();
        
        if (cleanPercent >= disappearThreshold)
        {
            Debug.Log($"[MoldSurface] Плесень очищена на {cleanPercent:P1}, скрываем объект");
            HideMold();
        }
    }
    
    /// <summary>
    /// Скрывает плесень (устанавливает opacity в 0)
    /// </summary>
    private void HideMold()
    {
        if (spriteRenderer != null)
        {
            Color currentColor = spriteRenderer.color;
            currentColor.a = 0f; // Устанавливаем прозрачность в 0
            spriteRenderer.color = currentColor;
            
            Debug.Log($"[MoldSurface] Плесень {gameObject.name} скрыта (opacity = 0)");
        }
        
        // Отключаем коллайдер, чтобы с плесенью нельзя было взаимодействовать
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
            Debug.Log($"[MoldSurface] Коллайдер плесени отключен");
        }
        
        // Отключаем MoldInteractable
        MoldInteractable moldInteractable = GetComponent<MoldInteractable>();
        if (moldInteractable != null)
        {
            moldInteractable.enabled = false;
            Debug.Log($"[MoldSurface] MoldInteractable отключен");
        }
    }
    
    /// <summary>
    /// Публичный метод для проверки, скрыта ли плесень
    /// </summary>
    public bool IsHidden()
    {
        return spriteRenderer != null && spriteRenderer.color.a <= 0.01f;
    }
    
    private void OnDestroy()
    {
        if (runtimeTex != null)
        {
            DestroyImmediate(runtimeTex);
        }
    }
}
