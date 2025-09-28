using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Простая система голубого следа-кисти с прозрачностью.
/// Создает эффект "мокрой кисти" в Paint с постепенным исчезновением.
/// </summary>
public class WaterBrushTrail : MonoBehaviour
{
    [Header("Brush Settings")]
    [SerializeField] private float brushSize = 0.5f;
    [SerializeField] private float trailLifetime = 3f;
    [SerializeField] private float fadeSpeed = 1f;
    [SerializeField] private int maxTrails = 20;
    [SerializeField] private float minDistance = 0.05f;
    
    [Header("Visual Settings")]
    [SerializeField] private Color brushColor = new Color(0.5f, 0.8f, 1f, 0.4f); // Голубой полупрозрачный
    [SerializeField] private AnimationCurve alphaOverTime = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    [SerializeField] private AnimationCurve sizeOverTime = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.2f);
    
    private List<BrushTrail> activeTrails = new List<BrushTrail>();
    private Vector3 lastPosition;
    private bool isFirstTrail = true;
    
    [System.Serializable]
    private class BrushTrail
    {
        public GameObject gameObject;
        public SpriteRenderer spriteRenderer;
        public float lifetime;
        public float maxLifetime;
        public Vector3 position;
        public float size;
        
        public BrushTrail(GameObject obj, float maxLife, Vector3 pos, float trailSize)
        {
            gameObject = obj;
            spriteRenderer = obj.GetComponent<SpriteRenderer>();
            maxLifetime = maxLife;
            lifetime = maxLife;
            position = pos;
            size = trailSize;
        }
    }
    
    private void Start()
    {
        // Создаем префаб следа
        CreateTrailPrefab();
    }
    
    private void CreateTrailPrefab()
    {
        // Создаем простой круглый спрайт для следа
        GameObject prefab = new GameObject("BrushTrailPrefab");
        
        // Создаем текстуру для следа
        Texture2D trailTexture = CreateBrushTexture();
        
        // Создаем спрайт
        Sprite trailSprite = Sprite.Create(
            trailTexture,
            new Rect(0, 0, trailTexture.width, trailTexture.height),
            new Vector2(0.5f, 0.5f),
            100f
        );
        
        // Добавляем SpriteRenderer
        SpriteRenderer renderer = prefab.AddComponent<SpriteRenderer>();
        renderer.sprite = trailSprite;
        renderer.color = brushColor;
        renderer.sortingOrder = 5; // Поверх плесени
        
        // Сохраняем префаб
        trailPrefab = prefab;
        trailPrefab.SetActive(false);
    }
    
    private GameObject trailPrefab;
    
    private Texture2D CreateBrushTexture()
    {
        int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.45f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(1f - (distance / radius));
                alpha = Mathf.Pow(alpha, 1.5f); // Мягкие края
                
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return texture;
    }
    
    /// <summary>
    /// Добавляет след кисти в указанную позицию
    /// </summary>
    public void AddBrushTrail(Vector3 worldPosition)
    {
        // Проверяем расстояние от последнего следа
        if (!isFirstTrail)
        {
            float distance = Vector3.Distance(worldPosition, lastPosition);
            if (distance < minDistance)
            {
                return; // Слишком близко к предыдущему следу
            }
        }
        
        // Ограничиваем количество активных следов
        if (activeTrails.Count >= maxTrails)
        {
            RemoveOldestTrail();
        }
        
        // Создаем новый след
        GameObject trailObj = Instantiate(trailPrefab, worldPosition, Quaternion.identity);
        trailObj.SetActive(true);
        
        // Случайный размер для разнообразия
        float randomSize = brushSize * Random.Range(0.8f, 1.2f);
        trailObj.transform.localScale = Vector3.one * randomSize;
        
        BrushTrail trail = new BrushTrail(trailObj, trailLifetime, worldPosition, randomSize);
        activeTrails.Add(trail);
        
        lastPosition = worldPosition;
        isFirstTrail = false;
    }
    
    /// <summary>
    /// Очищает все следы
    /// </summary>
    public void ClearTrails()
    {
        foreach (BrushTrail trail in activeTrails)
        {
            if (trail.gameObject != null)
            {
                DestroyImmediate(trail.gameObject);
            }
        }
        activeTrails.Clear();
        isFirstTrail = true;
    }
    
    private void RemoveOldestTrail()
    {
        if (activeTrails.Count > 0)
        {
            BrushTrail oldestTrail = activeTrails[0];
            if (oldestTrail.gameObject != null)
            {
                DestroyImmediate(oldestTrail.gameObject);
            }
            activeTrails.RemoveAt(0);
        }
    }
    
    private void Update()
    {
        // Обновляем все активные следы
        for (int i = activeTrails.Count - 1; i >= 0; i--)
        {
            BrushTrail trail = activeTrails[i];
            
            if (trail.gameObject == null)
            {
                activeTrails.RemoveAt(i);
                continue;
            }
            
            // Уменьшаем время жизни
            trail.lifetime -= Time.deltaTime * fadeSpeed;
            
            if (trail.lifetime <= 0f)
            {
                // Удаляем след
                DestroyImmediate(trail.gameObject);
                activeTrails.RemoveAt(i);
            }
            else
            {
                // Обновляем визуальные свойства
                float normalizedTime = 1f - (trail.lifetime / trail.maxLifetime);
                
                if (trail.spriteRenderer != null)
                {
                    Color currentColor = brushColor;
                    currentColor.a *= alphaOverTime.Evaluate(normalizedTime);
                    trail.spriteRenderer.color = currentColor;
                    
                    float currentSize = trail.size * sizeOverTime.Evaluate(normalizedTime);
                    trail.gameObject.transform.localScale = Vector3.one * currentSize;
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        ClearTrails();
    }
}
